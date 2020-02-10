using MvvmCross;
using MvvmCross.Plugin;

namespace PE.Plugins.Location.iOS
{
    [MvxPlugin]
    public class Plugin : IMvxPlugin
    {
        public void Load()
        {
            Mvx.IoCProvider.RegisterSingleton<ILocationService>(() => new LocationService());
        }
    }
}
