using MvvmCross.Plugin;
using PE.Framework.Droid;
using PE.Plugins.LocalStorage.Droid;

namespace Arena.Droid.Bootstrap
{
    public class LocalStorageBootstrap
    {
        public static IMvxPluginConfiguration Configure()
        {
            return new LocalStorageConfiguration
            {
                ApplicationContext = Android.App.Application.Context
            };
        }
    }
}
