using MvvmCross;
using MvvmCross.Plugin;

namespace PE.Plugins.Analytics.GA.iOS
{
    [MvxPlugin]
    public class Plugin : IMvxConfigurablePlugin
    {
        #region Fields

        private GAConfiguration _Configuration;

        #endregion Fields

        #region Init

        public void Configure(IMvxPluginConfiguration configuration)
        {
            if (!(configuration is GAConfiguration)) throw new System.Exception("Configuration does not appear to be a valid GAConfiguration.");
            _Configuration = (GAConfiguration)configuration;
        }

        public void Load()
        {
            Mvx.IoCProvider.RegisterSingleton<IAnalyticsService>(() => new iOSAnalyticsService(_Configuration));
        }

        #endregion Init
    }
}