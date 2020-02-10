using System;
using System.Threading.Tasks;
using Lightning.Core.Events;
using Lightning.Core.Services;
using PE.Shared.Models;

namespace Lightning.UnitTests.Stub
{
    public class AuthenticationServiceStub : IAuthenticationService
    {
        public event EventHandler<AuthenticationEventArgs> AuthenticationChanged;

        public Task<ServiceResult> AuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult> EndSessionAsync()
        {
            throw new NotImplementedException();
        }

        public Task FullyRegisterForPushNotificationsAsync(DeviceInfo deviceInfo)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult> RegisterForPushNotificationsAsync(DeviceInfo deviceInfo)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult> SilentAuthenticateAsync(bool hasRegainedConnectivity = false)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult> OfflineAuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        public Task WaitUntilAuthenticated()
        {
            throw new NotImplementedException();
        }

        public void RaiseAuthenticationChanged(string userAlias, bool authenticated)
        {
            AuthenticationChanged?.Invoke(this, new AuthenticationEventArgs(userAlias, authenticated, true));
        }

        public Task<bool> IsAuthenticated()
        {
            return Task.FromResult(true);
        }

        public DeviceInfo DeviceInfo { get; set; }
    }
}
