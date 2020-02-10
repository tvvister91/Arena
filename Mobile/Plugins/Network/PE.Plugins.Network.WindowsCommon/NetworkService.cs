using PE.Framework.Models;
using PE.Plugins.Network.Contracts;
using PE.Plugins.Network.Models;

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;

namespace PE.Plugins.Network.WindowsCommon
{
    public class NetworkService : INetworkService
    {

        #region Constants

        private const int ETHERNET = 6;

        #endregion Constants

        #region Events

        public event EventHandler OnConnectivityChanged;

        #endregion Events

        #region Fields

        private bool _FirstCallMade = false;
        private bool _StopScan = false;

        NetworkConfiguration _Configuration;

        #endregion Fields

        #region Constructors

        public NetworkService(NetworkConfiguration configuration)
        {
            BaseUrl = configuration.BaseUrl;
            _Configuration = configuration;
            Init();
        }

        ~NetworkService()
        {
            NetworkInformation.NetworkStatusChanged -= NetworkInformation_NetworkStatusChanged;
        }

        #endregion Constructors

        #region Properties

        private NetworkConnectionStates _ConnectionState = NetworkConnectionStates.None;
        public NetworkConnectionStates ConnectionState
        {
            get { return _ConnectionState; }
            set { _ConnectionState = value; }
        }

        public string BaseUrl { get; set; }

        public string SecondaryUrl { get; set; }

        public bool IsOnSecondary { get; set; }

        public bool IsFailover { get; set; }

        #endregion Properties

        #region Init

        private void Init()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            GetStatus();
        }

        private void GetStatus()
        {
            System.Diagnostics.Debug.WriteLine("NetworkManager: Get connection status");
            var profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile == null)
            {
                System.Diagnostics.Debug.WriteLine("NetworkManager: No connection found");
                _ConnectionState = NetworkConnectionStates.None;
                RaiseInitStatus();
                return;
            }
            System.Diagnostics.Debug.WriteLine(string.Format("NetworkManager: Connection found - {0}", profile.ProfileName));
            //  check 3g vs 2g
            if (profile.IsWwanConnectionProfile)
            {
                _ConnectionState = (profile.NetworkAdapter.InboundMaxBitsPerSecond < 171000) ? NetworkConnectionStates.CellularSlow : NetworkConnectionStates.CellularFast;
            }
            else if (profile.IsWlanConnectionProfile)
            {
                _ConnectionState = NetworkConnectionStates.Wifi;
            }
            else if(profile.NetworkAdapter != null &&
                profile.NetworkAdapter.IanaInterfaceType == ETHERNET)
            {
                _ConnectionState = NetworkConnectionStates.Ethernet;
            }
            else
            {
                bool isInternetConnected = NetworkInterface.GetIsNetworkAvailable();
                _ConnectionState = NetworkConnectionStates.Unknown;
            }

            RaiseInitStatus();
        }

        private async Task RaiseInitStatus()
        {
            System.Diagnostics.Debug.WriteLine("NetworkManager: Raise init status");
            DateTime start = DateTime.Now;
            while (!_FirstCallMade)
            {
                //  timeout - 10 mins
                if (DateTime.Now.Subtract(start).TotalMinutes > 10)
                {
                    _FirstCallMade = true;
                    return;
                }

                //  raise the event
                if (OnConnectivityChanged != null)
                {
                    _FirstCallMade = true;
                    System.Diagnostics.Debug.WriteLine("NetworkManager: raising event");
                    OnConnectivityChanged(this, new EventArgs());
                    return;
                }
                System.Diagnostics.Debug.WriteLine("NetworkManager: Init status complete.");
                //  wait 5 seconds and try again
                await Task.Delay(5000);
            }
        }

        #endregion Init

        #region Operations

        public async Task GetNetworksAsync(bool repeat, int intervalMs, Action<ServiceResult> complete)
        {
            if (complete == null) return;
            try
            {
                //  check if we have access to wifi
                var status = await WiFiAdapter.RequestAccessAsync();
                if (status != WiFiAccessStatus.Allowed) complete(new ServiceResult { Status = ServiceResultStatus.Failure, Message = "Access to WIFI is denied." });
                //  get a list of adapters
                var adapters = await WiFiAdapter.FindAllAdaptersAsync();
                foreach (var adapter in adapters)
                {
                    adapter.AvailableNetworksChanged += (a, o) =>
                    {
                        var signals = new List<NetworkSignal>();
                        foreach (var network in a.NetworkReport.AvailableNetworks)
                        {
                            signals.Add(new NetworkSignal { MacAddress = network.Bssid, Ssid = (string.IsNullOrEmpty(network.Ssid)) ? "[hidden]" : network.Ssid, Level = network.NetworkRssiInDecibelMilliwatts });
                        }
                        Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            complete(new ServiceResult<List<NetworkSignal>> { Status = ServiceResultStatus.Success, Payload = signals });
                        });
                    };
                    await adapter.ScanAsync();
                }
            }
            catch (Exception ex)
            {
                complete(new ServiceResult<Exception> { Status = ServiceResultStatus.Error, Payload = ex });
            }
        }

        public void StopNetworkMonitor()
        {
        }

        public void Reset()
        {
            System.Diagnostics.Debug.WriteLine(string.Format("*** NetworkManager.Reset - Reset to base - {0}", _Configuration.BaseUrl));
            BaseUrl = _Configuration.BaseUrl;
            SecondaryUrl = (string.IsNullOrEmpty(_Configuration.SecondaryUrl)) ? _Configuration.BaseUrl : _Configuration.SecondaryUrl;
            Init();
        }

        #endregion Operations

        #region Event Handlers

        void NetworkInformation_NetworkStatusChanged(object sender)
        {
            System.Diagnostics.Debug.WriteLine("NetworkManager: Network status changed");
            //  get the new status
            GetStatus();
            //  notify
            OnConnectivityChanged?.Invoke(this, new EventArgs());
        }

        #endregion Event Handlers
    }
}
