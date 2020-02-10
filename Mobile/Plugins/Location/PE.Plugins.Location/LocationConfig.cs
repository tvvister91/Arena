using MvvmCross.Plugin;
using System;
using System.Runtime.Serialization;

namespace PE.Plugins.Location
{
    public enum LocationAccuracy
    {
        Course,
        Fine
    }

    public enum AccessType
    {
        None,
        Partial,
        Full
    }

    public enum LocationServiceState
    {
        /// <summary>
        /// The service has not been interacted with - default
        /// </summary>
        None,
        /// <summary>
        /// The service has been started and is running
        /// </summary>
        Started,
        /// <summary>
        /// The service was started but has been stopped
        /// </summary>
        Stopped,
        /// <summary>
        /// The service encountered an error while starting
        /// </summary>
        Error,
        /// <summary>
        /// Location Services are disabled on the device
        /// </summary>
        Disabled,
        /// <summary>
        /// Location Services as enabled on the device
        /// </summary>
        Enabled
    }

    [DataContract]
    public class LocationConfigBase 
    {
        [DataMember]
        public LocationAccuracy Accuracy { get; set; }

        [DataMember]
        public int AccuracyMeters{ get; set; }

        [DataMember]
        public int UpdateInterval { get; set; }

        public float MovementThreshold { get; set; }    

        public Func<bool> CheckEnabled { get; set; }
    }
}
