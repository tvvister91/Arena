using MvvmCross;
using MvvmCross.Plugin;

using System;

namespace PE.Plugins.Analytics.GA.Droid
{
    [MvxPlugin]
    public class Plugin : IMvxConfigurablePlugin
    {
        #region Fields

        private DroidAnalyticsConfiguration _Configuration;

        #endregion Fields

        #region Init

        public void Configure(IMvxPluginConfiguration configuration)
        {
            if (!(configuration is DroidAnalyticsConfiguration)) throw new Exception("Configuration is not a valid DroidAnalyticsConfiguration");
            _Configuration = (DroidAnalyticsConfiguration)configuration;
        }

        public void Load()
        {
            Mvx.IoCProvider.RegisterSingleton<IAnalyticsService>(() => new DroidAnalyticsService(_Configuration));
        }

        #endregion Init
    }
}
