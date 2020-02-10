using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using MvvmCross;
using MvvmCross.IoC;
using PE.Framework.AppVersion;
using PE.Framework.Droid.AppVersion;

namespace PE.Plugins.Analytics.AppCenterAnalytics.Droid
{
    public class DroidAnalyticsService : AnalyticsService, IAnalyticsService
    {
        #region Constructors

        public DroidAnalyticsService(AppCenterConfiguration configuration)
            : base(configuration)
        {
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IVersion, AppVersionImpl>();
			_version = Mvx.IoCProvider.Resolve<IVersion>();
        }

        #endregion Constructors

        #region Properties

        protected override string AppVersion => _version.Version;

        #endregion Properties

        #region Operations

        public override void StartAnalytics()
        {
            AppCenter.Start(_Configuration.AppSecret, typeof(Crashes), typeof(Microsoft.AppCenter.Analytics.Analytics));
        }

        #endregion Operations
    }
}