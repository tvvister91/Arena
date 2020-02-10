using MvvmCross;

using PE.Plugins.LocalStorage;

using System;
using System.Threading.Tasks;

using Windows.ApplicationModel.Core;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PE.Plugins.Media.WindowsCommon
{
    public class WinMediaRecorder : IMediaRecorder, IDisposable
    {
        #region Events

        public event EventHandler RecorderStatus;

        #endregion Events

        #region Fields

        private readonly ILocalStorageService _Storage;
        private readonly MediaFormat _Format;

        private MediaCapture _Capture;
        private IRandomAccessStream _Stream;
        private string _FileName;

        private bool _Disposed = false;

        private CoreApplicationView _Window;

        #endregion Fields

        #region Constructors

        public WinMediaRecorder(MediaFormat format)
        {
            _Format = format;
            _Storage = Mvx.IoCProvider.Resolve<ILocalStorageService>();
            Init();
        }

        ~WinMediaRecorder()
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
            Task.Run(async () => await InitAsync());
        }

        private async Task InitAsync()
        {
            try
            {
                //  initialize media capture
                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings { StreamingCaptureMode = StreamingCaptureMode.Audio };
                _Capture = new MediaCapture();
                await _Capture.InitializeAsync(settings);

                //  this only happens if you try to record for more than 3 hours
                //  these event handlers are safe - Service is a singleton and will only ever have 1 instance for the life of the app
                _Capture.RecordLimitationExceeded += async (MediaCapture sender) =>
                {
                    await _Capture.StopRecordAsync();
                };
                _Capture.Failed += (MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs) =>
                {
                    RecorderReady = true;
                    //  TODO: handle record failure
                    RecorderStatus?.Invoke(this, new EventArgs());
                };
                RecorderReady = true;
                RecorderStatus?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** AudioService.InitRecordAsync - Exception: {0}", ex));
            }
        }


        #endregion Initialization

        #region Recording

        public void RecordStart(string fileName)
        {
            _FileName = fileName;
            var filePath = _Storage.GetPath(fileName);
            Task.Run(async () => await RecordStartAsync(filePath, _Format));
        }

        private async Task RecordStartAsync(string filePath, MediaFormat format)
        {
            if (_Stream != null) _Stream.Dispose();
            _Stream = new InMemoryRandomAccessStream();
            //  TODO: Audio Formats - Note that Apple does not support MP3 recording, only playback. 
            if (format == MediaFormat.WAV)
            {
                await _Capture.StartRecordToStreamAsync(MediaEncodingProfile.CreateWav(AudioEncodingQuality.High), _Stream);
            }
            else if (format == MediaFormat.MP3)
            {
                await _Capture.StartRecordToStreamAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High), _Stream);
            }
        }

        public void RecordStop()
        {
            Task.Run(async () => await RecordStopAsync());
        }

        private async Task RecordStopAsync()
        {
            try
            {
                await _Capture.StopRecordAsync();
                //  write the buffer to file
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(_FileName, CreationCollisionOption.ReplaceExisting);

                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await RandomAccessStream.CopyAndCloseAsync(_Stream.GetInputStreamAt(0), stream.GetOutputStreamAt(0));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Recording

        #region Cleanup

        public void Dispose()
        {
            if (_Disposed) return;
            if (_Capture != null)
            {
                _Capture.Dispose();
                _Capture = null;
                RecorderReady = false;
            }
            _Disposed = true;
        }

        #endregion Cleanup
    }
}
