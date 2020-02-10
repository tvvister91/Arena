using MvvmCross;
using PE.Plugins.Network.Contracts;

namespace PE.Plugins.Network.WindowsCommon
{
    [Preserve(AllMembers = true)]
    public class RestService : RestServiceBase, IRestService
    {
        public RestService(NetworkConfiguration configuration)
            : base(configuration)
        {

        }
    }
}
