using MvvmCross;
using MvvmCross.Plugin;

namespace PE.Plugins.SecureStorage.Droid
{
    [MvxPlugin]
    public class Plugin : IMvxConfigurablePlugin
    {
        private SecureStorageConfigurationDroid _Configuration;

        public void Configure(IMvxPluginConfiguration configuration)
        {
            if (!(configuration is SecureStorageConfigurationDroid)) throw new System.ArgumentException("Configuration is not a valid SecureStorageConfigurationDroid.");
            _Configuration = (SecureStorageConfigurationDroid)configuration;
        }

        public void Load()
        {
            Mvx.IoCProvider.RegisterSingleton<ISessionService>(() => new SessionService(_Configuration));
        }
    }
}