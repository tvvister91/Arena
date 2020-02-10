using System;
using System.Threading.Tasks;

using PE.Plugins.Dialogs.Enums;

namespace PE.Plugins.Dialogs
{
    public interface IDialogService
    {
        #region Loading 

        Task ShowLoadingAsync();

        Task ShowLoadingAsync(string message);

        Task UpdateLoadingAsync(string message);

        Task HideLoadingAsync();

        #endregion Loading

        #region Progress

        Task ShowProgressAsync();

        Task ShowProgressAsync(string message);

        Task ShowProgressAsync(string message, float value);

        Task UpdateProgressAsync(float value);

        Task UpdateProgressAsync(string message, float value);

        Task HideProgressAsync();

        #endregion Progress

        #region Toast

        void ShowToast(string message, ToastLength length, PlatformCode usingPlatforms = PlatformCode.All);

        #endregion Toast

        #region Alert

        Task AlertAsync(string message, string title, string button, Action onOk);

        #endregion Alert

        #region Confirm

        Task ConfirmAsync(string message, string title, string positiveButton, Action onPositive, string negativeButton, Action onNegative, bool hideOnClickOutside = false);

        #endregion Confirm

        #region Prompt

        [Obsolete("PromptAsync is deprecated, use instead PromShowPrompt")]
        Task PromptAsync(string message, string title, string affirmButton, string denyButton, Action<string> onAffirm, Action onDeny, bool password = false, string placeholder = "");

        [Obsolete("PromptPasswordAsync is deprecated, use instead PromShowPrompt")]
        Task PromptPasswordAsync(string message, string title,
                                 string affirmButton, string denyButton,
                                 Action<string> onAffirm = null, Action onDeny = null,
                                 string placeholder = "");

        [Obsolete("PromptAsync is deprecated, use instead ShowPrompt")]
        Task PromptAsync(string message, string title,
                         string affirmButton, string denyButton,
                         Action<string> onAffirm, Action onDeny = null,
                         string text = "", string placeholder = "",
                         Func<string, string, bool> validationFunc = null);

        void ShowPrompt(string title, string message,
                        string affirmButton, string denyButton,
                        Action<string> onAffirm, Action onDeny = null,
                        string text = "", string placeholder = "",
                        Func<string, bool> affirmEnableFunc = null,
                        bool isPassword = false);

        #endregion Prompt

        #region Action Sheet

        void ShowActionSheet(string title, string message, string cancel, string destructive, Action<string> callback, params string[] buttons);

        #endregion Action Sheet

        #region Date Picker

        Task DatePickerAsync(DateTime? date, Action<DateTime> onSet, Action onCancel);

        #endregion Date Picker

        #region Pickers

        Task TimespanDialogWheelPicker(
            TimeSpan timeSpan, 
            string title,
            string message,
            string affirmButton, 
            string denyButton,
            Action<TimeSpan> onAffirm,
            Action onDeny = null);

        #endregion

        #region Programatically Cancel Alerts

        void CancelAll();

        #endregion
    }
}
