using System;
using System.Collections.Generic;
using PE.Framework.AppVersion;

namespace PE.Plugins.Analytics.AppCenterAnalytics
{
    public abstract class AnalyticsService : IAnalyticsService
    {
        #region Constants

        private const string EX_STACK_TRACK = "StackTrace";
        private const string EX_MESSAGE = "Message";
        private const string EX_EXCEPTION = "Exception";
        private const string EX_FATAL = "ExceptionFatal";
        private const string USER_ID = "UserID";
        private const string APP_Version = "AppVersion";
        private const string ACTION = "Action";
        private const string LABEL = "Label";

        #endregion Constants

        #region Fields

        protected readonly AppCenterConfiguration _Configuration;
		protected IVersion _version;
        protected string _UserId = "";
        protected abstract string AppVersion { get; }
        #endregion Fields

        #region Constructors

        public AnalyticsService(AppCenterConfiguration configuration)
        {
            _Configuration = configuration;
        }

        #endregion Constructors

        #region Operations

        public abstract void StartAnalytics();

        public void RecordException(string exDescription, bool exFatal)
        {
            var values = new Dictionary<string, string> { { EX_FATAL, exFatal.ToString() }, { EX_MESSAGE, exDescription } };
            TrackEvent(EX_EXCEPTION, values);
        }

        public void RecordException(Exception exception, string description)
        {
            var values = new Dictionary<string, string> {{ EX_MESSAGE, exception.Message}, {EX_STACK_TRACK, exception.StackTrace}};
            TrackEvent(EX_EXCEPTION, values);
        }

        public void SetUserId(string userId)
        {
            _UserId = userId;
        }

        public void TrackEvent(string category, string action)
        {
            TrackEvent(category, action, null);
        }

        public void TrackEvent(string category, string action, string Label)
        {
            var values = new Dictionary<string, string> { { ACTION, action }, { LABEL, Label } };
            TrackEvent(category, values);
        }

        protected void TrackEvent(string name, IDictionary<string, string> properties)
        {
            properties[USER_ID] = _UserId ?? "";
            properties[APP_Version] = AppVersion ?? "";
            Microsoft.AppCenter.Analytics.Analytics.TrackEvent(name, properties);
        }

        #endregion  Operations
    }
}
