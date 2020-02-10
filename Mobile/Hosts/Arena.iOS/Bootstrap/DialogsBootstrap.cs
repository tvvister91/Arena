using MvvmCross.Plugin;
using PE.Plugins.Dialogs.iOS;

namespace Arena.iOS.Bootstrap
{
    public class DialogsBootstrap
    {
        public static IMvxPluginConfiguration Configure()
        {
            return new iOSDialogsConfiguration()
            {
                //  TODO: for a custom loading indicator use the CustomLoading property
            };
        }
    }
}
