using Android.Content;

namespace PE.Plugins.LocalStorage.Droid
{
    public class LocalStorageService : LocalStorageServiceBase, ILocalStorageService
    {
        #region Fields

        private readonly LocalStorageConfiguration _Configuration;

        private Java.IO.File _Root;

        #endregion Fields

        #region Constructors

        public LocalStorageService(LocalStorageConfiguration configuration)
        {
            _Configuration = configuration;
        }

        #endregion Constructors

        public override string GetPath(string file)
        {
            if (_Root == null)
            {
                if (_Configuration.ApplicationContext == null) _Configuration.ApplicationContext = Android.App.Application.Context;
                _Root = _Configuration.ApplicationContext.GetDir("data", FileCreationMode.Private);
            }
            return string.Format("{0}/{1}", _Root, file);
        }
    }
}