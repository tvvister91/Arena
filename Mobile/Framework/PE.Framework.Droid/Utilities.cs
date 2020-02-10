using Android.App;
using Android.Content;
using MvvmCross;
using MvvmCross.Platforms.Android;
using PE.Framework.Droid.AndroidApp.AppVersion;
using System;
using System.Threading;

namespace PE.Framework.Droid
{
    public static class Utilities
    {
        public static Context GetActivityContext()
        {
			IMvxAndroidCurrentTopActivity topActivity;
			bool canResolve = Mvx.IoCProvider.TryResolve(out topActivity);
			if(canResolve){
				return topActivity.Activity;
			}
            IAndroidApp app; 
            canResolve = Mvx.IoCProvider.TryResolve(out app);
            if (canResolve)
            {
                return app.TopActivity;
            }
			return null;

        }

        public static void MaskException(Action action)
        {
            try
            {
                action();
            }
            catch
            {

            }
        }

        public static void Dispatch(Action action)
        {
            if (Application.SynchronizationContext == SynchronizationContext.Current)
            {
                action();
            }
            else
            {
                Application.SynchronizationContext.Post(x => MaskException(action), null);
            }
        }
    }
}