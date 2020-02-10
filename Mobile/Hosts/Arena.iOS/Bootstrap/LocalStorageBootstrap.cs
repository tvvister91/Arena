using PE.Plugins.LocalStorage.iOS;

namespace Arena.iOS.Bootstrap
{
    public class LocalStorageBootstrap
    {
        public static MvvmCross.Plugin.IMvxPluginConfiguration Configure()
        {
            return new LocalStorageConfiguration();
        }
    }
}
