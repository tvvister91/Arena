using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Foundation;
using MvvmCross;
using MvvmCross.Forms.Platforms.Ios.Core;
using MvvmCross.Platforms.Ios.Core;
using Security;
using UIKit;
using UserNotifications;

using Arena.Core.Services;

namespace Arena.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : MvxFormsApplicationDelegate<Setup, Core.CoreApp, UI.FormsApp>
    {
        private static string FIRST_LAUNCH_KEY = "FIRST_LAUNCH_KEY";

        // This code should clear keychain token when app was reinstalled.
        // Before this the app would keep the token in keychain which is persisted between app reinstalled
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            if (NSUserDefaults.StandardUserDefaults.StringForKey(FIRST_LAUNCH_KEY) == null)
            {
                NSUserDefaults.StandardUserDefaults.SetString("first_run", FIRST_LAUNCH_KEY);

                var securityRecords = new[] { SecKind.GenericPassword,
                                    SecKind.Certificate,
                                    SecKind.Identity,
                                    SecKind.InternetPassword,
                                    SecKind.Key
                                };

                foreach (var recordKind in securityRecords)
                {
                    SecRecord query = new SecRecord(recordKind);
                    SecKeyChain.Remove(query);
                }
            }

            var result = base.FinishedLaunching(app, options);

            return result;
        }
    }
}