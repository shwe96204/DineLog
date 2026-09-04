using Microsoft.EntityFrameworkCore;
using MyApplication.Api.Data;
using MyApplication.Api.Dtos;
using MyApplication.Api.Entities;
using System.Text.Json;

namespace MyApplication.Api.Endpoints;

public static class RestaurantDiariesEndpoints
{
    private const string GetRestaurantDiaryEndpointName = "GetRestaurantDiary";

    public static RouteGroupBuilder MapRestaurantDiariesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("restaurant-diaries");

        group.MapGet("/", async (Guid userId, Guid diaryId, DineLogContext dbContext) =>
        {
            if (userId == Guid.Empty || diaryId == Guid.Empty)
            {
                return Results.BadRequest(new { message = "A valid user and diary are required." });
            }

            var entries = await dbContext.RestaurantDiaries
                .Where(entry => entry.DiaryId == diaryId && entry.Diary!.UserId == userId)
                .OrderByDescending(entry => entry.VisitedDate)
                .Select(entry => new RestaurantDiaryDto
                {
                    Id = entry.Id,
                    DiaryId = entry.DiaryId,
                    RestaurantId = entry.RestaurantId,
                    RestaurantTypeId = entry.Restaurant!.RestaurantTypeId,
                    RestaurantName = entry.Restaurant.Name,
                    RestaurantCity = entry.Restaurant.City,
                    RestaurantCountry = entry.Restaurant.Country,
                    OverallRating = entry.OverallRating,
                    TasteScore = entry.TasteScore,
                    ServiceScore = entry.ServiceScore,
                    CleanlinessScore = entry.CleanlinessScore,
                    AmbienceScore = entry.AmbienceScore,
                    VisitedDate = entry.VisitedDate,
                    Description = entry.Description,
                    Remark = entry.Remark,
                    Image = entry.Image,
                    DetailImage = entry.DetailImage,
                    TagIds = ParseTagIds(entry.TagIdsJson),
                    GalleryImages = ParseGalleryImages(entry.GalleryImagesJson)
                })
                .AsNoTracking()
                .ToListAsync();

            return Results.Ok(entries);
        });

        group.MapGet("/{id:guid}", async (Guid id, Guid userId, DineLogContext dbContext) =>
        {
            var entry = await dbContext.RestaurantDiaries
                .Include(item => item.Diary)
                .Include(item => item.Restaurant)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id && item.Diary!.UserId == userId);

            return entry is null
                ? Results.NotFound(new { message = "Restaurant diary entry not found." })
                : Results.Ok(ToDto(entry));
        })
        .WithName(GetRestaurantDiaryEndpointName);

        group.MapPost("/", async (CreateRestaurantDiaryDto newEntry, DineLogContext dbContext) =>
        {
            if (newEntry.DiaryId == Guid.Empty || newEntry.RestaurantId == Guid.Empty)
            {
                return Results.BadRequest(new { message = "Diary and restaurant are required." });
            }

            var diary = await dbContext.Diaries.FirstOrDefaultAsync(item => item.Id == newEntry.DiaryId);
            if (diary is null)
            {
                return Results.BadRequest(new { message = "The selected diary does not exist." });
            }

            var restaurant = await dbContext.Restaurants.FirstOrDefaultAsync(item =>
                item.Id == newEntry.RestaurantId && item.UserId == diary.UserId);

            if (restaurant is null)
            {
                return Results.BadRequest(new { message = "The selected restaurant is not available for this diary." });
            }

            var entry = new RestaurantDiary
            {
                Id = Guid.NewGuid(),
                DiaryId = newEntry.DiaryId,
                RestaurantId = newEntry.RestaurantId,
                OverallRating = newEntry.OverallRating,
                TasteScore = newEntry.TasteScore,
                ServiceScore = newEntry.ServiceScore,
                CleanlinessScore = newEntry.CleanlinessScore,
                AmbienceScore = newEntry.AmbienceScore,
                VisitedDate = newEntry.VisitedDate,
                Description = newEntry.Description.Trim(),
                Remark = newEntry.Remark.Trim(),
                Image = newEntry.Image ?? string.Empty,
                DetailImage = newEntry.DetailImage ?? string.Empty,
                TagIdsJson = SerializeTagIds(newEntry.TagIds),
                GalleryImagesJson = SerializeGalleryImages(newEntry.GalleryImages)
            };

            await dbContext.RestaurantDiaries.AddAsync(entry);
            await dbContext.SaveChangesAsync();

            entry.Restaurant = restaurant;
            return Results.CreatedAtRoute(GetRestaurantDiaryEndpointName, new { id = entry.Id, userId = diary.UserId }, ToDto(entry));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateRestaurantDiaryDto updatedEntry, DineLogContext dbContext) =>
        {
            var diary = await dbContext.Diaries.FirstOrDefaultAsync(item => item.Id == updatedEntry.DiaryId);
            if (diary is null)
            {
                return Results.BadRequest(new { message = "The selected diary does not exist." });
            }

            var existingEntry = await dbContext.RestaurantDiaries
                .Include(entry => entry.Diary)
                .Include(entry => entry.Restaurant)
                .FirstOrDefaultAsync(entry => entry.Id == id && entry.DiaryId == updatedEntry.DiaryId);

            if (existingEntry is null || existingEntry.Diary?.UserId != diary.UserId)
            {
                return Results.NotFound(new { message = "Restaurant diary entry not found." });
            }

            var restaurant = await dbContext.Restaurants.FirstOrDefaultAsync(item =>
                item.Id == updatedEntry.RestaurantId && item.UserId == diary.UserId);

            if (restaurant is null)
            {
                return Results.BadRequest(new { message = "The selected restaurant is not available for this diary." });
            }

            existingEntry.RestaurantId = updatedEntry.RestaurantId;
            existingEntry.OverallRating = updatedEntry.OverallRating;
            existingEntry.TasteScore = updatedEntry.TasteScore;
            existingEntry.ServiceScore = updatedEntry.ServiceScore;
            existingEntry.CleanlinessScore = updatedEntry.CleanlinessScore;
            existingEntry.AmbienceScore = updatedEntry.AmbienceScore;
            existingEntry.VisitedDate = updatedEntry.VisitedDate;
            existingEntry.Description = updatedEntry.Description.Trim();
            existingEntry.Remark = updatedEntry.Remark.Trim();
            existingEntry.Image = updatedEntry.Image ?? string.Empty;
            existingEntry.DetailImage = updatedEntry.DetailImage ?? string.Empty;
            existingEntry.TagIdsJson = SerializeTagIds(updatedEntry.TagIds);
            existingEntry.GalleryImagesJson = SerializeGalleryImages(updatedEntry.GalleryImages);

            await dbContext.SaveChangesAsync();

            existingEntry.Restaurant = restaurant;
            return Results.Ok(ToDto(existingEntry));
        });

        group.MapDelete("/{id:guid}", async (Guid id, Guid userId, DineLogContext dbContext) =>
        {
            var entry = await dbContext.RestaurantDiaries
                .Include(item => item.Diary)
                .FirstOrDefaultAsync(item => item.Id == id && item.Diary!.UserId == userId);

            if (entry is null)
            {
                return Results.NotFound(new { message = "Restaurant diary entry not found." });
            }

            dbContext.RestaurantDiaries.Remove(entry);
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        return group;
    }

    private static RestaurantDiaryDto ToDto(RestaurantDiary entry) => new()
    {
        Id = entry.Id,
        DiaryId = entry.DiaryId,
        RestaurantId = entry.RestaurantId,
        RestaurantTypeId = entry.Restaurant?.RestaurantTypeId ?? Guid.Empty,
        RestaurantName = entry.Restaurant?.Name ?? string.Empty,
        RestaurantCity = entry.Restaurant?.City ?? string.Empty,
        RestaurantCountry = entry.Restaurant?.Country ?? string.Empty,
        OverallRating = entry.OverallRating,
        TasteScore = entry.TasteScore,
        ServiceScore = entry.ServiceScore,
        CleanlinessScore = entry.CleanlinessScore,
        AmbienceScore = entry.AmbienceScore,
        VisitedDate = entry.VisitedDate,
        Description = entry.Description,
        Remark = entry.Remark,
        Image = entry.Image,
        DetailImage = entry.DetailImage,
        TagIds = ParseTagIds(entry.TagIdsJson),
        GalleryImages = ParseGalleryImages(entry.GalleryImagesJson)
    };

    private static List<Guid> ParseTagIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<Guid>>(json) ?? [];
    }

    private static List<string> ParseGalleryImages(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }

    private static string SerializeGalleryImages(IEnumerable<string>? images) =>
        JsonSerializer.Serialize(images?.Where(image => !string.IsNullOrWhiteSpace(image)).ToList() ?? []);

    private static string SerializeTagIds(IEnumerable<Guid>? tagIds) =>
        JsonSerializer.Serialize(tagIds?.Where(tagId => tagId != Guid.Empty).Distinct().ToList() ?? []);
}
