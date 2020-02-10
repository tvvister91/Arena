using System;
using Arena.Core.Services;
using PE.Shared.Enums;

namespace Arena.Droid
{
    public class LaunchedNotificationCheckerDroid : ILaunchedNotificationChecker
    {
        public (NotificationType type, string payloadString)? CheckLaunchedFromNotification() => null;
    }
}