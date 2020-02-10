using Android.Content;
using Arena.Core.Enums;
using Arena.Core.Services;
using MvvmCross.Plugin;
using PE.Plugins.Analytics.AppCenterAnalytics;

namespace Arena.Droid.Bootstrap
{
    public class AnalyticsBootstrap
    {
        public static IMvxPluginConfiguration Configure()
        {
            var buildVersionProvider = new BuildVersionProviderService();

#pragma warning disable CS0162 // Unreachable code detected
            switch (buildVersionProvider.Version)
            {
                case BuildVersionEnum.Debug:
                    return new AppCenterConfiguration
                    {
                        AppSecret = "ed7e15c7-8963-460f-92d9-1514b592fe0c"
                    };
                case BuildVersionEnum.Dev:
                    return new AppCenterConfiguration
                    {
                        AppSecret = "ced1ae71-73ce-4ff3-bf0c-755c2edd6b5c"
                    };
                case BuildVersionEnum.QA:
                    return new AppCenterConfiguration
                    {
                        AppSecret = "55fbb571-3c07-46d1-9904-32edc0281c9d"
                    };
                case BuildVersionEnum.UAT:
                    return new AppCenterConfiguration
                    {
                        AppSecret = "eb2a39eb-4256-48b8-8b15-63a81201c58e"
                    };
                case BuildVersionEnum.Release:
                    return new AppCenterConfiguration
                    {
                        AppSecret = "266bca9f-abc0-4a4c-8ad7-cd6d9bd44409"
                    };
                default:
                    return null;
            }
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
