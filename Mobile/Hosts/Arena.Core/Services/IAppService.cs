using System;

namespace Arena.Core.Services
{
    public interface IAppService
    {
        #region Events

        event EventHandler ConnectivityChanged;
        event EventHandler DidEnterForeground;
        event EventHandler OnUserInteractionStopped;
        event EventHandler OnUserInteractionAcquired;

        #endregion Events

        #region Properties

        bool Initialized { get; }

        bool Connected { get; }

        #endregion Properties

        #region Operations

        #endregion Operations
    }
}
