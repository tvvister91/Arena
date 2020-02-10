using System;
using PE.Framework.AppVersion;

namespace PE.Plugins.Analytics.GA
{
    public abstract class AnalyticsService : IAnalyticsService
    {
        #region Fields

        protected readonly GAConfiguration _Configuration;
        protected IVersion _version;

        #endregion Fields

        #region Constructors

        public AnalyticsService(GAConfiguration configuration)
        {
            _Configuration = configuration;
        }

        #endregion Constructors

        public abstract void StartAnalytics();
        public abstract void TrackEvent(string category, string action);
        public abstract void TrackEvent(string category, string action, string label);
        public abstract void RecordException(string exDescription, bool exFatal);
        public abstract void RecordException(Exception exception, string description);
        public abstract void SetUserId(string userId);
    }
}
