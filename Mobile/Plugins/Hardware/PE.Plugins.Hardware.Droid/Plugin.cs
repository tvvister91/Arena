using MvvmCross;
using MvvmCross.Plugin;

namespace PE.Plugins.Hardware.Droid
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