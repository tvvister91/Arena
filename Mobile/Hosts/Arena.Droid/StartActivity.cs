using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Arena.Core.Services;
using Arena.Core.ViewModels;
using Arena.Droid.Helpers;
using Arena.UI;
using Microsoft.Identity.Client;
using MvvmCross;
using MvvmCross.Forms.Platforms.Android.Views;
using PE.Shared.Enums;
using Plugin.Permissions;
using System;
using System.Linq;
using System.Threading;
using Xamarin.Essentials;

namespace Arena.Droid
{
    [Activity(
        Name = "com.Arena.MainActivity",
        Label = "Lightning",
        Icon = "@mipmap/icon",
        Theme = "@style/AppTheme",
        MainLauncher = false,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask,
        ScreenOrientation = ScreenOrientation.Portrait),
        Android.Runtime.Preserve(AllMembers = true)]
    public class StartActivity : MvxFormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            InitializeForms(bundle);
            Window.SetSoftInputMode(SoftInput.AdjustResize | SoftInput.StateAlwaysHidden);

            var versionProvider = new BuildVersionProviderService();

            if (versionProvider.Version == Core.Enums.BuildVersionEnum.Release)
            {
                Window.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
            }

            base.OnCreate(bundle);
        }
    }
}
