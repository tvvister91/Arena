using System;
using System.Xml;
using PE.Framework.Helpers;
using Windows.UI.Notifications;

namespace PE.Framework.UWP.Helpers
{
    public class BadgeHelperImpl : IBadgeHelper
    {
        public void SetBadge(int num)
        {
            if (0 < num)
            {
                //https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/tiles-and-notifications-badges
                var badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
                Windows.Data.Xml.Dom.XmlElement badgeElement = badgeXml.SelectSingleNode("/badge") as Windows.Data.Xml.Dom.XmlElement;
                if (null != badgeElement)
                {
                    badgeElement.SetAttribute("value", num.ToString());
                    //XmlDocument badgeXml = new XmlDocument();
                    //badgeXml.LoadXml(string.Format("<badge value='{0}'/>", badgeNumber));
                    var badge = new BadgeNotification(badgeXml);
                    BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
                }
            }
            else
            {
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
            }
        }
    }
}
