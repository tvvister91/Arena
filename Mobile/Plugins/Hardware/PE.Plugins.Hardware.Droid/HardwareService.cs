using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using PE.Framework.Droid;
using System;

namespace PE.Plugins.Hardware.Droid
{
    public class HardwareService : IHardwareService
    {
        public float[] GetSafeArea()
        {
            return new float[] { 0, 0, 0, 0 };
        }

        public bool HasTopNotch()
        {
            return false;
        }

        public bool IsEmulator()
        {
            if (Build.Fingerprint != null)
            {
                if (Build.Fingerprint.Contains("vbox") || Build.Fingerprint.Contains("generic"))
                    return true;
            }
            return false;
        }

        public void ShowStatusBar(bool flag, bool animated = false)
        {
            Utilities.Dispatch(() =>
            {
                var window = GetCurrentWindow();

                if (flag)
                {
                     window.ClearFlags(WindowManagerFlags.Fullscreen);
                }
                else
                {
                    window.AddFlags(WindowManagerFlags.Fullscreen);
                }
            });
        }

        public void UpdateStatusBarColor(int a, int r, int g, int b)
        {
            try
            {
                Utilities.Dispatch(() =>
                {
                    var window = GetCurrentWindow();

                    window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LightStatusBar;
                    window.SetStatusBarColor(Color.Argb(a, r, g, b));
                });
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(ShowStatusBar)} - Exception: {exc}");
            }
        }

        private Window GetCurrentWindow()
        {
            var activity = (Activity)Utilities.GetActivityContext();
            if (activity == null) return null;
            var window = activity.Window;

            // clear FLAG_TRANSLUCENT_STATUS flag:
            window.ClearFlags(WindowManagerFlags.TranslucentStatus);

            // add FLAG_DRAWS_SYSTEM_BAR_BACKGROUNDS flag to the window
            window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

            return window;
        }
    }
}
