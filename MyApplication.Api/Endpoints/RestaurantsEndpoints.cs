using Microsoft.EntityFrameworkCore;
using MyApplication.Api.Data;
using MyApplication.Api.Dtos;
using MyApplication.Api.Entities;

namespace MyApplication.Api.Endpoints;

public static class RestaurantsEndpoints
{
    private const string GetRestaurantEndpointName = "GetRestaurant";

    public static RouteGroupBuilder MapRestaurantsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("restaurants");

        group.MapGet("/", async (Guid userId, DineLogContext dbContext) =>
        {
            if (userId == Guid.Empty)
            {
                return Results.BadRequest(new { message = "A valid user is required." });
            }

            return Results.Ok(await dbContext.Restaurants
                .Where(restaurant => restaurant.UserId == userId)
                .OrderBy(restaurant => restaurant.Name)
                .Select(restaurant => ToDto(restaurant))
                .AsNoTracking()
                .ToListAsync());
        });

        group.MapGet("/{id:guid}", async (Guid id, Guid userId, DineLogContext dbContext) =>
        {
            var restaurant = await dbContext.Restaurants
                .AsNoTracking()
                .FirstOrDefaultAsync(restaurant => restaurant.Id == id && restaurant.UserId == userId);

            return restaurant is null
                ? Results.NotFound(new { message = "Restaurant not found." })
                : Results.Ok(ToDto(restaurant));
        })
        .WithName(GetRestaurantEndpointName);

        group.MapPost("/", async (CreateRestaurantDto newRestaurant, DineLogContext dbContext) =>
        {
            if (string.IsNullOrWhiteSpace(newRestaurant.Name))
            {
                return Results.BadRequest("Restaurant name is required.");
            }

            if (!await dbContext.Users.AnyAsync(user => user.Id == newRestaurant.UserId))
            {
                return Results.BadRequest(new { message = "A valid user is required." });
            }

            var typeExists = await dbContext.RestaurantTypes
                .AnyAsync(type => type.Id == newRestaurant.RestaurantTypeId);

            if (!typeExists)
            {
                return Results.BadRequest("A valid restaurant type is required.");
            }

            var restaurant = ToEntity(newRestaurant);
            await dbContext.Restaurants.AddAsync(restaurant);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(GetRestaurantEndpointName, new { id = restaurant.Id }, ToDto(restaurant));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateRestaurantDto updatedRestaurant, DineLogContext dbContext) =>
        {
            var existingRestaurant = await dbContext.Restaurants
                .FirstOrDefaultAsync(restaurant => restaurant.Id == id && restaurant.UserId == updatedRestaurant.UserId);

            if (existingRestaurant is null)
            {
                return Results.NotFound(new { message = "Restaurant not found." });
            }

            var typeExists = await dbContext.RestaurantTypes
                .AnyAsync(type => type.Id == updatedRestaurant.RestaurantTypeId);

            if (!typeExists)
            {
                return Results.BadRequest("A valid restaurant type is required.");
            }

            dbContext.Entry(existingRestaurant)
                .CurrentValues
                .SetValues(ToEntity(id, updatedRestaurant));

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, Guid userId, DineLogContext dbContext) =>
        {
            var deletedRows = await dbContext.Restaurants
                .Where(restaurant => restaurant.Id == id && restaurant.UserId == userId)
                .ExecuteDeleteAsync();

            return deletedRows > 0
                ? Results.NoContent()
                : Results.NotFound(new { message = "Restaurant not found." });
        });

        return group;
    }

    private static RestaurantDto ToDto(Restaurant restaurant) => new()
    {
        Id = restaurant.Id,
        UserId = restaurant.UserId,
        RestaurantTypeId = restaurant.RestaurantTypeId,
        Name = restaurant.Name,
        Country = restaurant.Country,
        City = restaurant.City,
        Address = restaurant.Address,
        Description = restaurant.Description,
        Image = restaurant.Image,
        WebsiteUrl = restaurant.WebsiteUrl
    };

    private static Restaurant ToEntity(CreateRestaurantDto dto) => new()
    {
        Id = Guid.NewGuid(),
        UserId = dto.UserId,
        RestaurantTypeId = dto.RestaurantTypeId,
        Name = dto.Name.Trim(),
        Country = dto.Country.Trim(),
        City = dto.City.Trim(),
        Address = dto.Address.Trim(),
        Description = dto.Description.Trim(),
        Image = dto.Image.Trim(),
        WebsiteUrl = dto.WebsiteUrl.Trim()
    };

    private static Restaurant ToEntity(Guid id, UpdateRestaurantDto dto) => new()
    {
        Id = id,
        UserId = dto.UserId,
        RestaurantTypeId = dto.RestaurantTypeId,
        Name = dto.Name.Trim(),
        Country = dto.Country.Trim(),
        City = dto.City.Trim(),
        Address = dto.Address.Trim(),
        Description = dto.Description.Trim(),
        Image = dto.Image.Trim(),
        WebsiteUrl = dto.WebsiteUrl.Trim()
    };
}
