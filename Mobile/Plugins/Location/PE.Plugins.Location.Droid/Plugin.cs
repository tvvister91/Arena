using MvvmCross;
using MvvmCross.Plugin;
using System;

namespace PE.Plugins.Location.Droid
{
    [MvxPlugin]
    public class Plugin : IMvxConfigurablePlugin
    {
        private LocationConfig _Config;

        public void Configure(IMvxPluginConfiguration configuration)
        {
            if (!(configuration is LocationConfig)) throw new ArgumentException("Configuration is not a LocationConfig");
            _Config = (LocationConfig)configuration;
        }

        public void Load()
        {
            if ((_Config.Provider == LocationProvider.PlayServices) && (_Config.CheckEnabled != null))
            {
                Mvx.IoCProvider.RegisterSingleton<ILocationService>(() => new LocationPlayService(_Config));
            }
            else
            {
                Mvx.IoCProvider.RegisterSingleton<ILocationService>(() => new LocationService(_Config));
            }
        }
    }
}
