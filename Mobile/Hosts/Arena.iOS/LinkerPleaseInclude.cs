namespace Arena.iOS
{
    [Foundation.Preserve(AllMembers = true)]
    public class LinkerPleaseInclude
    {
        public void Include(PE.Plugins.Analytics.AppCenterAnalytics.AnalyticsService service)
        {
            service = new PE.Plugins.Analytics.AppCenterAnalytics.iOS.iOSAnalyticsService(null);
        }

        public void Include(PE.Plugins.Dialogs.iOS.DialogService service)
        {
            service = new PE.Plugins.Dialogs.iOS.DialogService(null);
        }

        public void Include(PE.Plugins.LocalStorage.iOS.LocalStorageService service)
        {
            service = new PE.Plugins.LocalStorage.iOS.LocalStorageService();
        }

        public void Include(PE.Plugins.Network.iOS.NetworkService service)
        {
            service = new PE.Plugins.Network.iOS.NetworkService(null);
        }

        public void Include(PE.Plugins.Network.iOS.RestService service)
        {
            service = new PE.Plugins.Network.iOS.RestService(null);
        }

        public void Include(PE.Plugins.Validation.ValidationService service)
        {
            service = new PE.Plugins.Validation.ValidationService(null);
        }
    }
}
