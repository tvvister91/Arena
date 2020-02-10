using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using MvvmCross;
using PE.Framework.AppVersion;
using PE.Framework.UWP.AppVersion;

namespace PE.Plugins.Analytics.AppCenterAnalytics.WindowsCommon
{
    public class WinAnalyticsService : AnalyticsService, IAnalyticsService
    {
        #region Constructors

        public WinAnalyticsService(AppCenterConfiguration configuration)
            : base(configuration)
        {
            Mvx.LazyConstructAndRegisterSingleton<IVersion>(() => new AppVersionImpl());
            _version = Mvx.Resolve<IVersion>();
        }

        #endregion Constructors

        #region Operations

        public override void StartAnalytics()
        {
            AppCenter.Start(_Configuration.AppSecret, typeof(Crashes), typeof(Microsoft.AppCenter.Analytics.Analytics));
        }

        #endregion Operations
    }
}
