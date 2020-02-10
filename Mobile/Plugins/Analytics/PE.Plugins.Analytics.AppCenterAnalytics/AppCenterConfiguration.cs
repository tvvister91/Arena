using MvvmCross.Plugin;

using System;

namespace PE.Plugins.Analytics.AppCenterAnalytics
{
    public class AppCenterConfiguration : IMvxPluginConfiguration
    {
        public string AppSecret { get; set; }
    }
}
