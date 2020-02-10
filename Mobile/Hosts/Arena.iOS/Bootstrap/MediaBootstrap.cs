using MvvmCross.Plugin;
using PE.Plugins.Media.iOS;

namespace Arena.iOS.Bootstrap
{
    public class MediaBootstrap
    {
        public static IMvxPluginConfiguration Configure()
        {
            return new MediaConfig();
        }
    }
}