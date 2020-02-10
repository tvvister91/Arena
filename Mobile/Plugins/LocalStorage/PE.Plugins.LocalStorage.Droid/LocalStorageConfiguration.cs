using Android.Content;
using MvvmCross.Plugin;

namespace PE.Plugins.LocalStorage.Droid
{
    public class LocalStorageConfiguration : IMvxPluginConfiguration
    {
        #region Properties

        public Context ApplicationContext { get; set; }

        #endregion Properties
    }
}
