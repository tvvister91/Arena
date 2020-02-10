using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using MvvmCross;
using System;

namespace PE.Plugins.Location.Droid
{
    public class LocationService : Fragment, ILocationService, ILocationListener
    {
        #region Fields

        private readonly LocationManager _Manager;

        private Action<GeoLocation> _LocationChanged;
        private Action<Exception> _LocationError;

        private LocationConfigBase _Config;
        private bool _Disposed = false;

        #endregion Fields

        #region Constructors

        public LocationService(LocationConfig config)
        {
            _Config = config;
            State = LocationServiceState.None;

            _Manager = (LocationManager)config.AppContext.GetSystemService(Context.LocationService);
        }

        ~LocationService()
        {
            Dispose();
        }

        #endregion Constructors

        #region Properties

        public GeoLocation CurrentLocation { get; set; }

        public LocationServiceState State { get; set; }

        public Action StateChanged { get; set; }

        #endregion Properties

        #region Operations

        public GeoLocation GetCurrentLocation()
        {
            string provider = (_Config.Accuracy == LocationAccuracy.Course) ? LocationManager.NetworkProvider : LocationManager.GpsProvider;
            var location = _Manager.GetLastKnownLocation(provider);
            if (location == null) return null;
            return new DroidGeoLocation(location);
        }

        public void RequestAccess(AccessType accessType)
        {
        }

        public void Start(LocationConfigBase config, Action<GeoLocation> locationChanged, Action<Exception> locationError)
        {
            System.Diagnostics.Debug.WriteLine("Starting location service...");

            _LocationChanged = locationChanged;
            _LocationError = locationError;

            _Config = config;

            try
            {
                string provider = (config.Accuracy == LocationAccuracy.Course) ? LocationManager.NetworkProvider : LocationManager.GpsProvider;
                if (!_Manager.IsProviderEnabled(provider))
                {
                    State = LocationServiceState.Error;
                    locationError?.Invoke(new Exception(string.Format("The requested location provider ({0}) is not enabled.", provider)));
                    return;
                }
                
                //  register for location updates
                _Manager.RequestLocationUpdates(provider, config.UpdateInterval, config.MovementThreshold, this);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error staring navigation service\r\n{0}", ex.Message);
                locationError?.Invoke(ex);
                State = LocationServiceState.Error;
            }
        }

        public void Stop()
        {
            _Manager.RemoveUpdates(this);
        }

        #endregion Operations

        #region ILocationListener

        public void OnLocationChanged(Android.Locations.Location location)
        {
            if (_LocationChanged == null) return;
            System.Diagnostics.Debug.WriteLine("*** LocationService.OnLocationChanged - Location changed! ***");

            CurrentLocation = new DroidGeoLocation(location);
            _LocationChanged?.Invoke(CurrentLocation);
        }

        public void OnProviderDisabled(string provider)
        {
            State = LocationServiceState.Disabled;
            StateChanged?.Invoke();
        }

        public void OnProviderEnabled(string provider)
        {
            State = LocationServiceState.Enabled;
            StateChanged?.Invoke();
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            State = (status == Availability.Available) ? LocationServiceState.Enabled : LocationServiceState.Disabled;
            StateChanged?.Invoke();
        }

        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;

            if (_Manager != null)
            {
                _Manager.RemoveUpdates(this);
                _Manager.Dispose();
            }
        }

        #endregion ILocationListener
    }
}