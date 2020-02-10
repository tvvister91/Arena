using System;

namespace PE.Plugins.Location
{
    public interface ILocationService
    {
        #region Properties

        GeoLocation CurrentLocation { get; set; }

        LocationServiceState State { get; set; }

        Action StateChanged { get; set; }

        #endregion Properties

        #region Operations

        void Start(LocationConfigBase config, Action<GeoLocation> locationChanged, Action<Exception> locationError);

        void Stop();

        void RequestAccess(AccessType accessType);

        GeoLocation GetCurrentLocation();

        #endregion Operations
    }
}
