using System;

namespace PE.Plugins.Media
{
    public class PlaybackEventArgs : EventArgs
    {
        #region Properties

        public bool Stopped { get; set; }

        public double Duration { get; set; }

        public double CurrentTime { get; set; }

        public bool HasError { get; set; }

        public bool IsNew { get; set; } // TODO: replace it w/ new EventArgs (w/ duration) for PlayerStatus

        #endregion Properties
    }
}
