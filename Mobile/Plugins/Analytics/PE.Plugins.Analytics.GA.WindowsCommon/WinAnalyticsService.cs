using GoogleAnalytics;
using MvvmCross;
using PE.Framework.AppVersion;
using PE.Framework.UWP.AppVersion;
using System;

namespace PE.Plugins.Analytics.GA.WindowsCommon
{
    public class WinAnalyticsService : AnalyticsService, IAnalyticsService
    {

        private static Tracker _sharedTracker;

        #region Constructors

        public WinAnalyticsService(GAConfiguration configuration)
            : base(configuration)
        {
            Mvx.IoCProvider.RegisterSingleton<IVersion>(() => new AppVersionImpl());
            _version = Mvx.IoCProvider.Resolve<IVersion>();

            _sharedTracker = AnalyticsManager.Current.CreateTracker(configuration.TrackingId);
            _sharedTracker.AppVersion = _version.Version;
            _sharedTracker.AppName = configuration.AppName;
            _sharedTracker.AppId = configuration.AppId;
            AnalyticsManager.Current.DispatchPeriod = TimeSpan.FromSeconds(configuration.DispatchInterval);
            AnalyticsManager.Current.ReportUncaughtExceptions = true;
#if DEBUG
            AnalyticsManager.Current.IsDebug = true;
#endif
        }

        #endregion Constructors

        public override void StartAnalytics()
        {
            // NOP
        }

        public override void SetUserId(string userId)
        {
            _sharedTracker?.Set("&uid", userId == null ? string.Empty : userId);
        }

        public override void TrackEvent(string category, string action)
        {
            TrackEvent(category, action, "");
        }

        public override void TrackEvent(string category, string action, string label)
        {
            _sharedTracker?.Send(HitBuilder.CreateCustomEvent(category, action, label, 0).Build());
        }

        public override void RecordException(string exDescription, bool exFatal)
        {
            _sharedTracker?.Send(HitBuilder.CreateException(exDescription, exFatal).Build());
        }

        public override void RecordException(Exception exception, string description)
        {
            string fullDescription = description + "\n" + exception.StackTrace;
            RecordException(fullDescription, false);
        }
    }
}
