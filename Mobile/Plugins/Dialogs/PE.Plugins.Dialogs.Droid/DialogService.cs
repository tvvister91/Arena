using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using MvvmCross;
using static Android.Views.ViewGroup.LayoutParams;

using PE.Framework.Droid;
using PE.Framework.Droid.AndroidApp.AppVersion;
using MvvmCross.Platforms.Android;

namespace PE.Plugins.Dialogs.Droid
{
    public class DialogService : IDialogService
    {
        #region Fields

        private string _oldPromtText;
        private bool _canChangePromtText = true;
        private IProgressDialog _Progress;
        private readonly DialogConfig _Config;
        private Dialog _Dialog;
        private SoftInput? _oldInputMode;

        #endregion Fields

        #region Constructors

        public DialogService(DialogConfig config)
        {
            _Config = config;
        }

        #endregion Constructors

        #region Loading

        public async Task ShowLoadingAsync()
        {
            ShowLoadingAsync(string.Empty);
        }

        public async Task ShowLoadingAsync(string message)
        {
            if (_Progress == null)
            {
                _Progress = (_Config.CustomLoadingDialog != null) ? _Config.CustomLoadingDialog(message) : Loading(message, null, null, true);
            }
        }

        public async Task UpdateLoadingAsync(string message)
        {
            if (_Progress == null) return;
            _Progress.Title = message;
        }

        public async Task HideLoadingAsync()
        {
            if (_Progress == null) return;
            _Progress.Hide();
            _Progress.Dispose();
            _Progress = null;
        }

        #endregion Loading

        #region Progress

        public async Task ShowProgressAsync()
        {
            ShowProgressAsync(string.Empty);
        }

        public async Task ShowProgressAsync(string message)
        {
            if (_Progress == null)
            {
                _Progress = Progress(message, null, string.Empty, true, false);
            }

            _Progress.Title = message;
        }

        public async Task ShowProgressAsync(string message, float value)
        {
            if (_Progress == null)
            {
                _Progress = Progress(message, null, string.Empty, true, true);
            }

            _Progress.Title = message;
            _Progress.PercentComplete = (int) (value * 100);
        }

        public async Task UpdateProgressAsync(float value)
        {
            if (_Progress == null) return;
            _Progress.PercentComplete = (int) (value * 100);
        }

        public async Task UpdateProgressAsync(string message, float value)
        {
            if (_Progress == null) return;
            _Progress.PercentComplete = (int) (value * 100);
            _Progress.Title = message;
        }

        public async Task HideProgressAsync()
        {
            if (_Progress == null) return;
            _Progress.Hide();
            _Progress.Dispose();
            _Progress = null;
        }

        #endregion Progress

        #region Toast

        private static object _ToastLock = new object();
        private static bool _HasToast;

        public void ShowToast(string message, Enums.ToastLength length, Enums.PlatformCode usingPlatforms = Enums.PlatformCode.All)
        {
            if (!usingPlatforms.HasFlag(Enums.PlatformCode.Droid)) return;

            Utilities.Dispatch(async () => {
                var shouldWait = false;

                do
                {
                    lock (_ToastLock)
                    {
                        shouldWait = _HasToast;
                        _HasToast = true;
                    }

                    if (shouldWait) await Task.Delay(DialogConstants.TOAST_CHECK_NEXT_DELAY);
                }
                while (shouldWait);

                Toast.MakeText(Application.Context, message, length == Enums.ToastLength.Long ? ToastLength.Long : ToastLength.Short).Show();

                var toastTime = length == Enums.ToastLength.Long ? DialogConstants.TOAST_LONG_LENGTH : DialogConstants.TOAST_SHORT_LENGTH;
                await Task.Delay(toastTime / 2);

                lock (_ToastLock)
                {
                    _HasToast = false;
                }
            });
        }

        #endregion Toast

        #region Alert

        public async Task AlertAsync(string message, string title, string button, Action onOk)
        {
            Utilities.Dispatch(() =>
            {
                Context context = GetActivityContext();

                var txt = string.Format("\n{0}\n", message);

                AlertDialog.Builder builder = new AlertDialog.Builder(context);
                AlertDialog dialog = builder
                    .SetCancelable(false)
                    .SetMessage(txt)
                    .SetTitle(title)
                    .SetPositiveButton(button, (o, e) =>
                    {
                        if (onOk != null) onOk();
                        _Dialog = null;
                    }).Show();

                //dialog.Show();
            });
        }

        #endregion Alert

        #region Confirm

        public async Task ConfirmAsync(string message, string title, string positiveButton, Action onPositive, string negativeButton, Action onNegative, bool hideOnClickOutside = false)
        {
            Utilities.Dispatch(() =>
            {
                Context context = GetActivityContext();

                var txt = string.Format("\n{0}\n", message);

                _Dialog = new AlertDialog.Builder(context)
                    .SetCancelable(false)
                    .SetMessage(txt)
                    .SetTitle(title)
                    .SetPositiveButton(positiveButton, (o, e) =>
                    {
                        if (onPositive != null) onPositive();
                        _Dialog = null;
                    })
                    .SetNegativeButton(negativeButton, (o, e) =>
                    {
                        if (onNegative != null) onNegative();
                        _Dialog = null;
                    })
                    .Show();
            });
        }

        #endregion Confirm

        #region Prompt

        public async Task PromptAsync(string message, string title, string affirmButton, string denyButton, Action<string> onAffirm, Action onDeny, bool password = false, string placeholder = "")
        {
            await PromptAsync(message, title, affirmButton, denyButton, onAffirm, onDeny, string.Empty, placeholder, password, null);
        }

        public async Task PromptPasswordAsync(string message, string title,
            string affirmButton, string denyButton,
            Action<string> onAffirm = null, Action onDeny = null,
            string placeholder = "")
        {
            await PromptAsync(message, title, affirmButton, denyButton, onAffirm, onDeny, string.Empty, placeholder, true, null);
        }

        public async Task PromptAsync(string message, string title,
            string affirmButton, string denyButton,
            Action<string> onAffirm, Action onDeny = null,
            string text = "", string placeholder = "",
            Func<string, string, bool> validationFunc = null)
        {
            await PromptAsync(message, title, affirmButton, denyButton, onAffirm, onDeny, text, placeholder, false, validationFunc);
        }

        private async Task PromptAsync(string message, string title,
            string affirmButton, string denyButton,
            Action<string> onAffirm, Action onDeny = null,
            string text = "", string placeholder = "",
            bool password = false,
            Func<string, string, bool> validationFunc = null)
        {
            Utilities.Dispatch(() => {
                var context = GetActivityContext();
                var frame = new FrameLayout(context);
                var txt = new EditText(context) {
                    Text = text,
                    Hint = placeholder
                };

                if (password) txt.InputType = InputTypes.TextVariationPassword;

                txt.TextChanged += (sender, e) => {
                    if (validationFunc == null || !_canChangePromtText)
                    {
                        _canChangePromtText = true;

                        return;
                    }

                    if (!validationFunc(_oldPromtText, txt.Text))
                    {
                        _canChangePromtText = false;
                        txt.Text = _oldPromtText;
                    }
                    else
                    {
                        _oldPromtText = txt.Text;
                    }
                };
                frame.SetPadding(50, 0, 50, 0);
                frame.AddView(txt);

                _Dialog = new AlertDialog.Builder(context)
                    .SetMessage(message)
                    .SetTitle(title)
                    .SetView(frame)
                    .SetPositiveButton(affirmButton, (o, e) =>
                    {
                        HideKeyboard(txt);
                        onAffirm?.Invoke(txt.Text);
                    })
                    .SetNegativeButton(denyButton, (o, e) =>
                    {
                        HideKeyboard(txt);
                        onDeny?.Invoke();
                    })
                    .SetOnDismissListener(new RestoreSIMDismissHandler {
                        DialogService = this
                    })
                    .Show();

                ShowKeyboard(txt);
            });
        }

        public void ShowPrompt(string title, string message,
            string affirmButton, string denyButton,
            Action<string> onAffirm, Action onDeny = null,
            string text = "", string placeholder = "",
            Func<string, bool> affirmEnableFunc = null,
            bool isPassword = false)
        {
            Utilities.Dispatch(() => {
                var context = GetActivityContext();
                var frame = new FrameLayout(context);
                var txt = new EditText(context) {
                    Text = text,
                    Hint = placeholder
                };

                if (isPassword) txt.InputType = InputTypes.TextVariationPassword;

                frame.SetPadding(50, 0, 50, 0);
                frame.AddView(txt);


                _Dialog = new AlertDialog.Builder(context)
                    .SetMessage(message)
                    .SetTitle(title)
                    .SetView(frame)
                    .SetPositiveButton(affirmButton, (o, e) =>
                    {
                        HideKeyboard(txt);
                        onAffirm?.Invoke(txt.Text);
                    })
                    .SetNegativeButton(denyButton, (o, e) =>
                    {
                        HideKeyboard(txt);
                        onDeny?.Invoke();
                    })
                    .SetOnDismissListener(new RestoreSIMDismissHandler
                    {
                        DialogService = this
                    })
                    .Show();

                ShowKeyboard(txt);

                if (affirmEnableFunc != null && _Dialog is AlertDialog dlg)
                {
                    var positiveButton = dlg.GetButton((int) DialogButtonType.Positive);
                    positiveButton.Enabled = affirmEnableFunc(txt.Text);
                    txt.TextChanged += (sender, e) => positiveButton.Enabled = affirmEnableFunc(txt.Text);
                }
            });
        }

        #endregion Prompt

        public void ShowActionSheet(string title, string message, string cancel, string destructive, Action<string> callback, params string[] buttons)
            {
            Utilities.Dispatch(() => {
                var context = GetActivityContext();
                var dialog = new AlertDialog.Builder(context);

                if (!string.IsNullOrEmpty(title))
                {
                    dialog.SetTitle(title);
                }
                if (!string.IsNullOrEmpty(message))
                {
                    dialog.SetMessage(message);
                }

                var grid = new GridLayout(context) {
                    RowCount = buttons.Length,
                    ColumnCount = 1
                };
                grid.SetPadding(20, 0, 20, 30);

                var texts = new List<string>(buttons ?? new string[0]);
                texts.ForEach(text => grid.AddView(GetActionSheetButton(context, text, callback)));

                if (!string.IsNullOrEmpty(destructive))
                {
                    var destructiveButton = GetActionSheetButton(context, destructive, callback);
                    destructiveButton.SetTextColor(Color.Red);
                    grid.RowCount++;
                    grid.AddView(destructiveButton);
                }
                if (!string.IsNullOrEmpty(cancel))
                {
                    var cancelButton = GetActionSheetButton(context, cancel, callback);
                    cancelButton.SetTextColor(Color.Black);
                    grid.RowCount++;
                    grid.AddView(cancelButton);
                }

                dialog.SetView(grid);
                _Dialog = dialog.Show();
            });
        }

        private Button GetActionSheetButton(Context context, string text, Action<string> callback)
        {
            var button = new Button(context) {
                Text = text,
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            button.SetBackgroundColor(Color.White);
            button.SetTextColor(Color.LightSkyBlue);

            button.Click += (o, e) => {
                _Dialog.Cancel();
                callback?.Invoke(text);
            };

            return button;
        }

        #region Date Picker

        public async Task DatePickerAsync(DateTime? date, Action<DateTime> onSet, Action onCancel)
        {
            var dt = (date == null) ? DateTime.Now : date.Value;

            Utilities.Dispatch(() =>
            {
                var dlg = new DatePickerDialog(GetActivityContext(), (o, e) =>
                {
                    onSet?.Invoke(e.Date);
                }, dt.Year, dt.Month, dt.Day);
                dlg.Show();
            });
        }

        #endregion Date Picker

        #region Private

        private IProgressDialog Loading(string title, Action onCancel, string cancelText, bool show)
        {
            return Progress(title, onCancel, cancelText, show, false);
        }

        private IProgressDialog Progress(string title, Action onCancel, string cancelText, bool show, bool deterministic)
        {
            var dlg = new ProgressDialog();

            dlg.Title = title;
            dlg.IsDeterministic = deterministic;

            if (onCancel != null)
            {
                dlg.SetCancel(onCancel, cancelText);
            }

            if (show)
            {
                dlg.Show();
            }

            return dlg;
        }

        private Context GetActivityContext()
        {
            Context context = Utilities.GetActivityContext();
            if (context == null)
            {
                IMvxAndroidCurrentTopActivity topActivity;
                bool canResolve = Mvx.IoCProvider.TryResolve(out topActivity);

                IAndroidApp app = Mvx.IoCProvider.Resolve<IAndroidApp>();
                if (app != null)
                {
                    context = (Context) app.TopActivity;
                }
            }
            return context;
        }

        public void CancelAll()
        {
            if (_Dialog != null)
            {
                Utilities.Dispatch(() =>
                {
                    _Dialog.Cancel();
                });
            }
        }

        public async Task TimespanDialogWheelPicker(
            TimeSpan timeSpan,
            string title, 
            string message,
            string affirmButton,
            string denyButton,
            Action<TimeSpan> onAffirm,
            Action onDeny = null)
        {
            Utilities.Dispatch(() =>
            {
                var context = GetActivityContext();

                // Hours picker
                var hoursNumberPicker = new NumberPicker(context);
                hoursNumberPicker.LayoutParameters = new LinearLayout.LayoutParams(WrapContent, WrapContent);
                hoursNumberPicker.MaxValue = 100;
                hoursNumberPicker.Value = (int)Math.Floor(timeSpan.TotalHours);

                var hoursPickerWrapper = new LinearLayout(context);
                hoursPickerWrapper.Orientation = Orientation.Vertical;
                hoursPickerWrapper.LayoutParameters = new LinearLayout.LayoutParams(WrapContent, WrapContent);

                var hoursLabel = new TextView(context);
                hoursLabel.LayoutParameters = new LinearLayout.LayoutParams(WrapContent, WrapContent) {Gravity = GravityFlags.CenterHorizontal};
                hoursLabel.Text = "Hours";

                hoursPickerWrapper.AddView(hoursLabel);
                hoursPickerWrapper.AddView(hoursNumberPicker);

                // Minutes picker
                var minutesNumberPicker = new NumberPicker(context);
                minutesNumberPicker.LayoutParameters = new LinearLayout.LayoutParams(WrapContent, WrapContent);
                minutesNumberPicker.MinValue = 0;
                minutesNumberPicker.MaxValue = 59;
                minutesNumberPicker.Value = timeSpan.Minutes;

                var minutesPickerWrapper = new LinearLayout(context);
                minutesPickerWrapper.Orientation = Orientation.Vertical;
                minutesPickerWrapper.LayoutParameters = new LinearLayout.LayoutParams(WrapContent, WrapContent);

                var minutesLabel = new TextView(context);
                minutesLabel.LayoutParameters = new LinearLayout.LayoutParams(WrapContent, WrapContent) { Gravity = GravityFlags.CenterHorizontal };
                minutesLabel.Text = "Minutes";

                minutesPickerWrapper.AddView(minutesLabel);
                minutesPickerWrapper.AddView(minutesNumberPicker);


                var linearLayout = new LinearLayout(context);
                linearLayout.LayoutParameters = new FrameLayout.LayoutParams(WrapContent, WrapContent) { Gravity = GravityFlags.CenterHorizontal };
                linearLayout.Orientation = Orientation.Horizontal;
                linearLayout.AddView(hoursPickerWrapper);
                linearLayout.AddView(minutesPickerWrapper);

                var frame = new FrameLayout(context);
                frame.AddView(linearLayout);

                _Dialog = new AlertDialog.Builder(context)
                    .SetMessage(message)
                    .SetTitle(title)
                    .SetView(frame)
                    .SetPositiveButton(affirmButton, (o, e) => onAffirm?.Invoke(new TimeSpan(hoursNumberPicker.Value, minutesNumberPicker.Value, 0)))
                    .SetNegativeButton(denyButton, (o, e) => onDeny?.Invoke())
                    .Show();
            });
        }

        private void ShowKeyboard(View pView)
        {
            pView.RequestFocus();

            var window = ((Activity)GetActivityContext()).Window;
            if (_oldInputMode == null)
            {
                _oldInputMode = window.Attributes.SoftInputMode;
            }

            window.SetSoftInputMode(SoftInput.AdjustPan);

            InputMethodManager inputMethodManager = Application.Context.GetSystemService(Context.InputMethodService) as InputMethodManager;
            inputMethodManager.ShowSoftInput(pView, ShowFlags.Forced);
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
        }

        private void HideKeyboard(View pView)
        {
            InputMethodManager inputMethodManager = Application.Context.GetSystemService(Context.InputMethodService) as InputMethodManager;
            inputMethodManager.HideSoftInputFromWindow(pView.WindowToken, HideSoftInputFlags.None);
        }

        private class RestoreSIMDismissHandler : Java.Lang.Object, IDialogInterfaceOnDismissListener
        {
            public View View { get; set; }
            public DialogService DialogService { get; set; }

            public void OnDismiss(IDialogInterface dialog)
            {
                if (DialogService._oldInputMode != null)
                {
                    ((Activity)DialogService.GetActivityContext()).Window.SetSoftInputMode((SoftInput) DialogService._oldInputMode);
                    DialogService._oldInputMode = null;
                }
            }
        }

        #endregion Private
    }
}