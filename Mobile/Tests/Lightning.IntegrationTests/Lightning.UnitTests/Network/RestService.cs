using System;
using PE.Plugins.Network;
using PE.Plugins.Network.Contracts;

namespace Lightning.IntegrationTests.Network
{
    public class RestService : RestServiceBase, IRestService
    {
        public static NetworkConfiguration configuration = new NetworkConfiguration
        {
            BaseUrl = "https://dev.lightning.sedgwick.com",
            AuthToken = "Bearer",
            RefreshToken = "Refresh",
            PingPath = "api/v1/Test/ping",
            Timeout = 30
        };

        public RestService() : base(configuration)
        {
        }
    }
}
