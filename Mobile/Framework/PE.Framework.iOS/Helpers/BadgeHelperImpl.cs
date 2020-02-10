using System;
using PE.Framework.Helpers;
using UIKit;
namespace PE.Framework.iOS.Helpers
{
    public class BadgeHelperImpl : IBadgeHelper
    {
        public void SetBadge(int num)
        {
            if (0 < num)
            {
                UIApplication.SharedApplication.ApplicationIconBadgeNumber = num;
            }
            else
            {
                UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
            }
        }
    }
}
