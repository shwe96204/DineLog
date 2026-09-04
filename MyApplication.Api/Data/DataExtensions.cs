using Microsoft.EntityFrameworkCore;
namespace MyApplication.Api.Data;
public static class DataExtensions
{
    public static void InitializeDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DineLogContext>();
        dbContext.Database.EnsureCreated();
        EnsureLegacySchemaCompatibility(dbContext);
    }

    private static void EnsureLegacySchemaCompatibility(DineLogContext dbContext)
    {
        dbContext.Database.OpenConnection();
        try
        {
            CreateUsersTableIfMissing(dbContext);
            CreateTagsTableIfMissing(dbContext);
            CreateDiariesTableIfMissing(dbContext);
            CreateRestaurantDiariesTableIfMissing(dbContext);
            AddUsersThemePreferenceColumnIfMissing(dbContext);
            AddUsersIsEmailVerifiedColumnIfMissing(dbContext);
            AddUsersEmailVerificationCodeHashColumnIfMissing(dbContext);
            AddUsersEmailVerificationExpiresAtColumnIfMissing(dbContext);
            AddUsersEmailVerificationSentAtColumnIfMissing(dbContext);
            AddUsersPasswordResetCodeHashColumnIfMissing(dbContext);
            AddUsersPasswordResetExpiresAtColumnIfMissing(dbContext);
            AddUsersPasswordResetSentAtColumnIfMissing(dbContext);
            AddUsersPasswordResetVerifiedAtColumnIfMissing(dbContext);
            AddRestaurantsUserIdColumnIfMissing(dbContext);
            AddRestaurantDiaryTagIdsColumnIfMissing(dbContext);
            AddRestaurantDiaryGalleryImagesColumnIfMissing(dbContext);
            AddRestaurantDiaryServiceScoreColumnIfMissing(dbContext);
            AddRestaurantDiaryCleanlinessScoreColumnIfMissing(dbContext);
            AddRestaurantDiaryAmbienceScoreColumnIfMissing(dbContext);
        }
        finally
        {
            dbContext.Database.CloseConnection();
        }
    }

    private static void CreateUsersTableIfMissing(DineLogContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "Users" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
                "FullName" TEXT NOT NULL,
                "Email" TEXT NOT NULL,
                "Password" TEXT NOT NULL,
                "PasswordHint" TEXT NOT NULL,
                "DateJoined" TEXT NOT NULL,
                "ProfileImageDataUrl" TEXT NOT NULL,
                "ThemePreference" TEXT NOT NULL DEFAULT 'light',
                "IsEmailVerified" INTEGER NOT NULL DEFAULT 0,
                "EmailVerificationCodeHash" TEXT NOT NULL DEFAULT '',
                "EmailVerificationExpiresAt" TEXT NULL,
                "EmailVerificationSentAt" TEXT NULL,
                "PasswordResetCodeHash" TEXT NOT NULL DEFAULT '',
                "PasswordResetExpiresAt" TEXT NULL,
                "PasswordResetSentAt" TEXT NULL,
                "PasswordResetVerifiedAt" TEXT NULL
            );
            """);

        dbContext.Database.ExecuteSqlRaw("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");
            """);
    }

    private static void CreateTagsTableIfMissing(DineLogContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "RestaurantTags" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_RestaurantTags" PRIMARY KEY,
                "UserId" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "Category" TEXT NOT NULL,
                "Description" TEXT NOT NULL
            );
            """);

        dbContext.Database.ExecuteSqlRaw("""
            CREATE INDEX IF NOT EXISTS "IX_RestaurantTags_UserId" ON "RestaurantTags" ("UserId");
            """);
    }

    private static void CreateDiariesTableIfMissing(DineLogContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "Diaries" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Diaries" PRIMARY KEY,
                "UserId" TEXT NOT NULL,
                "DiaryName" TEXT NOT NULL,
                "Color" TEXT NOT NULL,
                "DateCreated" TEXT NOT NULL,
                "CoverImage" TEXT NOT NULL,
                "Description" TEXT NOT NULL
            );
            """);

        dbContext.Database.ExecuteSqlRaw("""
            CREATE INDEX IF NOT EXISTS "IX_Diaries_UserId" ON "Diaries" ("UserId");
            """);
    }

    private static void CreateRestaurantDiariesTableIfMissing(DineLogContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "RestaurantDiaries" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_RestaurantDiaries" PRIMARY KEY,
                "DiaryId" TEXT NOT NULL,
                "RestaurantId" TEXT NOT NULL,
                "OverallRating" INTEGER NOT NULL,
                "TasteScore" INTEGER NOT NULL,
                "ServiceScore" INTEGER NOT NULL DEFAULT 0,
                "CleanlinessScore" INTEGER NOT NULL DEFAULT 0,
                "AmbienceScore" INTEGER NOT NULL DEFAULT 0,
                "VisitedDate" TEXT NULL,
                "Description" TEXT NOT NULL,
                "Remark" TEXT NOT NULL,
                "Image" TEXT NOT NULL,
                "DetailImage" TEXT NOT NULL
            );
            """);

        dbContext.Database.ExecuteSqlRaw("""
            CREATE INDEX IF NOT EXISTS "IX_RestaurantDiaries_DiaryId" ON "RestaurantDiaries" ("DiaryId");
            """);

        dbContext.Database.ExecuteSqlRaw("""
            CREATE INDEX IF NOT EXISTS "IX_RestaurantDiaries_RestaurantId" ON "RestaurantDiaries" ("RestaurantId");
            """);
    }

    private static void AddRestaurantsUserIdColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "Restaurants", "UserId"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "Restaurants" ADD COLUMN "UserId" TEXT NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
            """);

        dbContext.Database.ExecuteSqlRaw("""
            CREATE INDEX IF NOT EXISTS "IX_Restaurants_UserId" ON "Restaurants" ("UserId");
            """);
    }

    private static void AddUsersThemePreferenceColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "Users", "ThemePreference"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "Users" ADD COLUMN "ThemePreference" TEXT NOT NULL DEFAULT 'light';
            """);
    }

    private static void AddUsersIsEmailVerifiedColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "Users", "IsEmailVerified"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "Users" ADD COLUMN "IsEmailVerified" INTEGER NOT NULL DEFAULT 1;
            """);
    }

    private static void AddUsersEmailVerificationCodeHashColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "Users", "EmailVerificationCodeHash"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "Users" ADD COLUMN "EmailVerificationCodeHash" TEXT NOT NULL DEFAULT '';
            """);
    }

    private static void AddUsersEmailVerificationExpiresAtColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "Users", "EmailVerificationExpiresAt"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "Users" ADD COLUMN "EmailVerificationExpiresAt" TEXT NULL;
            """);
    }

    private static void AddUsersEmailVerificationSentAtColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "Users", "EmailVerificationSentAt"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "Users" ADD COLUMN "EmailVerificationSentAt" TEXT NULL;
            """);
    }

    private static void AddUsersPasswordResetCodeHashColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "Users", "PasswordResetCodeHash"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "Users" ADD COLUMN "PasswordResetCodeHash" TEXT NOT NULL DEFAULT '';
            """);
    }

    private static void AddUsersPasswordResetExpiresAtColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "Users", "PasswordResetExpiresAt"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "Users" ADD COLUMN "PasswordResetExpiresAt" TEXT NULL;
            """);
    }

    private static void AddUsersPasswordResetSentAtColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "Users", "PasswordResetSentAt"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "Users" ADD COLUMN "PasswordResetSentAt" TEXT NULL;
            """);
    }

    private static void AddUsersPasswordResetVerifiedAtColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "Users", "PasswordResetVerifiedAt"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "Users" ADD COLUMN "PasswordResetVerifiedAt" TEXT NULL;
            """);
    }

    private static void AddRestaurantDiaryGalleryImagesColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "RestaurantDiaries", "GalleryImagesJson"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "RestaurantDiaries" ADD COLUMN "GalleryImagesJson" TEXT NOT NULL DEFAULT '[]';
            """);
    }

    private static void AddRestaurantDiaryTagIdsColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "RestaurantDiaries", "TagIdsJson"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "RestaurantDiaries" ADD COLUMN "TagIdsJson" TEXT NOT NULL DEFAULT '[]';
            """);
    }

    private static void AddRestaurantDiaryServiceScoreColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "RestaurantDiaries", "ServiceScore"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "RestaurantDiaries" ADD COLUMN "ServiceScore" INTEGER NOT NULL DEFAULT 0;
            """);
    }

    private static void AddRestaurantDiaryCleanlinessScoreColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "RestaurantDiaries", "CleanlinessScore"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "RestaurantDiaries" ADD COLUMN "CleanlinessScore" INTEGER NOT NULL DEFAULT 0;
            """);
    }

    private static void AddRestaurantDiaryAmbienceScoreColumnIfMissing(DineLogContext dbContext)
    {
        if (ColumnExists(dbContext, "RestaurantDiaries", "AmbienceScore"))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw("""
            ALTER TABLE "RestaurantDiaries" ADD COLUMN "AmbienceScore" INTEGER NOT NULL DEFAULT 0;
            """);
    }

    private static bool ColumnExists(DineLogContext dbContext, string tableName, string columnName)
    {
        using var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{tableName}\");";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
