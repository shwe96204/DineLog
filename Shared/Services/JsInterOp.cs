using Microsoft.JSInterop;
using Shared.Services;

namespace Shared.Services
{
    // This class provides an example of how JavaScript functionality can be wrapped
    // in a .NET class for easy consumption. The associated JavaScript module is
    // loaded on demand when first needed.
    //
    // This class can be registered as scoped DI service and then injected into Blazor
    // components for use.

    public class JsInterop : IDisposable
    {
        IJSRuntime _jsRuntime;
        VariablesService Gva;
        private readonly Lazy<Task> BeforeLoginModuleTask;
        private readonly Lazy<Task> AfterLoginModuleTask;

        public JsInterop(IJSRuntime jsRuntime, VariablesService variablesService)
        {
            _jsRuntime = jsRuntime;
            Gva = variablesService;
            BeforeLoginModuleTask = new Lazy<Task>(LoadMultiScriptsBeforeLoginAsync);
            AfterLoginModuleTask = new Lazy<Task>(LoadMultipleScriptAfterLoginAsync);
        }

        public async Task LoadMultiScriptsBeforeLoginAsync()
        {
            List<string> scripts = new List<string>
            {
              Gva.asset + "/js/animation.js",
              Gva.asset + "/css/login.css",
            };
            await _jsRuntime.InvokeVoidAsync("loadResources", scripts);
        }

        public async Task LoadMultipleScriptAfterLoginAsync()
        {
            List<string> scripts = new List<string>
            {
              Gva.asset + "/js/animation.js",
              Gva.asset + "/css/Home.css",
              Gva.asset + "/css/journalStyle.css",
              Gva.asset + "/css/welcome.css",
              Gva.asset + "/js/ScanQRCode.js",
              Gva.libFilePath + "/html5-qrcode.min.js"

            };
            await _jsRuntime.InvokeVoidAsync("loadResources", scripts);
        }

        public async Task EnsureBeforeLoginScriptsLoadedAsync()
        {
            if (!BeforeLoginModuleTask.Value.IsCompleted)
            {
                await BeforeLoginModuleTask.Value;
            }
        }

        public async Task EnsureAfterLoginScriptLoadedAsync()
        {
            if (!AfterLoginModuleTask.Value.IsCompleted)
            {
                await AfterLoginModuleTask.Value;
            }
        }

        public void Dispose()
        {

        }
    }
}
