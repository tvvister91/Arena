using CoreLocation;
using ObjCRuntime;
using System;
using System.Linq;

namespace PE.Plugins.Location.iOS
{
    public class LocationService : ILocationService
    {
        #region Fields

        private CLLocationManager _Manager;
        private bool _Disposed = false;
        private AccessType _AccessType = AccessType.Partial;

        private Action<GeoLocation> _LocationChanged;
        private Action<Exception> _LocationError;

        #endregion Fields

        #region Constructors

        public LocationService()
        {
            _Manager = new CLLocationManager();
            _Manager.AuthorizationChanged += OnAuthorizationChanged;
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
            _Manager.RequestLocation();
            return (CurrentLocation != null) ? CurrentLocation : null;
        }

        public void RequestAccess(AccessType accessType)
        {
            _AccessType = accessType;
            var s = (accessType == AccessType.Full) ? "requestAlwaysAuthorization" : "requestWhenInUseAuthorization";
            if (_Manager.RespondsToSelector(new Selector(s)))
            {
                _Manager.RequestAlwaysAuthorization();
            }
        }

        public void Start(LocationConfigBase config, Action<GeoLocation> locationChanged, Action<Exception> locationError)
        {
            try
            {
                if (_Manager == null) _Manager = new CLLocationManager();

                _LocationChanged = locationChanged;
                _LocationError = locationError;

                System.Diagnostics.Debug.WriteLine("Starting location service");

                //  register app with this service ALWAYS
                if (_Manager.RespondsToSelector(new Selector("requestAlwaysAuthorization")))
                {
                    _Manager.RequestAlwaysAuthorization();
                }

                if (!CLLocationManager.LocationServicesEnabled)
                {
                    System.Diagnostics.Debug.WriteLine("Location services are disabled. Requesting permission.");
                    if (!CLLocationManager.LocationServicesEnabled)
                    {
                        System.Diagnostics.Debug.WriteLine("Location services are still disabled.");
                        State = LocationServiceState.Disabled;
                        if (_LocationError != null) _LocationError(new Exception("Location services are disabled."));
                        return;
                    }
                }

                _Manager.LocationsUpdated += _Manager_LocationsUpdated;
                _Manager.UpdatedLocation += _Manager_UpdatedLocation;
                _Manager.DesiredAccuracy = config.MovementThreshold;
                _Manager.DistanceFilter = config.MovementThreshold;

                _Manager.StartUpdatingLocation();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error starting location services\r\n{0}", ex.Message);
                State = LocationServiceState.Error;
                if (_LocationError != null) _LocationError(ex);
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        #endregion Operations

        #region Event Handlers

        void _Manager_LocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {
            if (_LocationChanged == null) return;

            System.Diagnostics.Debug.WriteLine("{0} locations returned", e.Locations.Length);

            foreach (var location in e.Locations.OrderBy(l => l.Timestamp))
            {
                if (location == null) return;

                CurrentLocation = new GeoLocation { Latitude = location.Coordinate.Latitude, Longitude = location.Coordinate.Longitude };
                _LocationChanged(CurrentLocation);
            }
        }

        void _Manager_UpdatedLocation(object sender, CLLocationUpdatedEventArgs e)
        {
            if (_LocationChanged == null) return;
            System.Diagnostics.Debug.WriteLine("Location updated.");

            CurrentLocation = new GeoLocation { Latitude = e.NewLocation.Coordinate.Latitude, Longitude = e.NewLocation.Coordinate.Longitude };
            _LocationChanged(CurrentLocation);
        }

        private void OnAuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e)
        {
            if (_AccessType == AccessType.Full)
            {
                State = ((e.Status == CLAuthorizationStatus.Authorized) || (e.Status == CLAuthorizationStatus.AuthorizedAlways)) ? LocationServiceState.Enabled : LocationServiceState.Disabled;
            }
            else if (_AccessType == AccessType.Partial)
            {
                State = ((e.Status == CLAuthorizationStatus.Authorized) || (e.Status == CLAuthorizationStatus.AuthorizedAlways) || (e.Status == CLAuthorizationStatus.AuthorizedWhenInUse)) ? LocationServiceState.Enabled : LocationServiceState.Disabled;
            }
        }

        #endregion Event Handlers
    }
}
