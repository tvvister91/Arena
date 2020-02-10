using MvvmCross.Plugin;

namespace PE.Plugins.Analytics.GA
{
    public class GAConfiguration : IMvxPluginConfiguration
    {

        public string TrackingId { get; set; }

        public string AppId { get; set; }

        public string AppName { get; set; }

        public string AppVersion { get; set; }

        public double DispatchInterval { get; set; }

    }
}
