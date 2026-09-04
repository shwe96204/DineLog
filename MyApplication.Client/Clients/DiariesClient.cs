using System.Net.Http.Json;
using Shared.Models;

namespace MyApplication.Client.Clients;

public class DiariesClient(HttpClient httpClient)
{
    public async Task<DiaryFolderItem[]> GetDiariesAsync(Guid userId) =>
        await httpClient.GetFromJsonAsync<DiaryFolderItem[]>($"diaries?userId={userId}") ?? [];

    public async Task<DiaryFolderItem?> GetDiaryAsync(Guid id, Guid userId) =>
        await httpClient.GetFromJsonAsync<DiaryFolderItem>($"diaries/{id}?userId={userId}");

    public async Task<DiaryFolderItem?> AddDiaryAsync(DiaryFolderItem diary)
    {
        var response = await httpClient.PostAsJsonAsync("diaries", diary);
        return await response.Content.ReadFromJsonAsync<DiaryFolderItem>();
    }

    public async Task<DiaryFolderItem?> UpdateDiaryAsync(DiaryFolderItem diary)
    {
        var response = await httpClient.PutAsJsonAsync($"diaries/{diary.Id}", diary);
        return await response.Content.ReadFromJsonAsync<DiaryFolderItem>();
    }

    public async Task DeleteDiaryAsync(Guid id, Guid userId) =>
        await httpClient.DeleteAsync($"diaries/{id}?userId={userId}");
}
