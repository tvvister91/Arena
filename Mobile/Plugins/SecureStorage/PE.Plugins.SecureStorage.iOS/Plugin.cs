using MvvmCross;
using MvvmCross.Plugin;

namespace PE.Plugins.SecureStorage.iOS
{
    [MvxPlugin]
    public class Plugin : IMvxConfigurablePlugin
    {
        private SecureStorageConfiguration _Configuration;

        public void Configure(IMvxPluginConfiguration configuration)
        {
            if (!(configuration is SecureStorageConfiguration)) throw new System.ArgumentException("Configuration is not a valid SecureStorageConfiguration");
            _Configuration = (SecureStorageConfiguration)configuration;
        }

        public void Load()
        {
            Mvx.IoCProvider.RegisterSingleton<ISessionService>(() => new SessionService(_Configuration));
        }
    }
}