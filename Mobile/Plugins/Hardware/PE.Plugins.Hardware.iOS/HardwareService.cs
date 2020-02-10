using Foundation;
using System;
using UIKit;

namespace PE.Plugins.Hardware.iOS
{
    public class HardwareService : IHardwareService
    {
        #region Fields

        private iOSDevice _Device = null;

        #endregion Fields

        #region Operations

        public float[] GetSafeArea()
        {
            UIEdgeInsets sArea = new UIEdgeInsets();
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                sArea = UIApplication.SharedApplication.Delegate.GetWindow().SafeAreaInsets;
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetSafeArea)} - Safe area:{sArea.ToString()}");
            }
            return new float[] { (float)sArea.Left, (float)sArea.Top, (float)sArea.Right, (float)sArea.Bottom };
        }

        public bool HasTopNotch()
        {
            if (null == _Device)
            {
                _Device = new iOSDevice();
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                UIEdgeInsets sArea = UIApplication.SharedApplication.Delegate.GetWindow().SafeAreaInsets;
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(HasTopNotch)} - Safe area: {sArea.ToString()}");
            }
            return _Device.deviceHasNotch();
        }

        public bool IsEmulator()
        {
            return (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR);
        }

        public void ShowStatusBar(bool show, bool animated = false)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                UIApplication.SharedApplication.SetStatusBarHidden(!show, (animated ? UIStatusBarAnimation.Slide : UIStatusBarAnimation.None));
            });
        }

        public void UpdateStatusBarColor(int a, int r, int g, int b)
        {
            try
            {
                UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.Default, false);

                    UIView statusBar = UIApplication.SharedApplication.ValueForKey(new NSString("statusBar")) as UIView;
                    if (statusBar != null)
                    {
                        statusBar.BackgroundColor = UIColor.FromRGBA(r, g, b, a);
                        statusBar.TintColor = UIColor.Black;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(UpdateStatusBarColor)} - Exception: {ex}");
            }
        }

        #endregion Operations

        #region Private

        UIViewController GetCurrentViewController()
        {
            var window = UIApplication.SharedApplication.KeyWindow;
            var vc = window.RootViewController;
            while (vc.PresentedViewController != null) vc = vc.PresentedViewController;
            return vc;
        }

        #endregion
    }
}