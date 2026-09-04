using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using Shared.Models;

namespace Shared.Services
{
    public class JournalAuthService
    {
        private const string SessionKey = "journal.session";
        private const string LastEmailKey = "journal.last-email";

        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public JournalAuthService(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public async Task<JournalSession?> GetSessionAsync()
        {
            var json = await GetItemAsync(SessionKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<JournalSession>(json, _jsonOptions);
        }

        public async Task<string?> GetLastEmailAsync() => await GetItemAsync(LastEmailKey);

        public async Task<bool> EmailExistsAsync(string email)
        {
            var normalizedEmail = email.Trim();
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return false;
            }

            var response = await _httpClient.GetFromJsonAsync<EmailExistsResponse>($"users/exists?email={Uri.EscapeDataString(normalizedEmail)}");
            return response?.Exists ?? false;
        }

        public async Task<(bool Success, string Message, bool RequiresVerification, string Email)> RegisterAsync(JournalUser user)
        {
            var response = await _httpClient.PostAsJsonAsync("users/register", new
            {
                user.FullName,
                user.Email,
                user.Password,
                user.PasswordHint
            });

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ReadErrorMessageAsync(response, "Unable to create the account."), false, user.Email);
            }

            var createdUser = await response.Content.ReadFromJsonAsync<PendingAuthResponse>(_jsonOptions);
            if (createdUser is null)
            {
                return (false, "Unable to read the account response.", false, user.Email);
            }

            await SetItemAsync(LastEmailKey, createdUser.Email);

            return (true, createdUser.Message, createdUser.RequiresVerification, createdUser.Email);
        }

        public async Task<(bool Success, string Message, bool RequiresVerification, string Email)> LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("users/login", new
            {
                Email = email,
                Password = password
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<PendingAuthResponse>(_jsonOptions);
                return (
                    false,
                    error?.Message ?? await ReadErrorMessageAsync(response, "Email or password is incorrect."),
                    error?.RequiresVerification ?? false,
                    error?.Email ?? email);
            }

            var user = await response.Content.ReadFromJsonAsync<JournalUser>(_jsonOptions);
            if (user is null)
            {
                return (false, "Unable to load the account details.", false, email);
            }

            await SaveSessionAsync(user);
            await SetItemAsync(LastEmailKey, user.Email);

            return (true, $"Welcome back, {user.FullName}.", false, user.Email);
        }

        public async Task<(bool Success, string Message)> ResendVerificationAsync(string email)
        {
            var response = await _httpClient.PostAsJsonAsync("users/resend-verification", new
            {
                Email = email
            });

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ReadErrorMessageAsync(response, "Unable to resend the verification code."));
            }

            await SetItemAsync(LastEmailKey, email.Trim());

            var body = await response.Content.ReadFromJsonAsync<ApiMessageResponse>(_jsonOptions);
            return (true, body?.Message ?? "A new verification code has been sent.");
        }

        public async Task<(bool Success, string Message)> VerifyEmailAsync(string email, string code)
        {
            var response = await _httpClient.PostAsJsonAsync("users/verify-email", new
            {
                Email = email,
                Code = code
            });

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ReadErrorMessageAsync(response, "Unable to verify the email."));
            }

            var user = await response.Content.ReadFromJsonAsync<JournalUser>(_jsonOptions);
            if (user is null)
            {
                return (false, "Unable to load the verified account.");
            }

            await SaveSessionAsync(user);
            await SetItemAsync(LastEmailKey, user.Email);

            return (true, "Your email has been verified.");
        }

        public async Task<(bool Success, string Message)> RequestPasswordResetAsync(string email)
        {
            var response = await _httpClient.PostAsJsonAsync("users/forgot-password", new
            {
                Email = email
            });

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ReadErrorMessageAsync(response, "Unable to send the password reset code."));
            }

            await SetItemAsync(LastEmailKey, email.Trim());

            var body = await response.Content.ReadFromJsonAsync<ApiMessageResponse>(_jsonOptions);
            return (true, body?.Message ?? "If an account exists for this email, we sent a password reset code.");
        }

        public async Task<(bool Success, string Message)> VerifyPasswordResetCodeAsync(string email, string code)
        {
            var response = await _httpClient.PostAsJsonAsync("users/verify-password-reset-code", new
            {
                Email = email,
                Code = code
            });

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ReadErrorMessageAsync(response, "Unable to verify the reset code."));
            }

            var body = await response.Content.ReadFromJsonAsync<ApiMessageResponse>(_jsonOptions);
            return (true, body?.Message ?? "OTP verified. You can set a new password now.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string newPassword)
        {
            var response = await _httpClient.PostAsJsonAsync("users/reset-password", new
            {
                Email = email,
                NewPassword = newPassword
            });

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ReadErrorMessageAsync(response, "Unable to reset the password."));
            }

            await SetItemAsync(LastEmailKey, email.Trim());

            var body = await response.Content.ReadFromJsonAsync<ApiMessageResponse>(_jsonOptions);
            return (true, body?.Message ?? "Your password has been reset successfully.");
        }

        public async Task<JournalUser?> GetCurrentUserAsync()
        {
            var session = await GetSessionAsync();
            if (session is null)
            {
                return null;
            }

            return await _httpClient.GetFromJsonAsync<JournalUser>($"users/{session.Id}");
        }

        public async Task<(bool Success, string Message, JournalSession? Session, JournalUser? User)> UpdateProfileAsync(JournalUser profile)
        {
            var session = await GetSessionAsync();
            if (session is null)
            {
                return (false, "No active session found.", null, null);
            }

            var response = await _httpClient.PutAsJsonAsync($"users/{session.Id}", new
            {
                profile.FullName,
                profile.Email,
                profile.PasswordHint,
                profile.ProfileImageDataUrl,
                profile.ThemePreference
            });

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ReadErrorMessageAsync(response, "Unable to update the profile."), null, null);
            }

            var updatedUser = await response.Content.ReadFromJsonAsync<JournalUser>(_jsonOptions);
            if (updatedUser is null)
            {
                return (false, "Unable to read the updated profile.", null, null);
            }

            await SaveSessionAsync(updatedUser, session.LoggedInAt);
            await SetItemAsync(LastEmailKey, updatedUser.Email);

            return (true, "Profile updated successfully.", await GetSessionAsync(), updatedUser);
        }

        public async Task LogoutAsync() => await RemoveItemAsync(SessionKey);

        private Task SaveSessionAsync(JournalUser user, DateTime? loggedInAt = null)
        {
            var session = new JournalSession
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                DateJoined = user.DateJoined,
                LoggedInAt = loggedInAt ?? DateTime.Now,
                ThemePreference = user.ThemePreference,
                IsEmailVerified = user.IsEmailVerified
            };

            return SetItemAsync(SessionKey, JsonSerializer.Serialize(session, _jsonOptions)).AsTask();
        }

        private async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, string fallbackMessage)
        {
            if (response.StatusCode == HttpStatusCode.Conflict
                || response.StatusCode == HttpStatusCode.BadRequest
                || response.StatusCode == HttpStatusCode.Unauthorized
                || response.StatusCode == HttpStatusCode.NotFound
                || response.StatusCode == HttpStatusCode.Forbidden)
            {
                var body = await response.Content.ReadFromJsonAsync<ApiMessageResponse>(_jsonOptions);
                if (!string.IsNullOrWhiteSpace(body?.Message))
                {
                    return body.Message;
                }
            }

            return fallbackMessage;
        }

        private ValueTask<string?> GetItemAsync(string key) =>
            _jsRuntime.InvokeAsync<string?>("journalAuthStorage.get", key);

        private ValueTask SetItemAsync(string key, string value) =>
            _jsRuntime.InvokeVoidAsync("journalAuthStorage.set", key, value);

        private ValueTask RemoveItemAsync(string key) =>
            _jsRuntime.InvokeVoidAsync("journalAuthStorage.remove", key);

        private class ApiMessageResponse
        {
            public string Message { get; set; } = string.Empty;
        }

        private sealed class EmailExistsResponse
        {
            public bool Exists { get; set; }
        }

        private sealed class PendingAuthResponse : ApiMessageResponse
        {
            public bool RequiresVerification { get; set; }
            public string Email { get; set; } = string.Empty;
        }
    }
}
