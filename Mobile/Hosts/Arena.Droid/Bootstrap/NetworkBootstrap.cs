using Android.Content;
using Arena.Core.Enums;
using Arena.Core.Services;
using MvvmCross.Plugin;
using PE.Plugins.Network.Droid;

namespace Arena.Droid.Bootstrap
{
    public class NetworkBootstrap
    {
        public static IMvxPluginConfiguration Configure()
        {
            var provider = new BuildVersionProviderService();

            string baseURL;
#pragma warning disable CS0162 // Unreachable code detected
            switch (provider.Version)
            {
                case BuildVersionEnum.Debug:
                case BuildVersionEnum.Dev:
                case BuildVersionEnum.QA:
                    baseURL = "";
                    break;
                case BuildVersionEnum.UAT:
                    baseURL = "";
                    break;
                case BuildVersionEnum.Release:
                    baseURL = "";
                    break;
                default:
                    baseURL = "";
                    break;
            }
#pragma warning restore CS0162 // Unreachable code detected

            return new NetworkConfigurationDroid
            {
                BaseUrl = baseURL,
                AuthToken = "Bearer",
                RefreshToken = "Refresh",
                PingPath = "api/v1/Test/ping",
                Timeout = 30,
                ApplicationContext = Android.App.Application.Context
            };
        }
    }
}
