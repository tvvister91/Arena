namespace PE.Plugins.Media.WindowsCommon
{
    public class AudioService : IAudioService
    {
        #region Constructors

        public AudioService(MediaConfig config)
        {
            Recorder = new WinMediaRecorder(config.RecordingFormat);
        }

        #endregion Constructors

        #region Properties

        public IMediaRecorder Recorder { get; private set; }

        public IMediaPlayer Player { get; } = new WinMediaPlayer();

        #endregion Properties
    }
}
