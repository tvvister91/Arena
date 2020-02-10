using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace PE.Plugins.Analytics.AppInsights
{
    public abstract class AnalyticsService : IAnalyticsService
    {
        public abstract void RecordException(string exDescription, bool exFatal);
        public abstract void RecordException(Exception exception, string description);
        public abstract void SetUserId(string userId);
        public abstract void StartAnalytics();
        public abstract void TrackEvent(string category, string action);
        public abstract void TrackEvent(string category, string action, string Label);
    }
}
