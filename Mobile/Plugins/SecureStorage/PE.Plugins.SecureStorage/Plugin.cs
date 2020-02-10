using MvvmCross;
using MvvmCross.Plugin;
using System;

namespace PE.Plugins.SecureStorage
{
    [MvxPlugin]
    public class Plugin : IMvxConfigurablePlugin
    {
        private SecureStorageConfiguration _Configuration;

        public void Configure(IMvxPluginConfiguration configuration)
        {
            if (!(configuration is SecureStorageConfiguration)) throw new ArgumentException("Configuration is not a valid SecureStorageConfiguration.");
            _Configuration = (SecureStorageConfiguration)configuration;
        }

        public void Load()
        {
            Mvx.IoCProvider.RegisterSingleton<ISessionService>(() => new SessionService(_Configuration));
        }
    }
}
