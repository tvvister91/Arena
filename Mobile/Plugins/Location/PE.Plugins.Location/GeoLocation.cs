using System;
using System.Runtime.Serialization;

namespace PE.Plugins.Location
{
    [DataContract]
    public class GeoLocation
    {
        #region Constructors

        public GeoLocation()
        {
            TimeStamp = DateTime.Now;
        }

        public GeoLocation(string data)
        {
            if (string.IsNullOrEmpty(data)) return;
            if (data.StartsWith("POINT")) data = data.Substring(7, data.Length - 8);
            var parts = data.Split(new char[] { (char)32 });
            Latitude = double.Parse(parts[0]);
            Longitude = double.Parse(parts[1]);
        }

        #endregion Constructors

        #region Properties

        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Longitude { get; set; }

        [DataMember]
        public double Heading { get; set; }

        [DataMember]
        public double Speed { get; set; }

        #endregion Properties

        #region Public

        public override string ToString()
        {
            return string.Format("{0} {1}", Latitude, Longitude);
        }

        #endregion Public
    }
}
