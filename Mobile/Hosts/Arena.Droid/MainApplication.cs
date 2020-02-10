using Android.App;
using Android.OS;
using Android.Runtime;
using MvvmCross;
using MvvmCross.Platforms.Android.Views;
using PE.Framework.Droid.AndroidApp.AppVersion;
using System;
using Arena.Core.Services;
using Plugin.CurrentActivity;

namespace Arena.Droid
{
    [Application]
    public class MainApplication : MvxAndroidApplication, Application.IActivityLifecycleCallbacks
    {
        #region Constructors

        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            RegisterActivityLifecycleCallbacks(this);
        }

        public override void OnCreate()
        {
            base.OnCreate();
            CrossCurrentActivity.Current.Init(this);
        }

        #endregion Constructors

        #region Properties

        public static Activity Activity { get; private set; }

        public static bool Backgrounded { get; private set; }

        public static NotificationData PendingNotification { get; set; } = null;

        #endregion Properties

        #region IActivityLifecycleCallbacks

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityDestroyed(Activity activity)
        {
            if (Activity == activity) Activity = null;
        }

        public void OnActivityPaused(Activity activity)
        {
            // NOP
        }

        public void OnActivityResumed(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;

            try
            {
                Activity = activity;
                Backgrounded = false;
                var appService = Mvx.IoCProvider.Resolve<IAppService>();
                appService.RaiseOnUserInteractionAcquired();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(OnActivityResumed)} - Exception: {ex}");
            }
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
            // NOP
        }

        public void OnActivityStarted(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;

            IAndroidApp app = Mvx.IoCProvider.Resolve<IAndroidApp>();
            app.TopActivity = activity;
        }

        public void OnActivityStopped(Activity activity)
        {
            IAndroidApp app = Mvx.IoCProvider.Resolve<IAndroidApp>();
            if (activity.Equals(app.TopActivity))
            {
                app.TopActivity = null;
                Backgrounded = true;
                Mvx.IoCProvider.Resolve<IAppService>()?.RaiseOnUserInteractionStopped();
            }
        }

        #endregion IActivityLifecycleCallbacks
    }
}
