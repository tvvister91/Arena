using System;
using MvvmCross.Plugin;

namespace PE.Plugins.Analytics.AppInsights
{
    public class AIConfiguration : IMvxPluginConfiguration
    {
        public string InstrumentationKey { get; set; }

        public string TrackingId { get; set; }

        public string AppId { get; set; }

        public string AppName { get; set; }

        public string AppVersion { get; set; }

        public double DispatchInterval { get; set; }

    }
}
