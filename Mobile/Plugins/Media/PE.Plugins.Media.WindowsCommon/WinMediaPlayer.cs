using System;
using System.Threading.Tasks;
using System.Timers;

using MvvmCross;
using PE.Plugins.LocalStorage;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

namespace PE.Plugins.Media.WindowsCommon
{
    public class WinMediaPlayer : IMediaPlayer, IDisposable
    {
        #region Events

        public event EventHandler<PlaybackEventArgs> PlaybackStatus;
        public event EventHandler PlayerStatus;

        #endregion Events

        #region Fields

        private readonly ILocalStorageService _Storage;
        private string _FileName;
        private MediaPlayer _Player;
        /// <summary>
        /// The best way is to move that timer in common code, but for now it's enough, bcs it took less time
        /// </summary>
        private Timer _playbackTimer;
        private bool _Disposed = false;

        #endregion Fields

        #region Constructors

        public WinMediaPlayer()
        {
            _Storage = Mvx.IoCProvider.Resolve<ILocalStorageService>();
            _playbackTimer = new Timer(1000);
            _playbackTimer.Elapsed += OnPlaybackTimerElapsed;
            Init();
        }

        ~WinMediaPlayer()
        {
            Dispose();
        }

        #endregion Constructors

        #region Properties

        public bool PlayerReady { get; set; } = false;

        public bool RecorderReady { get; set; } = false;

        #endregion Properties

        #region Initialization

        private void Init()
        {
            Task.Run(() => InitAsync());
        }

        private void InitAsync()
        {
            try
            {
                if (_Player != null)
                {
                    _Player.CurrentStateChanged -= PlaybackStateChanged;
                    _Player.MediaEnded -= PlaybackMediaEnded;
                    _Player.MediaFailed -= PlaybackMediaFailed;
                    _Player.MediaOpened -= PlaybackMediaOpened;
                    _Player.Dispose();
                    _Player = null;
                }

                _Player = new MediaPlayer {
                    AudioCategory = MediaPlayerAudioCategory.Media,
                    AutoPlay = false
                };
                _Player.CurrentStateChanged += PlaybackStateChanged;
                _Player.MediaEnded += PlaybackMediaEnded;
                _Player.MediaFailed += PlaybackMediaFailed;
                _Player.MediaOpened += PlaybackMediaOpened;

                PlayerStatus?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Initialization

        #region Playback

        public void PlayPrepare(string fileName)
        {
            PlayerReady = false;
            _FileName = fileName;
            Task.Run(async () => await PlayPrepareAsync(fileName));
        }

        private async Task PlayPrepareAsync(string fileName)
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            _Player.Source = MediaSource.CreateFromStorageFile(file);
        }

        public void PlayStart()
        {
            if (!PlayerReady) return;// throw new Exception("Player not ready.");
            _playbackTimer.Start();
            _Player.Play();
        }

        public void PlayStop()
        {
            _playbackTimer.Stop();
            _Player.Pause();
        }

        private void PlaybackStateChanged(MediaPlayer sender, object args)
        {
            if (!PlayerReady) return;

            PlaybackStatus.Invoke(this, new PlaybackEventArgs {
                Stopped = sender.PlaybackSession.PlaybackState != MediaPlaybackState.Playing && sender.PlaybackSession.PlaybackState != MediaPlaybackState.Paused,
                CurrentTime = sender.PlaybackSession.Position.TotalSeconds,
                Duration = sender.PlaybackSession.NaturalDuration.TotalSeconds
            });
        }

        private void PlaybackMediaEnded(MediaPlayer sender, object args)
        {
            _playbackTimer.Stop();
            PlaybackStatus?.Invoke(this, new PlaybackEventArgs {
                Stopped = true,
                CurrentTime = sender.PlaybackSession.NaturalDuration.TotalSeconds,
                Duration = sender.PlaybackSession.NaturalDuration.TotalSeconds
            });
        }

        private void PlaybackMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            _playbackTimer.Stop();
            PlayerReady = false;
            PlaybackStatus?.Invoke(this, new PlaybackEventArgs {
                Stopped = true,
                Duration = 0,
                CurrentTime = 0,
                HasError = true
            });
        }

        private void PlaybackMediaOpened(MediaPlayer sender, object args)
        {
            PlayerReady = true;
            PlaybackStatus?.Invoke(this, new PlaybackEventArgs {
                Stopped = true,
                CurrentTime = 0,
                Duration = sender.PlaybackSession.NaturalDuration.TotalSeconds,
                IsNew = true
            });
            PlayerStatus?.Invoke(this, new EventArgs());
        }

        private void OnPlaybackTimerElapsed(object sender, ElapsedEventArgs e)
        {
            PlaybackStateChanged(_Player, null);
        }

        #endregion Playback

        #region Cleanup

        public void Dispose()
        {
            if (_Disposed) return;
            if (_playbackTimer != null)
            {
                _playbackTimer.Elapsed -= OnPlaybackTimerElapsed;
                _playbackTimer.Dispose();
                _playbackTimer = null;
            }
            if (_Player != null)
            {
                _Player.CurrentStateChanged -= PlaybackStateChanged;
                _Player.MediaEnded -= PlaybackMediaEnded;
                _Player.MediaFailed -= PlaybackMediaFailed;
                _Player.MediaOpened -= PlaybackMediaOpened;
                _Player.Dispose();
                _Player = null;
                PlayerReady = false;
            }
            _Disposed = true;
        }

        #endregion Cleanup
    }
}
