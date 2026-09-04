using System.Net.Http.Json;
using Shared.Models;

namespace MyApplication.Client.Clients;

public class QuotesClient(HttpClient httpClient)
{
    public async Task<JournalQuoteItem?> GetJournalQuoteAsync() =>
        await httpClient.GetFromJsonAsync<JournalQuoteItem>("quotes/journal");
}
