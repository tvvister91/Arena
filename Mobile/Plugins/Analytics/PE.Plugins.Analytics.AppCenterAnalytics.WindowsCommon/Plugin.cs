using MvvmCross;
using MvvmCross.Plugin;

namespace PE.Plugins.Analytics.AppCenterAnalytics.WindowsCommon
{
    [MvxPlugin]
    public class Plugin : IMvxConfigurablePlugin
    {
        #region Fields

        private AppCenterConfiguration _Configuration;

        #endregion Fields

        #region Init

        public void Configure(IMvxPluginConfiguration configuration)
        {
            if (!(configuration is AppCenterConfiguration)) throw new System.Exception("Configuration does not appear to be a valid AppCenterConfiguration.");
            _Configuration = (AppCenterConfiguration)configuration;
        }

        public void Load()
        {
            Mvx.LazyConstructAndRegisterSingleton<IAnalyticsService>(() => new WinAnalyticsService(_Configuration));
        }

        #endregion Init
    }
}
