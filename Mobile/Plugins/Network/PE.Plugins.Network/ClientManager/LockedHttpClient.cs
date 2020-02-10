using System.Net.Http;
using MvvmCross;

namespace PE.Plugins.Network.ClientManager
{
    [Preserve(AllMembers = true)]
    public class LockedHttpClient : HttpClient
    {
        static int _Count = 0;

        public LockedHttpClient()
        {
            Id = _Count++;
        }

        public LockedHttpClient(HttpMessageHandler handler)
            : base(handler)
        {
            Id = _Count++;
        }

        public LockedHttpClient(HttpMessageHandler handler, bool disposeHandler)
            : base(handler, disposeHandler)
        {
            Id = _Count++;
        }

        public bool Locked = false;

        public int UseCount = 0;

        public int Id = 0;

        public bool Refresh = false;

        public bool HasToken = false;
    }
}
