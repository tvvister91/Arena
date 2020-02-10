using PE.Plugins.Dialogs.WindowsCommon.Controls;

using System;
using System.Threading.Tasks;

using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace PE.Plugins.Dialogs.WindowsCommon
{
    public class DialogService : IDialogService
    {
        #region Private

        private readonly DialogConfig _Configuration;

        private Popup _Popup;
        private IUpdatablePopup _CustomDialog;
        private ContentDialog _dialog;

        #endregion Private

        #region C-tors

        public DialogService(DialogConfig config)
        {
            _Configuration = config;
        }

        #endregion C-tors

        #region Members

        #region Loading

        public async Task ShowLoadingAsync()
        {
            CreateLoading(string.Empty);
        }

        public async Task ShowLoadingAsync(string message)
        {
            CreateLoading(message);
        }

        public async Task UpdateLoadingAsync(string message)
        {
            if (_CustomDialog == null) return;
            _CustomDialog.Message = message;
        }

        public async Task HideLoadingAsync()
        {
            if (_Popup == null) return;
            RemovePopup();
        }

        private void CreateLoading(string text)
        {
            if (_Popup != null)
            {
                _Popup.IsOpen = false;
                _Popup.Child = null;
                _CustomDialog = null;
                _Popup = null;
            }

            _Popup = new Popup();
            _CustomDialog = (_Configuration.CustomLoadingDialog != null) ? _Configuration.CustomLoadingDialog(text) : new CustomLoading();
            _Popup.Child = (UIElement)_CustomDialog;
            ApplicationView window = ApplicationView.GetForCurrentView();
            _Popup.Width = window.VisibleBounds.Width;
            _Popup.Height = window.VisibleBounds.Height;
            _CustomDialog.PopupWidth = window.VisibleBounds.Width;
            _CustomDialog.PopupHeight = window.VisibleBounds.Height;
            _CustomDialog.Message = text;

            _Popup.IsOpen = true;
        }

        private void CreateProgress(string text, float value)
        {
            if (_Popup != null)
            {
                _Popup.IsOpen = false;
                _Popup.Child = null;
                _CustomDialog = null;
                _Popup = null;
            }

            _Popup = new Popup();
            _CustomDialog = new CustomProgress();
            _Popup.Child = (UIElement)_CustomDialog;
            var window = ApplicationView.GetForCurrentView();
            _Popup.Width = window.VisibleBounds.Width;
            _Popup.Height = window.VisibleBounds.Height;
            _CustomDialog.PopupWidth = window.VisibleBounds.Width;
            _CustomDialog.PopupHeight = window.VisibleBounds.Height;
            _CustomDialog.Message = text;
            _CustomDialog.Progress = value;

            _Popup.IsOpen = true;
        }

        private void RemovePopup()
        {
            if (_Popup != null)
            {
                _Popup.IsOpen = false;
                _Popup.Child = null;
                _CustomDialog = null;
                _Popup = null;
            }
        }

        #endregion Loading

        #region Progress

        public async Task ShowProgressAsync()
        {
            CreateProgress(string.Empty, 0);
        }

        public async Task ShowProgressAsync(string message)
        {
            CreateProgress(message, 0);
        }

        public async Task ShowProgressAsync(string message, float value)
        {
            CreateProgress(message, value);
        }

        public async Task UpdateProgressAsync(float value)
        {
            if (_CustomDialog == null) return;
            _CustomDialog.Progress = value;
        }

        public async Task UpdateProgressAsync(string message, float value)
        {
            _CustomDialog.Message = message;
            _CustomDialog.Progress = value;
        }

        public async Task HideProgressAsync()
        {
            if (_CustomDialog == null) return;
            RemovePopup();
        }

        #endregion Progress

        #region Alert

        public async Task AlertAsync(string message, string title, string button, Action onOk)
        {   //  show message dialog
            var dialog = new MessageDialog(message, title ?? string.Empty);
            dialog.Commands.Add(new UICommand(button));
            await dialog.ShowAsync();
            onOk?.Invoke();
        }

        #endregion Alert

        #region Confirm 

        public async Task ConfirmAsync(string message, string title, string positiveButton, Action onPositive, string negativeButton, Action onNegative, bool hideOnClickOutside = false)
        {   // HACK: don't use the MessageDialog - for Win 10 it has duplicated titles issue. AT-1358
            HideDialog();
            //  show message dialog
            _dialog = new ContentDialog {
                Title = title ?? string.Empty,
                Content = message ?? string.Empty,
                CloseButtonText = negativeButton,
                PrimaryButtonText = positiveButton
            };

            var result = await _dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                onPositive?.Invoke();
            }
            else
            {
                onNegative?.Invoke();
            }
        }

        #endregion Confirm

        #region Prompt

        public async Task PromptAsync(string message, string title, string affirmButton, string denyButton, Action<string> onAffirm, Action onDeny, bool password = false, string placeholder = "")
        {
            if (password)
            {
                await PromptPasswordAsync(message, title, affirmButton, denyButton, onAffirm, onDeny, placeholder);
            }
            else
            {
                await PromptAsync(message, title, affirmButton, denyButton, onAffirm, onDeny, string.Empty, placeholder);
            }
        }

        public async Task PromptPasswordAsync(string message, string title,
                                              string affirmButton, string denyButton,
                                              Action<string> onAffirm = null, Action onDeny = null,
                                              string placeholder = "")
        {
            HideDialog();

            var passwordBox = new PasswordBox {
                Height = 32,
                PlaceholderText = placeholder
            };

            await PromptAsync(passwordBox, message, title, affirmButton, denyButton, onAffirm, onDeny);
        }

        public async Task PromptAsync(string message, string title,
                                      string affirmButton, string denyButton,
                                      Action<string> onAffirm, Action onDeny = null,
                                      string text = "", string placeholder = "",
                                      Func<string, string, bool> validationFunc = null)
        {
            var textBox = new TextBox {
                Height = 32,
                AcceptsReturn = false,
                Text = text,
                PlaceholderText = placeholder
            };

            textBox.BeforeTextChanging += (tb, e) => {
                if (validationFunc != null)
                {
                    e.Cancel = !validationFunc(tb.Text, e.NewText);
                }
            };

            await PromptAsync(textBox, message, title, affirmButton, denyButton, onAffirm, onDeny);
        }

        private async Task PromptAsync(Control input, string message, string title,
                                       string affirmButton, string denyButton,
                                       Action<string> onAffirm, Action onDeny = null)
        {
            HideDialog();

            _dialog = new ContentDialog {
                Title = title,
                Content = new StackPanel {
                    Spacing = 20,
                    Children = {
                        new TextBlock { Text = message },
                        input
                    }
                },
                CloseButtonText = denyButton,
                PrimaryButtonText = affirmButton,
            };

            var result = await _dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var inputText = string.Empty;

                if (input is PasswordBox passwordBox) inputText = passwordBox.Password;
                else if (input is TextBox textBox) inputText = textBox.Text;

                onAffirm?.Invoke(inputText);
            }
            else
            {
                onDeny?.Invoke();
            }
        }

        private void HideDialog()
        {
            _dialog?.Hide();
        }

        #endregion Prompt

        #region Date Picker

        public Task DatePickerAsync(DateTime? date, Action<DateTime> onSet, Action onCancel)
        {
            throw new NotImplementedException();
        }

        #endregion Date Picker

        #endregion Members
    }
}
