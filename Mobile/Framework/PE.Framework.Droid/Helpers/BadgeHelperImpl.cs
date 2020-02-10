using System;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using PE.Framework.Helpers;
namespace PE.Framework.Droid.Helpers
{
    public class BadgeHelperImpl : IBadgeHelper
    {
        // https://stackoverflow.com/questions/17565307/how-to-display-count-of-notifications-in-app-launcher-icon/17565479#17565479
        //
        public BadgeHelperImpl()
        {
        }

        public void SetBadge(int num)
        {
           // NOP
        }
    }
}
