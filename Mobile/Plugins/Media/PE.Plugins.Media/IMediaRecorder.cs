using System;

namespace PE.Plugins.Media
{
    public interface IMediaRecorder
    {
        #region Events

        event EventHandler RecorderStatus;

        #endregion Events

        #region Properties

        bool RecorderReady { get; set; }

        #endregion Properties

        #region Operations

        void RecordStart(string fileName);

        void RecordStop();

        #endregion Operations
    }
}
