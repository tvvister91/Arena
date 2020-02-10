using AVFoundation;

using CoreFoundation;
using CoreMedia;

using Foundation;

using MvvmCross;

using PE.Plugins.LocalStorage;

using System;

namespace PE.Plugins.Media.iOS
{
    public class MediaPlayer : NSObject, IMediaPlayer, IDisposable
    {
        #region Const

        private const string _status = "status";
        private const string _error = "error";

        #endregion

        #region Events

        public event EventHandler<PlaybackEventArgs> PlaybackStatus;
        public event EventHandler PlayerStatus;

        #endregion Events

        #region Fields

        private readonly ILocalStorageService _Storage;

        private bool _isRestart;
        private NSError _Error;
        private AVPlayer _Player;
        private AVPlayerItem _PlayerItem;
        private AVAsset _Asset;

        private bool _Disposed = false;

        private NSObject _TimeChanged;

        #endregion Fields

        #region Constructors

        public MediaPlayer()
        {
            _Storage = Mvx.IoCProvider.Resolve<ILocalStorageService>();
        }

        ~MediaPlayer()
        {
            Dispose();
        }

        #endregion Constructors

        #region Properties

        public bool PlayerReady { get; set; }

        #endregion Properties

        #region Playback

        public void PlayPrepare(string fileName)
        {
            try
            {
                var filePath = _Storage.GetPath(fileName);
                var url = NSUrl.FromFilename(filePath);

                if (_Player == null)
                {
                    _Player = new AVPlayer();
                }

                if (_PlayerItem != null)
                {
                    _Player.RemoveTimeObserver(_TimeChanged); // for skipping 1-2 extra triggering for new item creation code below
                    CleanPlayerItem();
                }

                PlayerReady = false;
                _Asset = AVAsset.FromUrl(url);
                _PlayerItem = new AVPlayerItem(_Asset);
                _PlayerItem.AddObserver(this, _status, NSKeyValueObservingOptions.Initial | NSKeyValueObservingOptions.New, _Player.Handle);
                _PlayerItem.AddObserver(this, _error, NSKeyValueObservingOptions.Initial | NSKeyValueObservingOptions.New, _Player.Handle);
                _Player.ReplaceCurrentItemWithPlayerItem(_PlayerItem);
                _Player.Rate = 1;
                _Player.Pause(); // HACK: stop autoplaying after recording

                _TimeChanged = _Player.AddPeriodicTimeObserver(new CMTime(1, 1), DispatchQueue.MainQueue, time => {
                    // skip restart event
                    if (_isRestart)
                    {
                        _isRestart = false;

                        return;
                    }

                    if (_PlayerItem != null && _PlayerItem.Duration != CMTime.Indefinite)
                    {
                        var stopped = time == _PlayerItem.Duration;
                        PlaybackStatus?.Invoke(this, new PlaybackEventArgs {
                            CurrentTime = time.Seconds,
                            Duration = _PlayerItem.Duration.Seconds,
                            Stopped = stopped
                        });

                        if (stopped)
                        {
                            _isRestart = true;
                            _Player.Seek(CMTime.Zero);
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** AudioService.PlayPrepare - Exception: {0}", ex));
            }
        }

        public void PlayStart()
        {
            try
            {
                _Player.Volume = 1;
                _Player.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** AudioService.PlayStart - Exception: {0}", ex));
            }
        }

        public void PlayStop()
        {
            try
            {
                //  TODO: split out to pause and stop. On Stop reset current time
                System.Diagnostics.Debug.WriteLine("*** AudioService.PlayStop - Attempting to end playback.");
                _Player.Pause();
                System.Diagnostics.Debug.WriteLine("*** AudioService.PlayStop - Playback paused.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** AudioService.PlayStop - Exception: {0}", ex));
            }
        }

        private void CleanPlayerItem()
        {
            _PlayerItem.RemoveObserver(this, _status);
            _PlayerItem.RemoveObserver(this, _error);
            _Asset.Dispose();
            _PlayerItem.Dispose();
        }

        #endregion Playback

        #region Observer

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            var path = keyPath.ToString();
            System.Diagnostics.Debug.WriteLine(string.Format("*** AudioService.ObserveValue - Value change: {0} = {1}", keyPath, change.ToString()));
            if (path.Equals(_status))
            {
                if (_Player.Status == AVPlayerStatus.ReadyToPlay)
                {
                    if (!PlayerReady)
                    {
                        PlayerReady = _PlayerItem.Duration != CMTime.Indefinite;

                        if (PlayerReady)
                        {
                            PlaybackStatus?.Invoke(this, new PlaybackEventArgs {
                                CurrentTime = 0,
                                Duration = _PlayerItem.Duration.Seconds,
                                Stopped = true,
                                IsNew = true
                            });
                            PlayerStatus?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }
            else if (path.Equals(_error))
            {
                if (_Player.Error != null)
                {
                    PlayerReady = false;
                    PlaybackStatus?.Invoke(this, new PlaybackEventArgs { CurrentTime = 0, Duration = 0, Stopped = true, HasError = true });
                    PlayerStatus?.Invoke(this, new EventArgs());
                }
            }
        }

        #endregion Observer

        #region Cleanup

        public new void Dispose()
        {
            if (_Disposed) return;

            if (_Player != null)
            {
                if (_PlayerItem != null)
                {
                    CleanPlayerItem();
                }

                _Player.RemoveTimeObserver(_TimeChanged);
                _Player.Dispose();
                _Player = null;
            }
            _Disposed = true;
        }

        #endregion Cleanup
    }
}