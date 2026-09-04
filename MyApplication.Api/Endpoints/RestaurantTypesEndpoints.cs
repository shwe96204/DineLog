using Microsoft.EntityFrameworkCore;
using MyApplication.Api.Data;
using MyApplication.Api.Dtos;

namespace MyApplication.Api.Endpoints;

public static class RestaurantTypesEndpoints
{
    public static RouteGroupBuilder MapRestaurantTypesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("restaurant-types");

        group.MapGet("/", async (DineLogContext dbContext) =>
            Results.Ok(await dbContext.RestaurantTypes
                .OrderBy(type => type.Name)
                .Select(type => new RestaurantTypeDto
                {
                    Id = type.Id,
                    Name = type.Name,
                    Description = type.Description
                })
                .AsNoTracking()
                .ToListAsync()));

        return group;
    }
}
