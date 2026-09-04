using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MyApplication.Client.Clients;
using Shared;
using Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();
builder.Services.AddScoped<CommonFunctionServices>();
builder.Services.AddScoped<VariablesService>();
builder.Services.AddScoped<RestaurantService>();
builder.Services.AddScoped<JsInterop>();
var myApplicationApiUrl = "http://localhost:5203/";

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(myApplicationApiUrl)
});
builder.Services.AddScoped<JournalAuthService>();
builder.Services.AddScoped(sp => new RestaurantsClient(new HttpClient
{
    BaseAddress = new Uri(myApplicationApiUrl)
}));
builder.Services.AddScoped(sp => new RestaurantTypesClient(new HttpClient
{
    BaseAddress = new Uri(myApplicationApiUrl)
}));
builder.Services.AddScoped(sp => new RestaurantTagsClient(new HttpClient
{
    BaseAddress = new Uri(myApplicationApiUrl)
}));
builder.Services.AddScoped(sp => new DiariesClient(new HttpClient
{
    BaseAddress = new Uri(myApplicationApiUrl)
}));
builder.Services.AddScoped(sp => new RestaurantDiariesClient(new HttpClient
{
    BaseAddress = new Uri(myApplicationApiUrl)
}));
builder.Services.AddScoped(sp => new QuotesClient(new HttpClient
{
    BaseAddress = new Uri(myApplicationApiUrl)
}));

await builder.Build().RunAsync();
