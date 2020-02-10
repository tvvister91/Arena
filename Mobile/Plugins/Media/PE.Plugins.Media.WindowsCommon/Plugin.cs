using MvvmCross;
using MvvmCross.Plugin;

namespace PE.Plugins.Media.WindowsCommon
{
    [MvxPlugin]
    public class Plugin : IMvxConfigurablePlugin
    {
        private MediaConfig _Configuration;

        public void Configure(IMvxPluginConfiguration configuration)
        {
            if (!(configuration is MediaConfig)) throw new System.Exception("Configuration is not a valid MediaConfig.");
            _Configuration = (MediaConfig)configuration;
        }

        public void Load()
        {
            Mvx.IoCProvider.RegisterSingleton<IAudioService>(() => new AudioService(_Configuration));
            Mvx.IoCProvider.RegisterSingleton<ICameraService>(() => new CameraService());
        }
    }
}
