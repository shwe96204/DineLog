using System.Net.Http.Json;
using Shared.Models;

namespace MyApplication.Api.Endpoints;

public static class QuotesEndpoints
{
    public static RouteGroupBuilder MapQuotesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("quotes");

        group.MapGet("/journal", async (IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
        {
            var quote = await GetJournalQuoteAsync(httpClientFactory.CreateClient(), cancellationToken);
            return quote is null
                ? Results.Ok(new JournalQuoteItem
                {
                    Content = "A meal remembered well is a story worth writing down.",
                    Author = "Dine Log"
                })
                : Results.Ok(quote);
        });

        return group;
    }

    private static async Task<JournalQuoteItem?> GetJournalQuoteAsync(HttpClient httpClient, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync("https://zenquotes.io/api/random", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<List<ZenQuoteResponse>>(cancellationToken: cancellationToken);
        var quote = payload?.FirstOrDefault();
        if (quote is null || string.IsNullOrWhiteSpace(quote.Q))
        {
            return null;
        }

        return new JournalQuoteItem
        {
            Content = quote.Q.Trim(),
            Author = string.IsNullOrWhiteSpace(quote.A) ? "Unknown" : quote.A.Trim()
        };
    }

    private sealed class ZenQuoteResponse
    {
        public string Q { get; set; } = string.Empty;
        public string A { get; set; } = string.Empty;
    }
}
