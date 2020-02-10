using MvvmCross.Plugin;
using PE.Plugins.Media.Droid;

namespace Arena.Droid.Bootstrap
{
    public class MediaBootstrap
    {
        public static IMvxPluginConfiguration Configure()
        {
            return new MediaConfig();
        }
    }
}