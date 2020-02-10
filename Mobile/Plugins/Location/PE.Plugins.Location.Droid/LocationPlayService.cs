using Android;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.Support.V4.Content;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PE.Plugins.Location.Droid
{
    public class LocationPlayService : LocationCallback, ILocationService
    {
        #region Fields

        private LocationConfigBase _Config;
        private FusedLocationProviderClient _Client;

        private Action<GeoLocation> _LocationChanged;
        private Action<Exception> _LocationError;

        #endregion Fields

        #region Constructors

        public LocationPlayService(LocationConfig config)
        {
            _Config = config;
            //  check if play services is enabled
            if (_Config.CheckEnabled == null) throw new Exception("Unable to check whether play services are available. Make sure you have a CheckEnabled Action.");
            State = (_Config.CheckEnabled()) ? LocationServiceState.Enabled : LocationServiceState.Disabled;
            _Client = LocationServices.GetFusedLocationProviderClient(config.AppContext);
            if (_Client == null) State = LocationServiceState.Disabled;
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
            return Task.Run(async () =>
            {
                //  assumes that the necessary run-time permission checks have succeeded
                var location = await _Client.GetLastLocationAsync();
                //  nothing!?
                if (location == null) return null;
                //  return as a generic object
                return new GeoLocation
                {
                    Heading = location.Bearing,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Speed = location.Speed,
                    TimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(location.Time)
                };
            }).Result;
        }

        public void RequestAccess(AccessType accessType)
        {
            ContextCompat.CheckSelfPermission(((LocationConfig)_Config).AppContext, Manifest.Permission.AccessFineLocation);
        }

        public void Start(LocationConfigBase config, Action<GeoLocation> locationChanged, Action<Exception> locationError)
        {
            _LocationChanged = locationChanged;
            _LocationError = locationError;

            LocationRequest request = new LocationRequest().SetPriority((config.Accuracy == LocationAccuracy.Course) ? LocationRequest.PriorityLowPower : LocationRequest.PriorityHighAccuracy);
            if (config.MovementThreshold > 0)
            {
                request.SetSmallestDisplacement(config.MovementThreshold);
            }
            else
            {
                request.SetInterval(config.UpdateInterval).SetFastestInterval(config.UpdateInterval);
            }
            Task.Run(async () => await _Client.RequestLocationUpdatesAsync(request, this));
        }

        public void Stop()
        {
            _LocationChanged = null;
            _LocationError = null;
            Task.Run(async () => await _Client.RemoveLocationUpdatesAsync(this));
        }

        #endregion Operations

        #region LocationCallback

        public override void OnLocationAvailability(LocationAvailability availability)
        {
            //  avoid false triggers
            if (availability.IsLocationAvailable && (State == LocationServiceState.Enabled)) return;
            if (!availability.IsLocationAvailable && (State == LocationServiceState.Disabled)) return;
            State = (availability.IsLocationAvailable) ? LocationServiceState.Enabled : LocationServiceState.Disabled;
            StateChanged?.Invoke();
        }

        public override void OnLocationResult(LocationResult result)
        {
            var location = result.Locations.FirstOrDefault();
            if (location == null) return;
            CurrentLocation = ConvertLocation(location);
            _LocationChanged?.Invoke(CurrentLocation);
        }

        #endregion LocationCallback

        #region Private

        private GeoLocation ConvertLocation(Android.Locations.Location location)
        {
            return new GeoLocation
            {
                Heading = location.Bearing,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Speed = location.Speed,
                TimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(location.Time)
            };
        }

        #endregion Private
    }
}