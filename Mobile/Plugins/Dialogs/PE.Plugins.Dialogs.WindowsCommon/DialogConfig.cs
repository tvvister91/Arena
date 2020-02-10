using MvvmCross.Plugin;
using System;
using Windows.UI.Core;

namespace PE.Plugins.Dialogs.WindowsCommon
{
    public class DialogConfig : IMvxPluginConfiguration
    {
        public CoreWindow CoreWindow { get; set; }

        public Func<string, IUpdatablePopup> CustomLoadingDialog { get; set; }
    }
}
