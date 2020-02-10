using System;

namespace PE.Plugins.Media
{
    public interface IMediaPlayer
    {
        #region Events

        event EventHandler<PlaybackEventArgs> PlaybackStatus;
        event EventHandler PlayerStatus;

        #endregion Events

        #region Properties

        bool PlayerReady { get; }

        #endregion Properties

        #region Operations

        void PlayPrepare(string fileName);

        void PlayStart();

        void PlayStop();

        #endregion Operations
    }
}
