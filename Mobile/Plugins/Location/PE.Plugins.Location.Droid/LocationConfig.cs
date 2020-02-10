using Android.Content;
using MvvmCross.Plugin;

namespace PE.Plugins.Location.Droid
{
    public enum LocationProvider
    {
        PlayServices,
        Legacy
    }

    public class LocationConfig : LocationConfigBase, IMvxPluginConfiguration
    {
        public Context AppContext { get; set; }

        public LocationProvider Provider { get; set; }
    }
}