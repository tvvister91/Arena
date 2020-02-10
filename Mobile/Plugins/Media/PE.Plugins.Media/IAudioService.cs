using System;

namespace PE.Plugins.Media
{
    public enum MediaFormat
    {
        WAV,
        MP3
    }

    public interface IAudioService
    {
        #region Properties

        IMediaRecorder Recorder { get; }

        IMediaPlayer Player { get; }

        #endregion Properties
    }
}
