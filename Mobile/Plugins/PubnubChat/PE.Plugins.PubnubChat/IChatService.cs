using PE.Plugins.PubnubChat.Models;

using System;

namespace PE.Plugins.PubnubChat
{
    public delegate void InitializationChangedEventHandler(bool isInitalized);

    public interface IChatService
    {
        #region Events

        event InitializationChangedEventHandler InitializedChanged;
        event EventHandler ConnectedChanged;
        event EventHandler<MessageEventArgs> MessageReceived;
        event EventHandler<PresenceEventArgs> ChannelJoined;
        event EventHandler<PresenceEventArgs> ChannelLeft;
        event EventHandler<PresenceEventArgs> ChannelTimeout;
        event EventHandler<PresenceEventArgs> ChannelState;
        event EventHandler<MessageEventArgs> PublishComplete;

        #endregion Events

        #region Properties

        bool Connected { get; }

        bool Initialized { get; }

        #endregion Properties

        #region Operations

        void Initialize(string userId, string authKey = null, bool reset = true, string push = "");

        void Uninitialize();

        void Publish(string to, string message, object state);

        void GetHistory(long timeStamp);

        #endregion Operations
    }
}
