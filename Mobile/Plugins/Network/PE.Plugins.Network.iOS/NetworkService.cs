using CoreFoundation;
using CoreTelephony;
using PE.Framework.Models;
using PE.Plugins.Network.Contracts;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SystemConfiguration;
using UIKit;

namespace PE.Plugins.Network.iOS
{
    public class NetworkService : INetworkService, IDisposable
    {
        #region Events

        public event EventHandler OnConnectivityChanged;

        #endregion Events

        #region Fields

        private NetworkReachability _Reachability;

        private string _Host = string.Empty;
        private string _Port = "80";

        private NetworkReachabilityFlags _LastFlags = NetworkReachabilityFlags.InterventionRequired;

        private CancellationTokenSource _MasterSource;
        private CancellationToken _MasterToken;

        #endregion Fields

        #region Constructors

        public NetworkService(NetworkConfiguration configuration)
        {
            Configuration = configuration;

            _MasterSource = new CancellationTokenSource();
            _MasterToken = _MasterSource.Token;

            Reset();
        }

        ~NetworkService()
        {
            _MasterSource.Cancel();
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
            //UIApplication.SharedApplication.InvokeOnMainThread(() =>
            //{
                try
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.Init - Reachability on Main Thread");

                    while (_Reachability == null)
                    {
                        Thread.Sleep(10);
                        _Reachability = SetReachability();
                    }

                    NetworkReachabilityFlags flags;
                    if (_Reachability.TryGetFlags(out flags))
                    {
                        System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.Init - Initial reachability: {flags}");
                        var connected = flags.HasFlag(NetworkReachabilityFlags.Reachable);
                        if (connected)
                        {
                            GetNetworkSetup(flags);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.Init - Exception: {ex}");
                }

                OnConnectivityChanged?.Invoke(this, new EventArgs());
            //});
        }

        private NetworkReachability SetReachability()
        {
            NetworkReachability reachability = new NetworkReachability(IPAddress.Any);
            var status = reachability.SetNotification(ReachablilityCallback);
            bool isScheduled = reachability.Schedule(CFRunLoop.Main, CFRunLoop.ModeDefault);
            if (!isScheduled)
            {
                System.Diagnostics.Debug.WriteLine($"NetworkReachability request was not scheduled");
                reachability.Dispose();
                reachability = null;
            }

            return reachability;
        }

        #endregion Initialization

        #region Operations

        public Task GetNetworksAsync(bool repeat, int intervalMs, Action<ServiceResult> complete)
        {
            throw new NotImplementedException();
        }

        public void StopNetworkMonitor()
        {
        }

        public void Reset()
        {
            System.Diagnostics.Debug.WriteLine(string.Format("*** NetworkManager.Reset - Reset to base - {0}", Configuration.BaseUrl));
            BaseUrl = Configuration.BaseUrl;
            SecondaryUrl = (string.IsNullOrEmpty(Configuration.SecondaryUrl)) ? Configuration.BaseUrl : Configuration.SecondaryUrl;
            //Task.Run(() => Init());
            new Thread(Init).Start();
        }

        #endregion Operations

        #region Private Methods

        private void ReachablilityCallback(NetworkReachabilityFlags flags)
        {
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.ReachablilityCallback - Callback!!");
            var connected = flags.HasFlag(NetworkReachabilityFlags.Reachable);
            if (connected)
            {
                GetNetworkSetup(flags);
            }
            else
            {
                ConnectionState = NetworkConnectionStates.None;
            }
            OnConnectivityChanged?.Invoke(this, new EventArgs());
        }

        private void GetNetworkSetup(NetworkReachabilityFlags flags)
        {
            //  we're on wifi here
            if (flags.HasFlag(NetworkReachabilityFlags.IsWWAN))
            {
                //  what type of connectivity is available
                CTTelephonyNetworkInfo info = new CTTelephonyNetworkInfo();
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(GetNetworkSetup)} - Cellular detected: {info.DebugDescription}");
                if ((info.CurrentRadioAccessTechnology == CTRadioAccessTechnology.GPRS) || (info.CurrentRadioAccessTechnology == CTRadioAccessTechnology.Edge))
                {
                    ConnectionState = NetworkConnectionStates.CellularSlow;
                    System.Diagnostics.Debug.WriteLine("Network Service - Connection speed designated to be SLOW.");
                }
                else
                {
                    ConnectionState = NetworkConnectionStates.CellularFast;
                    System.Diagnostics.Debug.WriteLine("Network Service - Connection speed designated to be FAST.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Network Service - WIFI detected.");
                ConnectionState = NetworkConnectionStates.Wifi;
            }
        }

        #endregion Private Methods

        #region Cleanup

        public void Dispose()
        {
            if (_Reachability == null) return;
            _Reachability.Unschedule();
            _Reachability.Dispose();
        }

        #endregion Cleanup
    }
}