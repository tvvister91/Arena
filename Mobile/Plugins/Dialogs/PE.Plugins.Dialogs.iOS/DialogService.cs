using System;
using System.Linq;
using System.Threading.Tasks;

using Foundation;
using UIKit;

using PE.Framework.iOS;
using PE.Plugins.Dialogs.Enums;

namespace PE.Plugins.Dialogs.iOS
{
    public class DialogService : IDialogService
    {
        #region Private

        private readonly DialogsConfiguration _Configuration;

        private IProgressDialog _Progress;
        private UIAlertView _Alert;
        private UIAlertController _AlertController;
        private string _oldText = string.Empty;

        #endregion Private

        #region C-tors

        public DialogService(DialogsConfiguration configuration)
        {
            _Configuration = configuration;
        }

        #endregion C-tors

        #region Members

        #region Loading

        public void CancelAll()
        {
            CancelAlert();
            CancelAlertController();
        }

        public async Task ShowLoadingAsync()
        {
            await ShowLoadingAsync(string.Empty);
        }

        public async Task ShowLoadingAsync(string message)
        {
            if (_Progress == null)
            {
                _Progress = (_Configuration.CustomLoadingDialog != null) ? _Configuration.CustomLoadingDialog(message) : Loading(message, null, null, true);
            }
        }

        public async Task UpdateLoadingAsync(string message)
        {
            if (_Progress == null) return;
            _Progress.Title = message;
        }

        public async Task HideLoadingAsync()
        {
            try
            {
                if (_Progress == null) return;
                _Progress.Hide();
                _Progress.Dispose();
                _Progress = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** DialogService.HideLoadingAsync - Exception: {0}", ex));
            }
        }

        #endregion Loading

        #region Progress

        public async Task ShowProgressAsync()
        {
            await ShowProgressAsync(string.Empty);
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
            _Progress.PercentComplete = (int)(value * 100);
        }

        public async Task UpdateProgressAsync(float value)
        {
            if (_Progress == null) return;
            _Progress.PercentComplete = (int)(value * 100);
        }

        public async Task UpdateProgressAsync(string message, float value)
        {
            if (_Progress == null) return;
            _Progress.PercentComplete = (int)(value * 100);
            _Progress.Title = message;
        }

        public async Task HideProgressAsync()
        {
            if (_Progress == null) return;
            _Progress.Hide();
            _Progress.Dispose();
            _Progress = null;
        }

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

        private void CancelAlert()
        {
            if (_Alert == null) return;

            Utilities.Dispatch(() => _Alert.DismissWithClickedButtonIndex(-1, true));
        }

        private void CancelAlertController()
        {
            if (_AlertController == null) return;

            _ToastTimer?.Invalidate();
            _ToastTimer?.Dispose();
            _ToastTimer = null;

            Utilities.Dispatch(() => _AlertController.DismissViewController(true, null));
        }

        #endregion Progress

        #region Toast

        private static object _ToastLock = new object();
        private bool _HasToast;
        private NSTimer _ToastTimer;

        public void ShowToast(string message, ToastLength length, PlatformCode usingPlatforms = PlatformCode.All)
        {
            if (!usingPlatforms.HasFlag(PlatformCode.Ios)) return;

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

                ShowOrUpdateToast(message, length);

                var toastTime = length == ToastLength.Long ? DialogConstants.TOAST_LONG_LENGTH : DialogConstants.TOAST_SHORT_LENGTH;
                await Task.Delay(toastTime / 2);

                lock (_ToastLock)
                {
                    _HasToast = false;
                }
            });
        }

        private void ShowOrUpdateToast(string message, ToastLength length)
        {
            var startTimer = false;

            if (_ToastTimer == null || !_ToastTimer.IsValid)
            {   // close any alert (EXCEPT working toast!) before the showing new toast
                CancelAlertController();

                _AlertController = UIAlertController.Create(null, message, UIAlertControllerStyle.ActionSheet);
                UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(_AlertController, true, () => {
                    // handle outside click
                    _AlertController.View?.Superview?.Subviews?.FirstOrDefault()?.AddGestureRecognizer(new UITapGestureRecognizer(CancelAlertController));
                });
                startTimer = true;
            }
            else if (_ToastTimer != null && _ToastTimer.IsValid &&
                _AlertController != null && _AlertController == UIApplication.SharedApplication.KeyWindow.RootViewController.PresentedViewController)
            {
                _ToastTimer.Invalidate();
                _AlertController.Message = message;
                startTimer = true;
            }

            if (startTimer)
            {
                var toastMilliseconds = length == ToastLength.Long ? DialogConstants.TOAST_LONG_LENGTH : DialogConstants.TOAST_SHORT_LENGTH;
                _ToastTimer = NSTimer.CreateScheduledTimer(TimeSpan.FromMilliseconds(toastMilliseconds), ToastTimeComplete);
            }
        }

        private void ToastTimeComplete(NSTimer timer)
        {
            CancelAlertController();
        }

        #endregion Toast

        #region Alert

        public async Task AlertAsync(string message, string title, string button, Action onOk)
        {
            CancelAlert();

            Utilities.Dispatch(() =>
            {
                _Alert = new UIAlertView(title ?? String.Empty, message, null, null, button);

                if (onOk != null)
                {
                    _Alert.Clicked += (s, e) => onOk();
                }

                _Alert.Show();
            });
        }

        #endregion Alert

        #region Confirm

        public async Task ConfirmAsync(string message, string title, string positiveButton, Action onPositive, string negativeButton, Action onNegative)
        {
            CancelAlert();

            Utilities.Dispatch(() =>
            {
                _Alert = new UIAlertView(title ?? String.Empty, message, null, negativeButton, positiveButton);

                _Alert.Clicked += (s, e) =>
                {
                    if (_Alert.CancelButtonIndex == e.ButtonIndex)
                    {
                        onNegative?.Invoke();
                    }
                    else
                    {
                        onPositive?.Invoke();
                    }
                };

                _Alert.Show();
            });
        }

        // TODO Andrew, Alexander, please review this approach
        public async Task ConfirmAsync(string message, string title, string positiveButton, Action onPositive, string negativeButton, Action onNegative, bool hideOnClickOutside = false)
        {
            CancelAlertController();

            Utilities.Dispatch(() =>
            {
                _AlertController = UIAlertController.Create(title ?? string.Empty, message, UIAlertControllerStyle.Alert);
                _AlertController.AddAction(UIAlertAction.Create(positiveButton, UIAlertActionStyle.Default, (action) => { onPositive?.Invoke(); }));
                _AlertController.AddAction(UIAlertAction.Create(negativeButton, UIAlertActionStyle.Cancel, (action) => { onNegative?.Invoke(); }));

                if (hideOnClickOutside)
                {
                    UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(_AlertController, true, () =>
                    {
                        UITapGestureRecognizer recognizer = new UITapGestureRecognizer((tapRecognizer) =>
                        {
                            _AlertController.DismissViewController(true, null);
                        });
                        _AlertController.View.Superview.Subviews[0].AddGestureRecognizer(recognizer);
                    });
                }
                else
                {
                    UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(_AlertController, true, null);
                }
            });
        }

        #endregion Confirm

        #region Prompt

        public async Task PromptAsync(string message, string title, string affirmButton, string denyButton, Action<string> onAffirm, Action onDeny, bool password = false, string placeholder = "")
        {
            await PromptAsync(message, title, affirmButton, denyButton, onAffirm, onDeny, "", placeholder, null, password);
        }

        public async Task PromptPasswordAsync(string message, string title,
                                              string affirmButton, string denyButton,
                                              Action<string> onAffirm, Action onDeny,
                                              string placeholder)
        {
            await PromptAsync(message, title, affirmButton, denyButton, onAffirm, onDeny, string.Empty, placeholder, null, true);
        }

        public async Task PromptAsync(string message, string title,
                                      string affirmButton, string denyButton,
                                      Action<string> onAffirm, Action onDeny = null,
                                      string text = "", string placeholder = "",
                                      Func<string, string, bool> validationFunc = null)
        {
            await PromptAsync(message, title, affirmButton, denyButton, onAffirm, onDeny, text, placeholder, validationFunc, false);
        }

        private async Task PromptAsync(string message, string title,
                                      string affirmButton, string denyButton,
                                      Action<string> onAffirm, Action onDeny = null,
                                      string text = "", string placeholder = "",
                                      Func<string, string, bool> validationFunc = null,
                                      bool password = false)
        {
            CancelAlert();

            Utilities.Dispatch(() => {
                _Alert = new UIAlertView(title ?? String.Empty, message, null, denyButton, affirmButton) {
                    AlertViewStyle = password ? UIAlertViewStyle.SecureTextInput : UIAlertViewStyle.PlainTextInput
                };

                _oldText = text;
                var txt = _Alert.GetTextField(0);
                txt.Text = text;
                txt.SecureTextEntry = password;
                txt.Placeholder = placeholder;
                txt.EditingChanged += (object sender, EventArgs e) => {
                    if (validationFunc == null) return;

                    if (!validationFunc(_oldText, txt.Text))
                    {
                        txt.Text = _oldText;
                    }
                    else
                    {
                        _oldText = txt.Text;
                    }
                };
                _Alert.Clicked += (s, e) => {
                    if (_Alert.CancelButtonIndex == e.ButtonIndex)
                    {
                        onDeny?.Invoke();
                    }
                    else
                    {
                        onAffirm?.Invoke(txt.Text);
                    }
                };
                _Alert.Show();
            });
        }

        public void ShowPrompt(string title, string message,
                               string affirmButton, string denyButton,
                               Action<string> onAffirm, Action onDeny = null,
                               string text = "", string placeholder = "",
                               Func<string, bool> affirmEnableFunc = null,
                               bool isPassword = false)
        {
            CancelAlert();

            Utilities.Dispatch(() => {
                _Alert = new UIAlertView(title ?? string.Empty, message, null, denyButton, new string[] { affirmButton }) {
                    AlertViewStyle = isPassword ? UIAlertViewStyle.SecureTextInput : UIAlertViewStyle.PlainTextInput
                };

                var textField = _Alert.GetTextField(0);
                textField.Text = text;
                textField.SecureTextEntry = isPassword;
                textField.Placeholder = placeholder;

                _Alert.ShouldEnableFirstOtherButton = al => affirmEnableFunc?.Invoke(textField?.Text) ?? true;

                _Alert.Clicked += (s, e) => {
                    if (_Alert.CancelButtonIndex == e.ButtonIndex)
                    {
                        onDeny?.Invoke();
                    }
                    else
                    {
                        onAffirm?.Invoke(textField?.Text);
                    }
                };

                _Alert.Show();
            });
        }

        #endregion Prompt

        #region Action Sheet

        public void ShowActionSheet(string title, string message, string cancel, string destructive, Action<string> callback, params string[] buttons)
        {
            CancelAlertController();

            Utilities.Dispatch(() => {
                _AlertController = UIAlertController.Create(title ?? string.Empty, message, UIAlertControllerStyle.ActionSheet);
                Array.ForEach(buttons ?? new string[0], x => _AlertController.AddAction(GetAlertAction(x, callback)));
                UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(_AlertController, true, null);
            });
        }

        private UIAlertAction GetAlertAction(string title, Action<string> callback)
        {
            return UIAlertAction.Create(title, UIAlertActionStyle.Default, x => callback(x.Title));
        }

        #endregion Action Sheet

        #region Date Picker

        public async Task DatePickerAsync(DateTime? date, Action<DateTime> onSet, Action onCancel)
        {
            throw new System.NotImplementedException();
            //var dt = (date == null) ? DateTime.Now : date.Value;

            //Utilities.Dispatch(() =>
            //{
            //    var dlg = new DatePickerDialog(Utilities.GetActivityContext(), (o, e) =>
            //    {
            //        onSet?.Invoke(e.Date);
            //    }, dt.Year, dt.Month, dt.Day);
            //    dlg.Show();
            //});
        }

        #endregion Date Picker

        #region Pickers

        public async Task TimespanDialogWheelPicker(
            TimeSpan timeSpan, 
            string title, 
            string message,
            string affirmButton,
            string denyButton,
            Action<TimeSpan> onAffirm,
            Action onDeny = null)
        {
            var controller = new TimeSpanPickerViewController(timeSpan, title, message, affirmButton, denyButton, onAffirm, onDeny)
                {
                    ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen,
                    ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve
                };

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(controller, true, null);
        }

        #endregion

        #endregion Members
    }
}