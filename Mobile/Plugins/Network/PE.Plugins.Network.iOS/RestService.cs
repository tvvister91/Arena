using PE.Plugins.Network.Contracts;

namespace PE.Plugins.Network.iOS
{
    public class RestService : RestServiceBase, IRestService
    {
        public RestService(NetworkConfiguration configuration)
            : base(configuration)
        {

        }
    }
}
