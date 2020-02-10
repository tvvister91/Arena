using System;
using System.Threading;
using Android.Content;
using Android.Gms.Analytics;
using MvvmCross;
using PE.Framework.Droid.AndroidApp.AppVersion;
using PE.Framework.AppVersion;
using PE.Framework.Droid.AppVersion;

namespace PE.Plugins.Analytics.GA.Droid
{
    public class DroidAnalyticsService : AnalyticsService, IAnalyticsService
    {

        Tracker mTracker;

        #region Constructors

        public DroidAnalyticsService(GAConfiguration configuration)
            : base(configuration)
        {
             Mvx.IoCProvider.RegisterSingleton<IVersion>(() => new AppVersionImpl());
             _version = Mvx.IoCProvider.Resolve<IVersion>();
            configuration.AppVersion = _version.Version;

            IAndroidApp app = Mvx.IoCProvider.Resolve<IAndroidApp>();
            if (app != null)
            {
                var context = (Context)app.TopActivity;
                var analytics = GoogleAnalytics.GetInstance(context);
                analytics.SetLocalDispatchPeriod((int)configuration.DispatchInterval);
                mTracker = analytics.NewTracker(configuration.TrackingId);
                mTracker.SetAppId(configuration.AppId);
                mTracker.SetAppName(configuration.AppName);
                mTracker.SetAppVersion(configuration.AppVersion);
                mTracker.EnableExceptionReporting(true);
            }

        }

        #endregion Constructors

        public override void StartAnalytics() 
        {
            // NOP
        }

        public override void SetUserId(string userId)
        {
            mTracker?.Set("&uid", userId == null ? string.Empty : userId);
        }

        public override void TrackEvent(string category, string action)
        {
            TrackEvent(category, action, "");
        }

        public override void TrackEvent(string category, string action, string label)
        {
            mTracker.Send(new HitBuilders.EventBuilder()
                          .SetCategory(category)
                          .SetAction(action)
                          .SetLabel(label)
                          .Build());
        }

        public override void RecordException(string exDescription, bool exFatal)
        {
            mTracker.Send(new HitBuilders.ExceptionBuilder()
                          .SetDescription(exDescription)
                          .SetFatal(exFatal)
                          .Build());

        }

        public override void RecordException(Exception exception, string description)
        {
            IAndroidApp app = Mvx.IoCProvider.Resolve<IAndroidApp>();
            string fullDescription = description + " " + new StandardExceptionParser((Android.Content.Context)app.TopActivity, null)
                .GetDescription(Thread.CurrentThread.Name, (Java.Lang.Throwable)exception);
            mTracker.Send(new HitBuilders.ExceptionBuilder()
                          .SetDescription(fullDescription)
                          .SetFatal(false)
                          .Build());

        }
    }
}