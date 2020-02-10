using System;
using System.Diagnostics;
using System.Threading.Tasks;

using PE.Plugins.Network.Contracts;

namespace Arena.Core.Services
{
    ///
    /// <summary>
    /// This is an example Service. Services are cross cutting concerns that are created as lazy singletons.
    /// 
    /// Services typically contain events, properties and methods that are leveraged by the rest of the app
    /// </summary>
    public class AppService : IAppService
    {
        #region Constants

        private const int DEBOUNCE_DELAY = 700;

        #endregion Constants

        #region Events

        public event EventHandler ConnectivityChanged;
        public event EventHandler DidEnterForeground;
        public event EventHandler OnUserInteractionStopped;
        public event EventHandler OnUserInteractionAcquired;

        #endregion Events

        #region Fields

        private readonly INetworkService _NetworkService;
        private readonly IDatabaseInitializer _DataService;
        private readonly IUserService _UserService;
        private readonly IRestService _RestService;

        private DateTime _StateDate;

        #endregion Fields

        #region Constructors

        public AppService(INetworkService networkService, IDatabaseInitializer dataService, IUserService userService, IRestService restService)
        {
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.ctor - Creating!");

            _NetworkService = networkService;
            _DataService = dataService;
            _UserService = userService;
            _RestService = restService;

            //  wait for authentication to register for push
            _UserService.UserChanged += OnUserChanged; ;
            //  you don't have to remove the event handler because this is a singleton and there will only ever be one instance around
            _NetworkService.OnConnectivityChanged += _NetworkService_OnConnectivityChanged;
            //  the service is now initialized
            Connected = !((_NetworkService.ConnectionState == NetworkConnectionStates.None) || (_NetworkService.ConnectionState == NetworkConnectionStates.Unknown));
            CheckConnectivityChanged().Wait();
        }

        #endregion Constructors

        #region Properties

        public bool Initialized { get; set; } = false;

        public bool Connected { get; set; } = false;

        #endregion Properties

        #region Operations

        private async Task CheckConnectivityChanged()
        {
            var connected = Connected;
            Debug.WriteLine($"*** {GetType().Name}.{nameof(CheckConnectivityChanged)} - Connectivity has changed - waiting for activity to settle ({DEBOUNCE_DELAY}ms...(Old value: {connected})");
            try
            {
                _StateDate = DateTime.Now;
                //  wait for things to settle
                await Task.Delay(DEBOUNCE_DELAY);
                if (DateTime.Now.Subtract(_StateDate).TotalMilliseconds < DEBOUNCE_DELAY) return;
                Debug.WriteLine($"*** {GetType().Name}.{nameof(CheckConnectivityChanged)} - Connectivity has changed...");
                //  check connection state
                var hasConnection = !((_NetworkService.ConnectionState == NetworkConnectionStates.None) || (_NetworkService.ConnectionState == NetworkConnectionStates.Unknown));
                //  check if we are able to ping our backend
                if (!hasConnection)
                {
                    Debug.WriteLine($"*** {GetType().Name}.{nameof(CheckConnectivityChanged)} - Connectivity has changed - NOT connected to the internet...");
                    Connected = hasConnection;
                    if (!Initialized) Initialized = true;
                    return;
                }
                Debug.WriteLine($"*** {GetType().Name}.{nameof(CheckConnectivityChanged)} - Connected to internet, Pinging backend...");
                var result = await _RestService.PingAsync();
                Connected = result;
                Debug.WriteLine($"*** {GetType().Name}.{nameof(CheckConnectivityChanged)} - Ping success: {Connected}...");
                if (!Initialized) Initialized = true;
            }
            catch {  /* do nothing */ }
            finally
            {
                if (connected != Connected)
                {
                    Debug.WriteLine($"*** {GetType().Name}.{nameof(CheckConnectivityChanged)} - Connectivity has changed, raising event.");
                    ConnectivityChanged?.Invoke(this, new EventArgs());
                }
                else
                {
                    Debug.WriteLine($"*** {GetType().Name}.{nameof(CheckConnectivityChanged)} - Nothing has changed.");
                }
            }
        }

        #endregion Operations

        #region Event Handlers

        private void _NetworkService_OnConnectivityChanged(object sender, EventArgs e)
        {
            Task.Run(async () => await CheckConnectivityChanged());
        }

        private void OnUserChanged(object sender, EventArgs e)
        {
            //  TODO: Initialize database now
        }

        #endregion Event Handlers
    }
}
