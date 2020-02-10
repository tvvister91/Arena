using Arena.Core.Models;
using Arena.Core.Repositories;
using PE.Plugins.LocalStorage;
using System;
using System.Threading.Tasks;

namespace Arena.Core.Services
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        #region Private

        private readonly IRepository<User> _UserRepository;
        private readonly ILocalStorageService _StorageService;

        #endregion Private

        #region C-tors

        public DatabaseInitializer(IRepository<User> userRepository, ILocalStorageService storageService)
        {
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.ctor - Creating!");

            //  DO NOT DO ANY INITIALIZATION HERE - THIS CAN ONLY BE DONE AFTER AUTHENTICATION
            _UserRepository = userRepository;
            _StorageService = storageService;
        }

        #endregion C-tors

        #region Properties

        public bool Intialized { get; private set; } = false;

        #endregion Properties

        #region Init

        public async Task InitializeAsync(string userId)
        {
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(InitializeAsync)} - Initializing data service - {userId}.");
            try
            {
                //  create the local store
                var path = _StorageService.GetPath($"{userId}.db3");

                await _UserRepository.Initialize(path).ConfigureAwait(false);

                Intialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {nameof(InitializeAsync)} - Exception: {ex}");
            }
        }

        #endregion Init
    }
}
