using AudioToolbox;

using AVFoundation;
using CoreFoundation;
using CoreMedia;
using Foundation;
using MvvmCross;
using PE.Plugins.LocalStorage;
using System;
using System.IO;

namespace PE.Plugins.Media.iOS
{
    public class MediaRecorder : NSObject, IMediaRecorder, IDisposable
    {
        #region Events

        public event EventHandler RecorderStatus;

        #endregion Events

        #region Fields

        private readonly ILocalStorageService _Storage;

        private AVAudioRecorder _Recorder;
        private NSError _Error;
        private NSUrl _OutputUrl;
        private NSDictionary _Settings;

        private bool _Disposed = false;

        #endregion Fields

        #region Constructors

        public MediaRecorder(MediaFormat format)
        {
            _Storage = Mvx.IoCProvider.Resolve<ILocalStorageService>();
            Init(format);
        }

        ~MediaRecorder()
        {
            Dispose();
        }

        #endregion Constructors

        #region Properties

        public bool RecorderReady { get; set; } = false;

        #endregion Properties

        #region Initializations

        private void Init(MediaFormat format)
        {
            var audioSession = AVAudioSession.SharedInstance();
            var err = audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
            if (err != null)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** AudioService.Init - Exception initializing session: {0}", err.Description));
                throw new System.Exception(err.DebugDescription);
            }
            err = audioSession.SetActive(true);
            if (err != null)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** AudioService.Init - Exception activating session: {0}", err.Description));
                throw new System.Exception(err.DebugDescription);
            }

            //  TODO: Audio Formats - Note that Apple does not support MP3 recording, only playback. 
            var formatType = (format == MediaFormat.WAV) ? AudioFormatType.LinearPCM : AudioFormatType.MPEGLayer3;
            //  values to define the type of audio we want to record
            NSObject[] values = new NSObject[]
            {
                NSNumber.FromFloat(44100.0f),                                       //  Sample Rate
                NSNumber.FromInt32((int)formatType),                                //  Format
                NSNumber.FromInt32(2),                                              //  Channels
                NSNumber.FromInt32(16),                                             //  Bit Depth
                NSNumber.FromBoolean(false),                                        //  Big EndianKey?
                NSNumber.FromBoolean(false)                                         //  Float Key?
            };

            //  keys to correspond with the values above
            NSObject[] keys = new NSObject[]
            {
                AVAudioSettings.AVSampleRateKey,
                AVAudioSettings.AVFormatIDKey,
                AVAudioSettings.AVNumberOfChannelsKey,
                AVAudioSettings.AVLinearPCMBitDepthKey,
                AVAudioSettings.AVLinearPCMIsBigEndianKey,
                AVAudioSettings.AVLinearPCMIsFloatKey
            };
            //  create settings
            _Settings = NSDictionary.FromObjectsAndKeys(values, keys);

            RecorderReady = true;
        }

        #endregion Initialization

        #region Recording

        public void RecordStart(string fileName)
        {
            var filePath = _Storage.GetPath(fileName);

            System.Diagnostics.Debug.WriteLine(string.Format("*** AudioService.RecordStart - Preparing to record to: {0}.", filePath));

            _OutputUrl = NSUrl.FromFilename(filePath);

            //  apply recorder settings
            _Recorder = AVAudioRecorder.Create(_OutputUrl, new AudioSettings(_Settings), out _Error);

            //  record
            if (!_Recorder.PrepareToRecord())
            {
                _Recorder.Dispose();
                _Recorder = null;
                throw new Exception("Could not prepare recording");
            }

            _Recorder.FinishedRecording += delegate (object sender, AVStatusEventArgs e)
            {
                _Recorder.Dispose();
                _Recorder = null;
                Console.WriteLine("Done Recording (status: {0})", e.Status);
            };

            _Recorder.Record();

            System.Diagnostics.Debug.WriteLine("*** AudioService.RecordStart - Recording started");
        }

        public void RecordStop()
        {
            System.Diagnostics.Debug.WriteLine("*** AudioService.RecordStop - Preparing to end recording");
            if (!_Recorder.Recording) return;
            System.Diagnostics.Debug.WriteLine("*** AudioService.RecordStop - Stopping");
            _Recorder.Stop();
        }

        #endregion Recording

        #region Cleanup

        public void Dispose()
        {
            if (_Disposed) return;
            if (_Recorder != null)
            {
                _Recorder.Dispose();
                _Recorder = null;
            }
            _Disposed = true;
        }

        #endregion Cleanup
    }
}