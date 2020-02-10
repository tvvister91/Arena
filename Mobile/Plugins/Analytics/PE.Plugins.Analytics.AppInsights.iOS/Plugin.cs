using MvvmCross;
using MvvmCross.Plugin;

using System;

namespace PE.Plugins.Analytics.AppInsights.iOS
{
    [MvxPlugin]
    public class Plugin : IMvxConfigurablePlugin
    {
        #region Fields

        private AIConfiguration _Configuration;

        #endregion Fields

        #region Init

        public void Configure(IMvxPluginConfiguration configuration)
        {
            if (!(configuration is AIConfiguration)) throw new Exception("Configuration is not a valid AIConfiguration");
            _Configuration = (AIConfiguration)configuration;
        }

        public void Load()
        {
            Mvx.IoCProvider.RegisterSingleton<IAnalyticsService>(() => new iOSAnalyticsService(_Configuration));
        }

        #endregion Init
    }
}
