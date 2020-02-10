using Arena.Core.Models;
using PE.Framework.Models;
using PE.Plugins.LocalStorage;
using System;

namespace Arena.Core.Services
{
    public class UserService : IUserService
    {
        #region Constants

        private const string CURRENT_USER_KEY = "CURRENT_USER_KEY";

        #endregion

        #region Private

        private readonly ILocalStorageService _LocalStorageService;

        #endregion

        #region Properties

        public User User { get; private set; }

        public event EventHandler UserChanged;

        #endregion

        #region C-tors

        public UserService(ILocalStorageService localStorageService)
        {
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.ctor - Creating!");

            _LocalStorageService = localStorageService;
        }

        ~UserService()
        {
        }

        #endregion

        #region Operations

        public ServiceResult<User> InitLastUser()
        {
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(InitLastUser)} - Try to get the last user...");
            try
            {
                User = _LocalStorageService.Get<User>(CURRENT_USER_KEY, true);
                UserChanged?.Invoke(this, new EventArgs());
                return new ServiceResult<User> { Status = ServiceResultStatus.Success, Payload = User };
            }
            catch (Exception ex)
            {
                return new ServiceResult<User> { Status = ServiceResultStatus.Error };
            }
        }

        #endregion Operations
    }
}
