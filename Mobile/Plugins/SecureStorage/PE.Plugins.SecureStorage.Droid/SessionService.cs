using Xamarin.Auth;

namespace PE.Plugins.SecureStorage.Droid
{
    public class SessionService : SessionServiceBase, ISessionService
    {
        #region Constants

        private const string KEY = "F46A3C53-EA8A-4B23-8F27-E51A1C7B2A09";

        #endregion Constants

        #region Constructors

        public SessionService(SecureStorageConfiguration configuration)
            : base(configuration)
        {
        }

        #endregion Constructors

        #region Overrides

        protected override AccountStore GetStore()
        {
            var config = (SecureStorageConfigurationDroid)_Configuration;
            return AccountStore.Create(config.ApplicationContext, KEY);
        }

        #endregion Overrides
    }
}
