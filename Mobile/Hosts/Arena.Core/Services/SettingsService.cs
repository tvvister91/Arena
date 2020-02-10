using System;
using Plugin.Permissions.Abstractions;
using Xamarin.Essentials;

namespace Arena.Core.Services
{
    public class SettingsService : ISettingsService
    {
        public const string FIRST_TIME_USE_KEY_BASE = "FIRST_TIME_USE_PERMISSION";

        public bool CheckFirstTimeUse(Permission permission)
        {
            return CheckFirstTimeKey($"{FIRST_TIME_USE_KEY_BASE}_{permission.ToString()}");
        }

        private bool CheckFirstTimeKey(string key)
        {
            var isFTU = GetBool(key, true);

            if (isFTU)
            {
                SetBool(key, false);
            }

            return isFTU;
        }

        private bool GetBool(string key, bool defaultValue)
        {
            return Preferences.Get(key, defaultValue);
        }

        private void SetBool(string key, bool value)
        {
            Preferences.Set(key, value);
        }
    }
}
