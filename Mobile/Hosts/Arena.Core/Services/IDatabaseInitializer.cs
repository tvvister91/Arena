using System.Threading.Tasks;

namespace Arena.Core.Services
{
    public interface IDatabaseInitializer
    {
        #region Properties

        bool Intialized { get; }

        #endregion Properties

        #region Methods

        Task InitializeAsync(string userId);

        #endregion Methods
    }
}
