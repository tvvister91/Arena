using System;
using System.Threading.Tasks;

namespace PE.Plugins.OktaAuth
{
    public interface IOktaAuthService
    {
        Task<AuthResponse> LoginAsync(string username, string password);
    }
}
