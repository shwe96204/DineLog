using Microsoft.JSInterop;

namespace Shared.Helpers
{
    public class MessageUpdateInvokeHelper
    { 
        public Action<object> action;
        public MessageUpdateInvokeHelper(Action<object> action)
        {
            this.action = action;
        }

        [JSInvokable]
        public void UpdateMessageCaller(object e)
        {
            action.Invoke(e);
        }
    }

    public class MessageUpdateHelper
    {
        public Action<bool> action;
        public MessageUpdateHelper(Action<bool> action)
        {
            this.action = action;
        }

        [JSInvokable]
        public void UpdateCaller(bool e)
        {
            action.Invoke(e);
        }
    }

    public class OnEnterHelper
    {
        public Action<string, string> action;

        public OnEnterHelper(Action<string , string> action)
        {
            this.action = action;
        }

        [JSInvokable]
        public void OnKeyboardHandler(string keycode, string key)
        {
            action.Invoke(keycode, key);
        }

    }

    public class OnPrintHelper
    {
        public Action action;

        public OnPrintHelper(Action action)
        {
            this.action = action;
        }

        [JSInvokable]
        public void OnPrintCaller()
        {
            action.Invoke();
        }

    }

    public class OnPrintHelper2
    {
        public Action action;

        public OnPrintHelper2(Action action)
        {
            this.action = action;
        }

        [JSInvokable]
        public void OnPrintCaller2()
        {
            action.Invoke();
        }

    }

    public class QRScannerInvokeHelper
    {
        public Action<object> action;
        public QRScannerInvokeHelper(Action<object> action)
        {
            this.action = action;
        }

        [JSInvokable]
        public void UpdateQRScannerCaller(object e)
        {
            action.Invoke(e);
        }
    }
}
