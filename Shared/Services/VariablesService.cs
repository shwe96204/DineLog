/*
    version 1.0.0
    Descirption: 
    Develper: Shwe Wutt Hmone
    Modify Time : 2026-4-7
*/
using Microsoft.JSInterop;

namespace Shared.Services
{
    public class VariablesService
    {
        #region Globals
        public string asset = "asset";
        public string libFilePath = "lib";
        
        private bool isLoading; //for lodaing
        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                isLoading = value;
                Task.Run(() => _jsRuntime.InvokeVoidAsync("ScanQRCodeJS.ToggleLoading", isLoading));
            }
        }
    
        IJSRuntime _jsRuntime;
        public VariablesService(IJSRuntime jSRuntime)
        {
            _jsRuntime = jSRuntime;
        }

        #endregion
    }

}
