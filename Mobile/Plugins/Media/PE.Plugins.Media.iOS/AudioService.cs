namespace PE.Plugins.Media.iOS
{
    public class AudioService : IAudioService
    {
        #region Constructors

        public AudioService(MediaConfig config)
        {
            //Recorder = new MediaRecorder(config.RecordingFormat);
        }

        #endregion Constructors

        #region Properties

        public IMediaRecorder Recorder { get; private set; }

        public IMediaPlayer Player { get; } = new MediaPlayer();

        #endregion Properties
    }
}