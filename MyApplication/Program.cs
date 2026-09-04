using MudBlazor.Services;
using MyApplication.Client.Clients;
using MyApplication.Client.Pages;
using MyApplication.Components;
using Shared;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);
// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddScoped<CommonFunctionServices>();
builder.Services.AddScoped<VariablesService>();
builder.Services.AddScoped<JsInterop>();
builder.Services.AddScoped<RestaurantService>();

var myApplicationApiUrl = "http://localhost:5203/";
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(myApplicationApiUrl)
});
builder.Services.AddScoped<JournalAuthService>();
builder.Services.AddHttpClient<RestaurantsClient>(client =>
{
    client.BaseAddress = new Uri(myApplicationApiUrl);
});
builder.Services.AddHttpClient<RestaurantTypesClient>(client =>
{
    client.BaseAddress = new Uri(myApplicationApiUrl);
});
builder.Services.AddHttpClient<RestaurantTagsClient>(client =>
{
    client.BaseAddress = new Uri(myApplicationApiUrl);
});
builder.Services.AddHttpClient<DiariesClient>(client =>
{
    client.BaseAddress = new Uri(myApplicationApiUrl);
});
builder.Services.AddHttpClient<RestaurantDiariesClient>(client =>
{
    client.BaseAddress = new Uri(myApplicationApiUrl);
});
builder.Services.AddHttpClient<QuotesClient>(client =>
{
    client.BaseAddress = new Uri(myApplicationApiUrl);
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MyApplication.Client._Imports).Assembly);

app.Run();
