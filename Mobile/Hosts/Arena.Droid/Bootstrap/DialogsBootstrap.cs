using MvvmCross.Plugin;

using PE.Plugins.Dialogs.Droid;

namespace Arena.Droid.Bootstrap
{
    public class DialogsBootstrap
    {
        public static IMvxPluginConfiguration Configure()
        {
            return new DialogConfig();
        }
    }
}
