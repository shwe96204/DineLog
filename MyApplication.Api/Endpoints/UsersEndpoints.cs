using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApplication.Api.Data;
using MyApplication.Api.Dtos;
using MyApplication.Api.Entities;
using MyApplication.Api.Services;

namespace MyApplication.Api.Endpoints;

public static class UsersEndpoints
{
    private static readonly TimeSpan VerificationCodeLifetime = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan PasswordResetCodeLifetime = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan PasswordResetVerifiedLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromMinutes(1);

    public static RouteGroupBuilder MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("users");

        group.MapGet("/exists", async (string email, DineLogContext dbContext) =>
        {
            var normalizedEmail = NormalizeEmail(email);
            var exists = !string.IsNullOrWhiteSpace(normalizedEmail)
                && await dbContext.Users.AnyAsync(user => user.Email == normalizedEmail);

            return Results.Ok(new { exists });
        });

        group.MapPost("/register", async (
            RegisterUserDto newUser,
            DineLogContext dbContext,
            IPasswordHasher<AppUser> passwordHasher,
            IEmailSender emailSender) =>
        {
            if (string.IsNullOrWhiteSpace(newUser.FullName)
                || string.IsNullOrWhiteSpace(newUser.Email)
                || string.IsNullOrWhiteSpace(newUser.Password))
            {
                return Results.BadRequest(new { message = "Full name, email, and password are required." });
            }

            if (newUser.Password.Length < 6)
            {
                return Results.BadRequest(new { message = "Password must be at least 6 characters." });
            }

            var normalizedEmail = NormalizeEmail(newUser.Email);
            if (await dbContext.Users.AnyAsync(user => user.Email == normalizedEmail))
            {
                return Results.Conflict(new { message = "An account with this email already exists." });
            }

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                FullName = newUser.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHint = newUser.PasswordHint.Trim(),
                DateJoined = DateTime.UtcNow,
                ProfileImageDataUrl = string.Empty,
                ThemePreference = "light",
                IsEmailVerified = false
            };

            user.Password = passwordHasher.HashPassword(user, newUser.Password);

            var verificationCode = GenerateOneTimeCode();
            SetEmailVerificationCode(user, passwordHasher, verificationCode);

            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            var emailResult = await SendVerificationEmailAsync(emailSender, user, verificationCode);

            return Results.Ok(new
            {
                message = emailResult.Message,
                requiresVerification = true,
                email = user.Email,
                emailSent = emailResult.Sent,
                usedDevelopmentFallback = emailResult.UsedDevelopmentFallback
            });
        });

        group.MapPost("/verify-email", async (
            VerifyEmailDto request,
            DineLogContext dbContext,
            IPasswordHasher<AppUser> passwordHasher) =>
        {
            var normalizedEmail = NormalizeEmail(request.Email);
            var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Email == normalizedEmail);
            if (user is null)
            {
                return Results.NotFound(new { message = "No account was found for this email." });
            }

            if (user.IsEmailVerified)
            {
                return Results.Ok(ToDto(user));
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return Results.BadRequest(new { message = "Please enter the verification code." });
            }

            if (string.IsNullOrWhiteSpace(user.EmailVerificationCodeHash)
                || user.EmailVerificationExpiresAt is null
                || user.EmailVerificationExpiresAt < DateTime.UtcNow)
            {
                return Results.BadRequest(new { message = "This verification code has expired. Please request a new code." });
            }

            var verification = passwordHasher.VerifyHashedPassword(user, user.EmailVerificationCodeHash, request.Code.Trim());
            if (verification == PasswordVerificationResult.Failed)
            {
                return Results.BadRequest(new { message = "The verification code is incorrect." });
            }

            user.IsEmailVerified = true;
            ClearEmailVerificationCode(user);
            await dbContext.SaveChangesAsync();

            return Results.Ok(ToDto(user));
        });

        group.MapPost("/resend-verification", async (
            ResendVerificationDto request,
            DineLogContext dbContext,
            IPasswordHasher<AppUser> passwordHasher,
            IEmailSender emailSender) =>
        {
            var normalizedEmail = NormalizeEmail(request.Email);
            var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Email == normalizedEmail);
            if (user is null)
            {
                return Results.NotFound(new { message = "No account was found for this email." });
            }

            if (user.IsEmailVerified)
            {
                return Results.BadRequest(new { message = "This email is already verified." });
            }

            if (user.EmailVerificationSentAt is not null && user.EmailVerificationSentAt > DateTime.UtcNow.Subtract(ResendCooldown))
            {
                return Results.BadRequest(new { message = "Please wait a moment before requesting another code." });
            }

            var verificationCode = GenerateOneTimeCode();
            SetEmailVerificationCode(user, passwordHasher, verificationCode);
            await dbContext.SaveChangesAsync();

            var emailResult = await SendVerificationEmailAsync(emailSender, user, verificationCode);

            return Results.Ok(new
            {
                message = emailResult.Message,
                emailSent = emailResult.Sent,
                usedDevelopmentFallback = emailResult.UsedDevelopmentFallback
            });
        });

        group.MapPost("/login", async (
            LoginUserDto login,
            DineLogContext dbContext,
            IPasswordHasher<AppUser> passwordHasher) =>
        {
            var normalizedEmail = NormalizeEmail(login.Email);
            var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Email == normalizedEmail);

            if (user is null || !VerifyPassword(user, login.Password, passwordHasher, out var shouldRehash))
            {
                return Results.NotFound(new { message = "Invalid email or password." });
            }

            if (shouldRehash)
            {
                user.Password = passwordHasher.HashPassword(user, login.Password);
            }

            if (!user.IsEmailVerified)
            {
                if (shouldRehash)
                {
                    await dbContext.SaveChangesAsync();
                }

                return Results.Json(
                    new
                    {
                        message = "Please verify your email before signing in.",
                        requiresVerification = true,
                        email = user.Email
                    },
                    statusCode: StatusCodes.Status403Forbidden);
            }

            if (shouldRehash)
            {
                await dbContext.SaveChangesAsync();
            }

            return Results.Ok(ToDto(user));
        });

        group.MapPost("/forgot-password", async (
            ForgotPasswordDto request,
            DineLogContext dbContext,
            IPasswordHasher<AppUser> passwordHasher,
            IEmailSender emailSender) =>
        {
            var normalizedEmail = NormalizeEmail(request.Email);
            var genericResponse = Results.Ok(new
            {
                message = "If an account exists for this email, we sent a password reset code."
            });

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return genericResponse;
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Email == normalizedEmail);
            if (user is null)
            {
                return genericResponse;
            }

            if (user.PasswordResetSentAt is not null && user.PasswordResetSentAt > DateTime.UtcNow.Subtract(ResendCooldown))
            {
                return Results.BadRequest(new { message = "Please wait a moment before requesting another reset code." });
            }

            var resetCode = GenerateOneTimeCode();
            SetPasswordResetCode(user, passwordHasher, resetCode);
            await dbContext.SaveChangesAsync();

            var emailResult = await SendPasswordResetEmailAsync(emailSender, user, resetCode);

            return Results.Ok(new
            {
                message = emailResult.Sent
                    ? "If an account exists for this email, we sent a password reset code."
                    : emailResult.Message,
                emailSent = emailResult.Sent,
                usedDevelopmentFallback = emailResult.UsedDevelopmentFallback
            });
        });

        group.MapPost("/verify-password-reset-code", async (
            VerifyPasswordResetCodeDto request,
            DineLogContext dbContext,
            IPasswordHasher<AppUser> passwordHasher) =>
        {
            var normalizedEmail = NormalizeEmail(request.Email);
            var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Email == normalizedEmail);
            if (user is null)
            {
                return Results.BadRequest(new { message = "The reset code or email is invalid." });
            }

            if (string.IsNullOrWhiteSpace(user.PasswordResetCodeHash)
                || user.PasswordResetExpiresAt is null
                || user.PasswordResetExpiresAt < DateTime.UtcNow)
            {
                return Results.BadRequest(new { message = "This reset code has expired. Please request a new code." });
            }

            var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordResetCodeHash, request.Code.Trim());
            if (verification == PasswordVerificationResult.Failed)
            {
                return Results.BadRequest(new { message = "The reset code is incorrect." });
            }

            user.PasswordResetVerifiedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "OTP verified. You can set a new password now." });
        });

        group.MapPost("/reset-password", async (
            ResetPasswordDto request,
            DineLogContext dbContext,
            IPasswordHasher<AppUser> passwordHasher) =>
        {
            var normalizedEmail = NormalizeEmail(request.Email);
            var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Email == normalizedEmail);
            if (user is null)
            {
                return Results.BadRequest(new { message = "The reset code or email is invalid." });
            }

            if (request.NewPassword.Length < 6)
            {
                return Results.BadRequest(new { message = "Password must be at least 6 characters." });
            }

            if (user.PasswordResetVerifiedAt is null
                || user.PasswordResetVerifiedAt < DateTime.UtcNow.Subtract(PasswordResetVerifiedLifetime))
            {
                return Results.BadRequest(new { message = "Please verify the OTP before changing the password." });
            }

            user.Password = passwordHasher.HashPassword(user, request.NewPassword);
            ClearPasswordResetCode(user);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Your password has been reset successfully." });
        });

        group.MapGet("/{id:guid}", async (Guid id, DineLogContext dbContext) =>
        {
            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.Id == id);

            return user is null
                ? Results.NotFound(new { message = "User not found." })
                : Results.Ok(ToDto(user));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserProfileDto profile, DineLogContext dbContext) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(candidate => candidate.Id == id);
            if (user is null)
            {
                return Results.NotFound(new { message = "User not found." });
            }

            var normalizedEmail = NormalizeEmail(profile.Email);
            var duplicateEmail = await dbContext.Users.AnyAsync(candidate =>
                candidate.Id != id && candidate.Email == normalizedEmail);

            if (duplicateEmail)
            {
                return Results.Conflict(new { message = "Another account already uses this email." });
            }

            user.FullName = profile.FullName.Trim();
            user.Email = normalizedEmail;
            user.PasswordHint = profile.PasswordHint.Trim();
            user.ProfileImageDataUrl = profile.ProfileImageDataUrl ?? string.Empty;
            user.ThemePreference = NormalizeThemePreference(profile.ThemePreference);

            await dbContext.SaveChangesAsync();

            return Results.Ok(ToDto(user));
        });

        return group;
    }

    private static UserDto ToDto(AppUser user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        PasswordHint = user.PasswordHint,
        DateJoined = user.DateJoined,
        ProfileImageDataUrl = user.ProfileImageDataUrl,
        ThemePreference = NormalizeThemePreference(user.ThemePreference),
        IsEmailVerified = user.IsEmailVerified
    };

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static string NormalizeThemePreference(string? themePreference) =>
        string.Equals(themePreference, "dark", StringComparison.OrdinalIgnoreCase) ? "dark" : "light";

    private static string GenerateOneTimeCode() =>
        RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static void SetEmailVerificationCode(AppUser user, IPasswordHasher<AppUser> passwordHasher, string code)
    {
        user.EmailVerificationCodeHash = passwordHasher.HashPassword(user, code);
        user.EmailVerificationExpiresAt = DateTime.UtcNow.Add(VerificationCodeLifetime);
        user.EmailVerificationSentAt = DateTime.UtcNow;
    }

    private static void ClearEmailVerificationCode(AppUser user)
    {
        user.EmailVerificationCodeHash = string.Empty;
        user.EmailVerificationExpiresAt = null;
        user.EmailVerificationSentAt = null;
    }

    private static void SetPasswordResetCode(AppUser user, IPasswordHasher<AppUser> passwordHasher, string code)
    {
        user.PasswordResetCodeHash = passwordHasher.HashPassword(user, code);
        user.PasswordResetExpiresAt = DateTime.UtcNow.Add(PasswordResetCodeLifetime);
        user.PasswordResetSentAt = DateTime.UtcNow;
        user.PasswordResetVerifiedAt = null;
    }

    private static void ClearPasswordResetCode(AppUser user)
    {
        user.PasswordResetCodeHash = string.Empty;
        user.PasswordResetExpiresAt = null;
        user.PasswordResetSentAt = null;
        user.PasswordResetVerifiedAt = null;
    }

    private static bool VerifyPassword(
        AppUser user,
        string providedPassword,
        IPasswordHasher<AppUser> passwordHasher,
        out bool shouldRehash)
    {
        shouldRehash = false;

        PasswordVerificationResult hashedResult;
        try
        {
            hashedResult = passwordHasher.VerifyHashedPassword(user, user.Password, providedPassword);
        }
        catch (FormatException)
        {
            hashedResult = PasswordVerificationResult.Failed;
        }

        if (hashedResult == PasswordVerificationResult.Success)
        {
            return true;
        }

        if (hashedResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            shouldRehash = true;
            return true;
        }

        if (user.Password == providedPassword)
        {
            shouldRehash = true;
            return true;
        }

        return false;
    }

    private static Task<EmailSendResult> SendVerificationEmailAsync(IEmailSender emailSender, AppUser user, string code)
    {
        var subject = "Verify your DineLog application email";
        var htmlBody =
            $"<p>Hello {System.Net.WebUtility.HtmlEncode(user.FullName)},</p>" +
            $"<p>Your verification code is <strong>{code}</strong>.</p>" +
            "<p>This code expires in 1 minute.</p>";
        var textBody =
            $"Hello {user.FullName},\n\nYour verification code is {code}.\nThis code expires in 1 minute.";

        return emailSender.SendAsync(user.Email, subject, htmlBody, textBody);
    }

    private static Task<EmailSendResult> SendPasswordResetEmailAsync(IEmailSender emailSender, AppUser user, string code)
    {
        var subject = "Reset your DineLog application password";
        var htmlBody =
            $"<p>Hello {System.Net.WebUtility.HtmlEncode(user.FullName)},</p>" +
            $"<p>Your password reset code is <strong>{code}</strong>.</p>" +
            "<p>This code expires in 1 minute.</p>";
        var textBody =
            $"Hello {user.FullName},\n\nYour password reset code is {code}.\nThis code expires in 1 minute.";

        return emailSender.SendAsync(user.Email, subject, htmlBody, textBody);
    }
}
