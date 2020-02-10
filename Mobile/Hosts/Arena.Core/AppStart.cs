using System;
using System.Threading.Tasks;

using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PE.Plugins.Analytics;
using PE.Plugins.Network.Contracts;

using Arena.Core.Services;
using Arena.Core.ViewModels;
using MvvmCross;

namespace Arena.Core
{
    public class AppStart : MvxAppStart
    {
        #region Fields

        private readonly INetworkService _NetworkService;
        private readonly IAppService _AppService;
        private readonly IMvxNavigationService _NavigationService;

        private bool _Ready = false;

        #endregion Fields

        #region Constructors

        public AppStart(IMvxApplication application, IAppService appService, INetworkService networkService, IAnalyticsService analyticsService, IMvxNavigationService navigationService)
            : base(application, navigationService)
        {
            //  start analytics
            analyticsService.StartAnalytics();

            _NetworkService = networkService;
            _AppService = appService;
            _NavigationService = navigationService;
        }

        #endregion Constructors

        #region Startup

        protected override async Task NavigateToFirstViewModel(object hint = null)
        {
            try
            {
                await _NavigationService.Navigate<MainViewModel>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(NavigateToFirstViewModel)} - Exception: {ex}");
                await _NavigationService.Navigate<MainViewModel>();
            }
        }

        #endregion Startup
    }
}
