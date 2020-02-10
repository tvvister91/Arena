using System;

namespace PE.Plugins.Location.Droid
{
    public class DroidGeoLocation : GeoLocation
    {
        public DroidGeoLocation(Android.Locations.Location location)
        {
            TimeStamp = DateTime.Now;
            Latitude = location.Latitude;
            Longitude = location.Longitude;
            Heading = location.Bearing;
            Speed = location.Speed;
        }
    }
}