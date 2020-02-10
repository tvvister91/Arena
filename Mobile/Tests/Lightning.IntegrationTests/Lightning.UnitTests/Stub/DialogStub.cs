using System;
using System.Threading.Tasks;
using PE.Plugins.Dialogs;
using PE.Plugins.Dialogs.Enums;

namespace Lightning.IntegrationTests.Stub
{
    public class DialogStub : IDialogService
    {
        public string LastMessageShown { get; private set; }
        public string LastTitleShown { get; private set; }

        public Task AlertAsync(string message, string title, string button, Action onOk)
        {
            LastMessageShown = message;
            LastTitleShown = title;
            return Task.FromResult(0);
        }

        public void CancelAll()
        {
            throw new NotImplementedException();
        }

        public Task ConfirmAsync(string message, string title, string positiveButton, Action onPositive, string negativeButton, Action onNegative, bool hideOnClickOutside = false)
        {
            LastMessageShown = message;
            LastTitleShown = title;
            return Task.FromResult(0);
        }

        public Task DatePickerAsync(DateTime? date, Action<DateTime> onSet, Action onCancel)
        {
            throw new NotImplementedException();
        }

        public Task HideLoadingAsync()
        {
            throw new NotImplementedException();
        }

        public Task HideProgressAsync()
        {
            throw new NotImplementedException();
        }

        public Task PromptAsync(string message, string title, string affirmButton, string denyButton, Action<string> onAffirm, Action onDeny, bool password = false, string placeholder = "")
        {
            throw new NotImplementedException();
        }

        public Task PromptAsync(string message, string title, string affirmButton, string denyButton, Action<string> onAffirm, Action onDeny = null, string text = "", string placeholder = "", Func<string, string, bool> validationFunc = null)
        {
            throw new NotImplementedException();
        }

        public Task PromptPasswordAsync(string message, string title, string affirmButton, string denyButton, Action<string> onAffirm = null, Action onDeny = null, string placeholder = "")
        {
            throw new NotImplementedException();
        }

        public void ShowPrompt(string title, string message, string affirmButton, string denyButton, Action<string> onAffirm, Action onDeny, string text, string placeholder, Func<string, bool> affirmEnableFunc, bool isPassword)
        {
            throw new NotImplementedException();
        }

        public void ShowActionSheet(string title, string message, string cancel, string destructive, Action<string> callback, params string[] buttons)
        {
            throw new NotImplementedException();
        }

        public Task ShowLoadingAsync()
        {
            throw new NotImplementedException();
        }

        public Task ShowLoadingAsync(string message)
        {
            throw new NotImplementedException();
        }

        public Task ShowProgressAsync()
        {
            throw new NotImplementedException();
        }

        public Task ShowProgressAsync(string message)
        {
            throw new NotImplementedException();
        }

        public Task ShowProgressAsync(string message, float value)
        {
            throw new NotImplementedException();
        }

        public Task UpdateLoadingAsync(string message)
        {
            throw new NotImplementedException();
        }

        public Task UpdateProgressAsync(float value)
        {
            throw new NotImplementedException();
        }

        public Task UpdateProgressAsync(string message, float value)
        {
            throw new NotImplementedException();
        }

        public Task TimespanDialogWheelPicker(TimeSpan timeSpan, string title, string message, string affirmButton, string denyButton, Action<TimeSpan> onAffirm, Action onDeny = null)
        {
            throw new NotImplementedException();
        }

        public void ShowToast(string message, ToastLength length, PlatformCode platformCode = PlatformCode.All)
        {
            throw new NotImplementedException();
        }
    }
}
