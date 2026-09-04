using Microsoft.EntityFrameworkCore;
using MyApplication.Api.Data;
using MyApplication.Api.Dtos;
using MyApplication.Api.Entities;

namespace MyApplication.Api.Endpoints;

public static class TagsEndpoints
{
    private const string GetTagEndpointName = "GetTag";

    public static RouteGroupBuilder MapTagsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("tags");

        group.MapGet("/", async (Guid userId, DineLogContext dbContext) =>
        {
            if (userId == Guid.Empty)
            {
                return Results.BadRequest(new { message = "A valid user is required." });
            }

            var tags = await dbContext.RestaurantTags
                .Where(tag => tag.UserId == userId)
                .OrderBy(tag => tag.Name)
                .Select(tag => ToDto(tag))
                .AsNoTracking()
                .ToListAsync();

            return Results.Ok(tags);
        });

        group.MapGet("/{id:guid}", async (Guid id, Guid userId, DineLogContext dbContext) =>
        {
            var tag = await dbContext.RestaurantTags
                .AsNoTracking()
                .FirstOrDefaultAsync(tag => tag.Id == id && tag.UserId == userId);

            return tag is null ? Results.NotFound(new { message = "Tag not found." }) : Results.Ok(ToDto(tag));
        })
        .WithName(GetTagEndpointName);

        group.MapPost("/", async (CreateTagDto newTag, DineLogContext dbContext) =>
        {
            if (!await dbContext.Users.AnyAsync(user => user.Id == newTag.UserId))
            {
                return Results.BadRequest(new { message = "A valid user is required." });
            }

            if (string.IsNullOrWhiteSpace(newTag.Name) || string.IsNullOrWhiteSpace(newTag.Category))
            {
                return Results.BadRequest(new { message = "Tag name and category are required." });
            }

            var tag = ToEntity(newTag);
            await dbContext.RestaurantTags.AddAsync(tag);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(GetTagEndpointName, new { id = tag.Id, userId = tag.UserId }, ToDto(tag));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateTagDto updatedTag, DineLogContext dbContext) =>
        {
            var existingTag = await dbContext.RestaurantTags
                .FirstOrDefaultAsync(tag => tag.Id == id && tag.UserId == updatedTag.UserId);

            if (existingTag is null)
            {
                return Results.NotFound(new { message = "Tag not found." });
            }

            existingTag.Name = NormalizeTagName(updatedTag.Name);
            existingTag.Category = updatedTag.Category.Trim();
            existingTag.Description = updatedTag.Description.Trim();

            await dbContext.SaveChangesAsync();

            return Results.Ok(ToDto(existingTag));
        });

        group.MapDelete("/{id:guid}", async (Guid id, Guid userId, DineLogContext dbContext) =>
        {
            var deletedRows = await dbContext.RestaurantTags
                .Where(tag => tag.Id == id && tag.UserId == userId)
                .ExecuteDeleteAsync();

            return deletedRows > 0
                ? Results.NoContent()
                : Results.NotFound(new { message = "Tag not found." });
        });

        return group;
    }

    private static TagDto ToDto(RestaurantTag tag) => new()
    {
        Id = tag.Id,
        UserId = tag.UserId,
        Name = tag.Name,
        Category = tag.Category,
        Description = tag.Description
    };

    private static RestaurantTag ToEntity(CreateTagDto dto) => new()
    {
        Id = Guid.NewGuid(),
        UserId = dto.UserId,
        Name = NormalizeTagName(dto.Name),
        Category = dto.Category.Trim(),
        Description = dto.Description.Trim()
    };

    private static string NormalizeTagName(string value) =>
        value.Trim().ToLowerInvariant().Replace(' ', '-');
}
