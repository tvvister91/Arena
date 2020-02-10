using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.Telephony;
using MvvmCross.Platforms.Android;
using PE.Framework.Models;
using PE.Plugins.Network.Contracts;
using PE.Plugins.Network.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PE.Plugins.Network.Droid
{
    public class NetworkService : BroadcastReceiver, INetworkService, IDisposable
    {
        #region Events

        public event EventHandler OnConnectivityChanged;

        #endregion Events

        #region Fields

        private readonly IMvxAndroidGlobals _Globals;
        private readonly ConnectivityManager _Manager;
        private readonly WifiManager _WifiManager;

        private bool _StopMonitor = false;
        private DateTime _Recieved = DateTime.MinValue;

        private bool _Initialized = false;

        #endregion Fields

        #region Constructors

        public NetworkService(NetworkConfigurationDroid configuration)
        {
            BaseUrl = configuration.BaseUrl;

            _Manager = (ConnectivityManager)Application.Context.GetSystemService(Application.ConnectivityService);
            _WifiManager = (WifiManager)Application.Context.GetSystemService(Application.WifiService);
            Configuration = configuration;

            Init();
        }

        #endregion Constructors

        #region Properties

        public NetworkConnectionStates ConnectionState { get; set; }

        public string BaseUrl { get; set; }

        public string SecondaryUrl { get; set; }

        public bool IsOnSecondary { get; set; }

        public bool IsFailover { get; set; }

        public NetworkConfiguration Configuration { get; private set; }

        #endregion Properties

        #region Initialization

        private void Init()
        {
            ConnectionState = NetworkConnectionStates.None;

            if (!_Initialized)
            {
                ((NetworkConfigurationDroid)Configuration).ApplicationContext.RegisterReceiver(this, new IntentFilter(ConnectivityManager.ConnectivityAction));
                _Initialized = true;
            }

            Task.Run(() => GetState());
        }

        void IDisposable.Dispose()
        {
            _Manager.Dispose();
        }

        #endregion Initialization

        #region Operations

        public async Task GetNetworksAsync(bool repeat, int intervalMs, Action<ServiceResult> complete)
        {
            _StopMonitor = false;
            //  no point in wasting CPU cycles if no-one is listening
            if (complete == null) return;
            while (true)
            {
                try
                {
                    //  see if we can enumerate all nearby wifis
                    var success = _WifiManager.StartScan();
                    if (!success) complete(new ServiceResult { Status = ServiceResultStatus.Failure, Message = "Check that Wifi is enabled." });
                    complete(new ServiceResult<List<NetworkSignal>> { Status = ServiceResultStatus.Success, Payload = _WifiManager.ScanResults.Select(r => new NetworkSignal { MacAddress = r.Bssid, Ssid = r.Ssid, Level = r.Level }).ToList() });
                    if (!repeat || _StopMonitor) return;
                    await Task.Delay(intervalMs);
                }
                catch (Exception ex)
                {
                    complete(new ServiceResult<Exception> { Status = ServiceResultStatus.Error, Payload = ex });
                }
            }
        }

        public void StopNetworkMonitor()
        {
            _StopMonitor = true;
        }

        public void Reset()
        {
            System.Diagnostics.Debug.WriteLine(string.Format("*** NetworkService.Reset - Reset to base - {0}", Configuration.BaseUrl));
            BaseUrl = Configuration.BaseUrl;
            SecondaryUrl = (string.IsNullOrEmpty(Configuration.SecondaryUrl)) ? Configuration.BaseUrl : Configuration.SecondaryUrl;
            GetState();
        }

        #endregion Operations

        #region Private Methods

        public override void OnReceive(Context context, Intent intent)
        {
            Task.Run(() => GetState()).ConfigureAwait(false);
        }

        private async Task GetState()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("*** NetworkService - Connectivity changed.");
                _Recieved = DateTime.Now;

                //  allow changes to settle
                await Task.Delay(500);
                var diffs = DateTime.Now.Subtract(_Recieved).TotalMilliseconds;
                System.Diagnostics.Debug.WriteLine(string.Format("*** NetworkService.GetState - Difference = {0}", diffs));
                if (diffs < 500) return;

                System.Diagnostics.Debug.WriteLine("*** NetworkService - Checking connectivity.");
                System.Diagnostics.Debug.WriteLine("*** NetworkService - Getting telephone service.");
                var tel = (TelephonyManager)Application.Context.GetSystemService(Application.TelephonyService);

                NetworkInfo info = _Manager.ActiveNetworkInfo;
                if (info == null)
                {
                    System.Diagnostics.Debug.WriteLine("*** NetworkService - No connectivity.");
                    ConnectionState = NetworkConnectionStates.None;
                    OnConnectivityChanged?.Invoke(this, new EventArgs());
                    return;
                }

                if ((info.Type == ConnectivityType.Ethernet) || (info.Type == ConnectivityType.Wifi))
                {
                    System.Diagnostics.Debug.WriteLine("*** NetworkService - On WIFI.");
                    ConnectionState = NetworkConnectionStates.Wifi;
                }
                else if (info.Type == ConnectivityType.Mobile)
                {
                    System.Diagnostics.Debug.WriteLine("*** NetworkService - On Cellular.");
                    ConnectionState = NetworkConnectionStates.CellularFast;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("*** NetworkService - Connectivity unknown.");
                    ConnectionState = NetworkConnectionStates.Unknown;
                }

                OnConnectivityChanged?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                ConnectionState = NetworkConnectionStates.Unknown;
                System.Diagnostics.Debug.WriteLine("NetworkService - Failed to get network state: {0}", ex.Message);
                OnConnectivityChanged?.Invoke(this, new EventArgs());
            }
        }

        #endregion Private Methods
    }
}