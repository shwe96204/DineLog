using Microsoft.EntityFrameworkCore;
using MyApplication.Api.Data;
using MyApplication.Api.Dtos;
using MyApplication.Api.Entities;

namespace MyApplication.Api.Endpoints;

public static class DiariesEndpoints
{
    private const string GetDiaryEndpointName = "GetDiary";

    public static RouteGroupBuilder MapDiariesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("diaries");

        group.MapGet("/", async (Guid userId, DineLogContext dbContext) =>
        {
            if (userId == Guid.Empty)
            {
                return Results.BadRequest(new { message = "A valid user is required." });
            }

            var diaries = await dbContext.Diaries
                .Where(diary => diary.UserId == userId)
                .OrderBy(diary => diary.DateCreated)
                .Select(diary => new DiaryDto
                {
                    Id = diary.Id,
                    UserId = diary.UserId,
                    DiaryName = diary.DiaryName,
                    Color = diary.Color,
                    DateCreated = diary.DateCreated,
                    CoverImageDataUrl = diary.CoverImage,
                    Description = diary.Description,
                    RestaurantDiaryCount = diary.RestaurantDiaries.Count
                })
                .AsNoTracking()
                .ToListAsync();

            return Results.Ok(diaries);
        });

        group.MapGet("/{id:guid}", async (Guid id, Guid userId, DineLogContext dbContext) =>
        {
            var diary = await dbContext.Diaries
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

            if (diary is null)
            {
                return Results.NotFound(new { message = "Diary not found." });
            }

            var diaryCount = await dbContext.RestaurantDiaries.CountAsync(entry => entry.DiaryId == diary.Id);
            return Results.Ok(ToDto(diary, diaryCount));
        })
        .WithName(GetDiaryEndpointName);

        group.MapPost("/", async (CreateDiaryDto newDiary, DineLogContext dbContext) =>
        {
            if (!await dbContext.Users.AnyAsync(user => user.Id == newDiary.UserId))
            {
                return Results.BadRequest(new { message = "A valid user is required." });
            }

            if (string.IsNullOrWhiteSpace(newDiary.DiaryName))
            {
                return Results.BadRequest(new { message = "Diary name is required." });
            }

            var diary = new DiaryFolder
            {
                Id = Guid.NewGuid(),
                UserId = newDiary.UserId,
                DiaryName = newDiary.DiaryName.Trim(),
                Color = string.IsNullOrWhiteSpace(newDiary.Color) ? "#d19044" : newDiary.Color.Trim(),
                DateCreated = DateTime.Now,
                CoverImage = newDiary.CoverImageDataUrl ?? string.Empty,
                Description = newDiary.Description.Trim()
            };

            await dbContext.Diaries.AddAsync(diary);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(GetDiaryEndpointName, new { id = diary.Id, userId = diary.UserId }, ToDto(diary, 0));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateDiaryDto updatedDiary, DineLogContext dbContext) =>
        {
            var existingDiary = await dbContext.Diaries
                .Include(diary => diary.RestaurantDiaries)
                .FirstOrDefaultAsync(diary => diary.Id == id && diary.UserId == updatedDiary.UserId);

            if (existingDiary is null)
            {
                return Results.NotFound(new { message = "Diary not found." });
            }

            existingDiary.DiaryName = updatedDiary.DiaryName.Trim();
            existingDiary.Color = string.IsNullOrWhiteSpace(updatedDiary.Color) ? existingDiary.Color : updatedDiary.Color.Trim();
            existingDiary.CoverImage = updatedDiary.CoverImageDataUrl ?? string.Empty;
            existingDiary.Description = updatedDiary.Description.Trim();

            await dbContext.SaveChangesAsync();

            return Results.Ok(ToDto(existingDiary, existingDiary.RestaurantDiaries.Count));
        });

        group.MapDelete("/{id:guid}", async (Guid id, Guid userId, DineLogContext dbContext) =>
        {
            var deletedRows = await dbContext.Diaries
                .Where(diary => diary.Id == id && diary.UserId == userId)
                .ExecuteDeleteAsync();

            return deletedRows > 0
                ? Results.NoContent()
                : Results.NotFound(new { message = "Diary not found." });
        });

        return group;
    }

    private static DiaryDto ToDto(DiaryFolder diary, int diaryCount) => new()
    {
        Id = diary.Id,
        UserId = diary.UserId,
        DiaryName = diary.DiaryName,
        Color = diary.Color,
        DateCreated = diary.DateCreated,
        CoverImageDataUrl = diary.CoverImage,
        Description = diary.Description,
        RestaurantDiaryCount = diaryCount
    };
}
