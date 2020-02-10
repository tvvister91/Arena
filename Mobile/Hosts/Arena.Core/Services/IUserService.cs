using Arena.Core.Models;
using PE.Framework.Models;
using System;

namespace Arena.Core.Services
{
    public interface IUserService
    {
        #region Events

        event EventHandler UserChanged;

        #endregion Events

        #region Properties

        User User { get; }

        #endregion Properties

        #region Operations

        ServiceResult<User> InitLastUser();

        #endregion Operations
    }
}
