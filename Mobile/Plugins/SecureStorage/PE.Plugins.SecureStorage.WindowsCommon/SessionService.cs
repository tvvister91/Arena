using Xamarin.Auth;

namespace PE.Plugins.SecureStorage.WindowsCommon
{
    public class SessionService : SessionServiceBase, ISessionService
    {
        #region Constructors

        public SessionService(SecureStorageConfiguration configuration)
            : base(configuration)
        {
        }

        #endregion Constructors

        #region Overrides

        protected override AccountStore GetStore()
        {
            return AccountStore.Create();
        }

        #endregion Overrides
    }
}
