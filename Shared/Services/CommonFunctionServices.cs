using Microsoft.JSInterop;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;
using System.Reflection;
using Timer = System.Timers.Timer;
using System.Net.Http.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Shared.Services
{
    public class CommonFunctionServices
    {
        IJSRuntime _jsRuntime;
        NavigationManager _navigationManager;

        public CommonFunctionServices(
            IJSRuntime jSRuntime,
            NavigationManager nManager
        )
        {
            _jsRuntime = jSRuntime;
            _navigationManager = nManager;
        }

        public void JSConsole(object? obj, string? title = null)
        {
            _jsRuntime.InvokeVoidAsync("console.log", $"{title ?? string.Empty} =>", obj);
        }

        public void GoToRoute(string routeStr)
        {
            if (string.IsNullOrWhiteSpace(routeStr)) return;
            _navigationManager.NavigateTo(routeStr);
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return true;
            }

            Regex regex = new Regex(@"^(?("")("".+?(?<!\\)""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-0-9a-zA-Z]*[0-9a-zA-Z]*\.)+[A-Za-z0-9][\-A-Z-a-z0-9]{0,22}[A-Za-z0-9]))$");

            return regex.IsMatch(email.ToString().Trim());
        }

        public bool IsValidURL(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            return Regex.IsMatch(url, @"^(http://|https://)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$");
        }
    
        public int GetWeekOfMonth(DateTime date)
        {
            int[] prefixes = new int[6] { 0, 1, 2, 3, 4, 5 };

            var day = date.Day;
            day -= ((int)date.DayOfWeek == 0 ? 6 : (int)date.DayOfWeek - 1);
            day += 7;
            return prefixes[0 | (day) / 7];
        }

        public string RemoveSpaces(string result)
        {
            return result.Replace(" ", "");
        }

        public async Task BrowserBack()
        {
            await _jsRuntime.InvokeVoidAsync("CommonJSFunctions.back");
        }   

    }
}
