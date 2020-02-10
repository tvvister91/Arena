using MvvmCross;
using MvvmCross.Plugin;

namespace PE.Plugins.OktaAuth
{

    [MvxPlugin]
    public class Plugin : IMvxConfigurablePlugin
    {
        #region Fields

        private OktaAuthConfiguration _Configuration;

        #endregion Fields

        #region Init

        public void Configure(IMvxPluginConfiguration configuration)
        {
            if (!(configuration is OktaAuthConfiguration)) throw new System.Exception("Configuration does not appear to be a valid OktaAuthConfiguration.");
            _Configuration = (OktaAuthConfiguration)configuration;
        }

        public void Load()
        {
            Mvx.LazyConstructAndRegisterSingleton<IOktaAuthService>(() => new OktaAuthService(_Configuration));
        }

        #endregion Init
    }
}
