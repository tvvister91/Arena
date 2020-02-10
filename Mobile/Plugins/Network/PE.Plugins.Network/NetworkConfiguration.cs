using MvvmCross.Plugin;

namespace PE.Plugins.Network
{
    public class NetworkConfiguration : IMvxPluginConfiguration
    {
        /// <summary>
        /// Base URL for all requests
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Secondary url to use if the base url fails. If applicable
        /// </summary>
        public string SecondaryUrl { get; set; }

        /// <summary>
        /// Path to the register controller to register the service id
        /// </summary>
        public string RegisterPath { get; set; }

        /// <summary>
        /// How long in seconds before a request times out
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Name of the Authorization token in the request header
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// Name of the Refresh token in the storage
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// The partial path to the use for heartbeat/reachability. This should NOT be an absolute path as it will be appended to BaseUrl
        /// </summary>
        public string PingPath { get; set; }
    }
}
