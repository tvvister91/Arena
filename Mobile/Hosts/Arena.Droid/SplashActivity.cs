using Android.App;
using Android.Content;
using Android.OS;
using MvvmCross.Forms.Platforms.Android.Views;

namespace Arena.Droid
{
    [Activity(Theme = "@style/AppTheme.Splash", MainLauncher = true, NoHistory = true),
    Android.Runtime.Preserve(AllMembers = true)]
    public class SplashActivity : MvxFormsSplashScreenActivity<Setup, Core.CoreApp, UI.FormsApp>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var intent = new Intent(this, typeof(StartActivity));
            StartActivity(intent);
        }
    }
}