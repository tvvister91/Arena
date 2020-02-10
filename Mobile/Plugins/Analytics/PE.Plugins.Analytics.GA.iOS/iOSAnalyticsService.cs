using System;
using System.Collections.Generic;
using Google.Analytics;
using MvvmCross;
using PE.Framework.AppVersion;
using PE.Framework.iOS.AppVersion;

namespace PE.Plugins.Analytics.GA.iOS
{
    public class iOSAnalyticsService : AnalyticsService, IAnalyticsService
    {
        ITracker _sharedTracker;

        #region Constructors

        public iOSAnalyticsService(GAConfiguration configuration)
            : base(configuration)
        {
             Mvx.IoCProvider.RegisterSingleton<IVersion>(() => new AppVersionImpl());
             _version = Mvx.IoCProvider.Resolve<IVersion>();

            Gai.SharedInstance.DispatchInterval = configuration.DispatchInterval;
            Gai.SharedInstance.TrackUncaughtExceptions = true;

            #if DEBUG
                // TODO uncomment before Release
                //Gai.SharedInstance.DryRun = true;
            #endif

            if (!string.IsNullOrWhiteSpace(configuration.TrackingId))
            {
                _sharedTracker = Gai.SharedInstance.GetTracker("Atelier", configuration.TrackingId);
                _sharedTracker.Set("&aid", configuration.AppId);
                _sharedTracker.Set("&an", configuration.AppName);
                _sharedTracker.Set("&av", _version.Version);
            }
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
            _sharedTracker.Send(DictionaryBuilder.CreateEvent(category, action, label, null).Build());
        }

        public override void RecordException(string exDescription, bool exFatal)
        {
            _sharedTracker.Send(DictionaryBuilder.CreateException(exDescription, exFatal)
                          .Build());

        }

        public override void RecordException(Exception exception, string description)
        {
            string fullDescription = description + "\n" + exception.StackTrace;
            RecordException(fullDescription, false);
        }
    }
}
