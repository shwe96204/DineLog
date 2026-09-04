using Microsoft.EntityFrameworkCore;

using MyApplication.Api.Entities;

namespace MyApplication.Api.Data;

public class DineLogContext(DbContextOptions<DineLogContext> options): DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<DiaryFolder> Diaries => Set<DiaryFolder>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<RestaurantDiary> RestaurantDiaries => Set<RestaurantDiary>();
    public DbSet<RestaurantType> RestaurantTypes => Set<RestaurantType>();
    public DbSet<RestaurantTag> RestaurantTags => Set<RestaurantTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<RestaurantType>().HasData(
            new RestaurantType
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-4789-a012-3456789abcde"),
                Name = "Fine Dining",
                Description = "Upscale restaurants with refined service and curated menus."
            },
            new RestaurantType
            {
                Id = Guid.Parse("b2c3d4e5-f6a7-4890-b123-456789abcdef"),
                Name = "Casual Dining",
                Description = "Relaxed full-service restaurants for everyday meals."
            },
            new RestaurantType
            {
                Id = Guid.Parse("c3d4e5f6-a7b8-4901-c234-56789abcdef0"),
                Name = "Cafe",
                Description = "Coffee shops, bakeries, and light-meal spots."
            },
            new RestaurantType
            {
                Id = Guid.Parse("d4e5f6a7-b8c9-4012-d345-6789abcdef01"),
                Name = "Fast Food",
                Description = "Quick-service chains and counter-order venues."
            },
            new RestaurantType
            {
                Id = Guid.Parse("e5f6a7b8-c9d0-4123-e456-789abcdef012"),
                Name = "Street Food",
                Description = "Market stalls, food trucks, and local street vendors."
            });

        modelBuilder.Entity<Restaurant>()
            .HasOne(restaurant => restaurant.User)
            .WithMany(user => user.Restaurants)
            .HasForeignKey(restaurant => restaurant.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DiaryFolder>()
            .HasOne(diary => diary.User)
            .WithMany()
            .HasForeignKey(diary => diary.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Restaurant>()
            .HasOne(restaurant => restaurant.RestaurantType)
            .WithMany(type => type.Restaurants)
            .HasForeignKey(restaurant => restaurant.RestaurantTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RestaurantDiary>()
            .HasOne(entry => entry.Diary)
            .WithMany(diary => diary.RestaurantDiaries)
            .HasForeignKey(entry => entry.DiaryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RestaurantDiary>()
            .HasOne(entry => entry.Restaurant)
            .WithMany()
            .HasForeignKey(entry => entry.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RestaurantTag>()
            .HasOne(tag => tag.User)
            .WithMany(user => user.Tags)
            .HasForeignKey(tag => tag.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
