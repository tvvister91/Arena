using MvvmCross;
using MvvmCross.Plugin;

namespace PE.Plugins.Hardware.iOS
{
    [MvxPlugin]
    public class Plugin : IMvxPlugin
    {
        public void Load()
        {
            Mvx.IoCProvider.RegisterSingleton<IHardwareService>(() => new HardwareService());
        }
    }
}
