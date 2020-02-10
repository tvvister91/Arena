using PE.Framework.Helpers;
using Windows.UI.ViewManagement;

namespace PE.Framework.UWP.Helpers
{
    public class KeyboardHelperImpl : IKeyboardHelper
    {
        private InputPane _inputPane;

        public KeyboardHelperImpl()
        {
            _inputPane = InputPane.GetForCurrentView();
        }

        public void ShowKeyboard()
        {
            _inputPane?.TryShow();
        }
    }
}
