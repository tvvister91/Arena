using System;
using MvvmCross;
using MvvmCross.Plugin;

namespace PE.Plugins.LocalStorage.iOS
{
    [MvxPlugin]
    public class Plugin : IMvxPlugin
    {
        #region Fields

        private LocalStorageConfiguration _Configuration;

        #endregion Fields

        #region Plugin

        public void Configure(IMvxPluginConfiguration configuration)
        {
            if (!(configuration is LocalStorageConfiguration)) throw new Exception("Configuration does not appear to be a valid NetworkConfiguration.");
            _Configuration = (LocalStorageConfiguration)configuration;
        }

        public void Load()
        {
            Mvx.IoCProvider.RegisterSingleton<ILocalStorageService>(() => new LocalStorageService());
        }

        #endregion Plugin
    }
}
