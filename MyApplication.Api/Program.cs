using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MyApplication.Api.Data;
using MyApplication.Api.Endpoints;
using MyApplication.Api.Entities;
using MyApplication.Api.Options;
using MyApplication.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DineLog")
    ?? "Data Source=dine-log.db";

builder.Services.AddDbContext<DineLogContext>(options =>
{
    options.UseSqlite(connectionString);
});
builder.Services.AddHttpClient();
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(
                "https://localhost:7222",
                "http://localhost:5025",
                "https://localhost:7044",
                "http://localhost:5044");
    });
});

var app = builder.Build();

app.UseCors("frontend");

app.MapGet("/", () => Results.Ok(new { message = "MyApplication API is running." }));
app.MapDiariesEndpoints();
app.MapRestaurantDiariesEndpoints();
app.MapUsersEndpoints();
app.MapTagsEndpoints();
app.MapRestaurantTypesEndpoints();
app.MapRestaurantsEndpoints();
app.MapQuotesEndpoints();

app.InitializeDb();

app.Run();
