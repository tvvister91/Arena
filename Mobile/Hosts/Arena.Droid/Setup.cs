using System;
using Arena.Core.Services;
using Arena.Droid.PlatformSpecific;
using MvvmCross;
using MvvmCross.Forms.Platforms.Android.Core;
using MvvmCross.Forms.Presenters;
using MvvmCross.IoC;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Presenters;
using MvvmCross.Plugin;
using PE.Framework.Droid.AndroidApp.AppVersion;
using PE.Framework.Droid.Helpers;
using PE.Framework.Helpers;

namespace Arena.Droid
{
    public class Setup : MvxFormsAndroidSetup<Core.CoreApp, UI.FormsApp>
    {
        protected override IMvxPluginConfiguration GetPluginConfiguration(Type plugin)
        {
            //  find the config method
            string name = plugin.FullName.Split(new char[] { '.' })[2];
            System.Diagnostics.Debug.WriteLine(string.Format("*** Setup.GetPluginConfiguration - Configuring plugin {0}", name));
            name = string.Format("Arena.Droid.Bootstrap.{0}Bootstrap", name);
            //  get this type
            var type = GetType().Assembly.GetType(name);
            if (type == null)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** Setup.GetPluginConfiguration - Setup: Could not find type {0}.", name));
                return base.GetPluginConfiguration(plugin);
            }
            //  find the configuration method
            var method = type.GetMethod("Configure", new Type[] { });
            if (method == null)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("*** Setup.GetPluginConfiguration - Setup: Could not find configuration method for type {0}.", name));
                return base.GetPluginConfiguration(plugin);
            }
            //  invoke the configuration method
            return (IMvxPluginConfiguration)method.Invoke(null, new object[] { });
        }

        protected override IMvxAndroidViewPresenter CreateViewPresenter()
        {
            var presenter = new CustomPresenter(GetViewAssemblies(), FormsApplication);
            Mvx.IoCProvider.RegisterSingleton<IMvxFormsViewPresenter>(presenter);
            presenter.FormsPagePresenter = CreateFormsPagePresenter(presenter);
            return presenter;
        }
    }
}