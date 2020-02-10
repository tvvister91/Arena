using MvvmCross.Plugin;
using PE.Plugins.Analytics.AppCenterAnalytics;
using Arena.Core.Enums;
using MvvmCross;
using Arena.Core.Services;

namespace Arena.iOS.Bootstrap
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
                        AppSecret = "c9223d8c-8ccc-4ee6-8f01-5d35a5c5ea01"
                    };
                case BuildVersionEnum.Dev:
                    return new AppCenterConfiguration
                    {
                        AppSecret = "7f682b63-fd4e-40a1-880c-9b1b16735e13"
                    };
                case BuildVersionEnum.QA:
                    return new AppCenterConfiguration
                    {
                        AppSecret = "a05ccd8e-ea1d-41c1-86ab-e4e8284d274a"
                    };
                case BuildVersionEnum.UAT:
                    return new AppCenterConfiguration
                    {
                        AppSecret = "97869653-dd29-4b25-b970-7651e4c15bd3"
                    };
                case BuildVersionEnum.Release:
                    return new AppCenterConfiguration
                    {
                        AppSecret = "a5082484-4bb4-40eb-a9f4-b82a385a6939"
                    };
                default:
                    return null;
            }
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}