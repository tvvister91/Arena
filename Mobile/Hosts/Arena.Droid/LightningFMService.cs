using System.Diagnostics;
using System.Threading;

using Android.App;
using Android.Content;
using Firebase.Messaging;
using MvvmCross;
using MvvmCross.Platforms.Android.Core;
using PE.Shared.Enums;
using PE.Shared.Models;

using Arena.Core.Services;
using Arena.Droid.Helpers;
using System;
using Android.Support.V4.App;
using Arena.Core.Resources;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Arena.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT"})]
    public class LightningFMService : FirebaseMessagingService
    {
        private Random random = new Random();

        public override void OnNewToken(string token)
        {
            Debug.WriteLine($"*** {GetType().Name}.{nameof(OnNewToken)} - Token: {token}");

            var deviceInfo = new DeviceInfo {
                Handle = token,
                Platform = DeviceType.Fcm
            };

            // We aren't guaranteed that Mvx is set up yet.
            var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(Application.Context);
            setup.EnsureInitialized();

            var auth = Mvx.IoCProvider.Resolve<IAuthenticationService>();
            auth.DeviceInfo = deviceInfo;
        }

        public override void OnMessageReceived(RemoteMessage remoteMessage)
        {
            var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(Application.Context);
            setup.EnsureInitialized();

            if (remoteMessage?.Data != null &&
                PushNotificationHelper.TryParse(remoteMessage.Data, out NotificationType type, out string payload) &&
                Mvx.IoCProvider.Resolve<IAppService>() is IAppService appService)
            {
                new Thread(() => appService.RaiseNotificationReceived(type, payload)).Start();
                ShowForegroundNotification(type, payload, remoteMessage.Data);
            }
        }

        private void ShowForegroundNotification(NotificationType type, string payload, IDictionary<string, string> data)
        {
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);

            string notificationText = null;
            string notificationTitle = null;

            switch (type)
            {
                case NotificationType.CreateClaimNotification:
                    notificationText = AppResources.ClaimCreatedNotificationFormat;
                    notificationTitle = AppResources.ClaimCreateTitle;
                    break;

                case NotificationType.DeleteClaimNotification:
                    notificationText = AppResources.ClaimDeletedNotificationFormat;
                    notificationTitle = AppResources.ClaimDeleteTile;
                    break;
                case NotificationType.UpdateClaimNotification:
                    notificationText = AppResources.ClaimUpdatedNotificationFormat;
                    notificationTitle = AppResources.ClaimUpdateTitle;
                    break;
                default:
                    break;
            }

            try
            {
                var intent = new Intent(this, typeof(StartActivity));

                foreach (var key in data.Keys)
                {
                    intent.PutExtra(key, data[key]);
                }

                PendingIntent pIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.UpdateCurrent);

                var payloadBody = JsonConvert.DeserializeObject<BaseClaimPayload>(payload);
                var notification = new NotificationCompat.Builder(this, SplashActivity.CHANNEL_ID)
                    .SetContentIntent(pIntent)
                    .SetSmallIcon(Resource.Drawable.icon_notifcation)
                    .SetContentTitle(notificationTitle)
                    .SetAutoCancel(true)
                    .SetStyle(new NotificationCompat.BigTextStyle()
                    .BigText(string.Format(notificationText, payloadBody.ClaimNo)))
                    .SetContentText(string.Format(notificationText, payloadBody.ClaimNo))
                    .SetPriority((int)Notification.PriorityHigh)
                    .SetDefaults(NotificationCompat.DefaultSound)
                    .SetChannelId(SplashActivity.CHANNEL_ID).Build();

                var id = random.Next();
                notificationManager.Notify(id, notification);
            }
            catch (Exception ex)
            { 
            }
        }
    }
}
