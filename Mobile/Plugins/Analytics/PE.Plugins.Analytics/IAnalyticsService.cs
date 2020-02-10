using System;

namespace PE.Plugins.Analytics
{
    public interface IAnalyticsService
    {
        void StartAnalytics();

        void SetUserId(string userId);

        void TrackEvent(string category, string action);

        void TrackEvent(string category, string action, string Label);

        void RecordException(string exDescription, bool exFatal);

        void RecordException(Exception exception, string description);
    }
}
