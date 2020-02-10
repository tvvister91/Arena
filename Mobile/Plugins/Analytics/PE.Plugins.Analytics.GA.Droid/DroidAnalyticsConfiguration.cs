using Android.Content;
using MvvmCross.Plugin;

namespace PE.Plugins.Analytics.GA.Droid
{
    public class DroidAnalyticsConfiguration : GAConfiguration, IMvxPluginConfiguration
    {
        public Context AppContext { get; set; }
    }
}