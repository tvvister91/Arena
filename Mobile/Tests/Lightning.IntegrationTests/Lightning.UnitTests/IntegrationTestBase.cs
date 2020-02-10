using Lightning.Core;
using Lightning.Core.Model;
using Lightning.Core.Repositories;
using Lightning.Core.Services;
using Lightning.IntegrationTests.Network;
using Lightning.IntegrationTests.Stub;
using Lightning.UnitTests.Stub;
using Moq;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.IoC;
using MvvmCross.Navigation;
using MvvmCross.Tests;
using MvvmCross.ViewModels;
using MvvmCross.Views;
using NUnit.Framework;
using PE.Framework.AppVersion;
using PE.Modules.Sedgwick.Clients;
using PE.Modules.Sedgwick.Contracts;
using PE.Modules.Sedgwick.Models.Claims;
using PE.Modules.Sedgwick.Models.Inspection;
using PE.Modules.Sedgwick.Models.Media;
using PE.Modules.Sedgwick.Models.Trackings;
using PE.Modules.Sedgwick.Models.Users;
using PE.Plugins.Analytics;
using PE.Plugins.Dialogs;
using PE.Plugins.LocalStorage;
using PE.Plugins.Media;
using PE.Plugins.Network.Contracts;
using IMediaService = Lightning.Core.Services.IMediaService;

namespace Lightning.UnitTests
{
    public class IntegrationTestBase : MvxIoCSupportingTest
    {
        [SetUp]
        public new void Setup()
        {
            base.Setup();

            // Log in the user
            var restService = Mvx.IoCProvider.Resolve<IRestService>();
            restService.Reset(UserToken.TestToken);
        }

        [TearDown]
        public virtual void TearDown()
        {
            base.Reset();
        }

        protected override void AdditionalSetup()
        {
            base.AdditionalSetup();

            var MockDispatcher = new MockDispatcher();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(MockDispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(MockDispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(MockDispatcher);

            var appServiceMock = new Mock<IAppService>();
            appServiceMock.Setup(a => a.Connected).Returns(true);

            Mvx.IoCProvider.RegisterSingleton(new Mock<IAnalyticsService>().Object);
            Mvx.IoCProvider.RegisterSingleton(appServiceMock.Object);
            Mvx.IoCProvider.RegisterSingleton(new Mock<INetworkService>().Object);
            Mvx.IoCProvider.RegisterSingleton(new Mock<IVersion>().Object);
            Mvx.IoCProvider.RegisterSingleton(new Mock<IMvxApplication>().Object);
            Mvx.IoCProvider.RegisterSingleton(new Mock<IPhoneDialer>().Object);
            Mvx.IoCProvider.RegisterSingleton(new Mock<IEmailSender>().Object);
            Mvx.IoCProvider.RegisterSingleton(new Mock<IInspectionDataDeleterService>().Object);
            Mvx.IoCProvider.RegisterSingleton(new Mock<IMapNavigator>().Object);
            Mvx.IoCProvider.RegisterSingleton(new Mock<ISettingsService>().Object);
            Mvx.IoCProvider.RegisterSingleton(new Mock<ILocalStorageService>().Object);
            Mvx.IoCProvider.RegisterSingleton(new Mock<ICameraService>().Object);
            Mvx.IoCProvider.RegisterSingleton(new Mock<IInspectionDataInitialFetchService>().Object);

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IMvxNavigationService, NavigationServiceStub>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IImageResizerService, PhotoResizerStub>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IDialogService, DialogStub>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IDatabaseInitializer, DatabaseInitializer>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRestService, RestService>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IInspectionService, InspectionServiceStub>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IPhotoCaptureService, PhotoCaptureService>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IUserServiceClient, UserServiceClient>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ISynchronizationService, SynchronizationService>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IClaimsService, ClaimsServiceStub>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IPermissionService, PermissionStub>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IMvxAppStart, AppStart>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IEncryptionKeyProvider, EncryptionKeyProviderStub>();

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IUserServiceClient, UserServiceClient>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IMediaService, MediaService>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IClaimsService, ClaimsService>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IClaimServiceClient, ClaimServiceClient>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IMediaServiceClient, MediaServiceClient>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ITrackingService, TrackingService>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IBuildVersionProvider, BuildVersionProviderStub>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ITrackingServiceClient, TrackingServiceClient>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IInspectionServiceClient, InspectionServiceClient>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IAuthenticationService, AuthenticationServiceStub>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IUserService, UserService>();

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<Claim>, RepositoryStub<Claim>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<User>, RepositoryStub<User>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<AssetCategory>, RepositoryStub<AssetCategory>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<ClaimAssetCategory>, RepositoryStub<ClaimAssetCategory>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<MediaAsset>, RepositoryStub<MediaAsset>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<FormInput>, RepositoryStub<FormInput>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<FormText>, RepositoryStub<FormText>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<FormInputType>, RepositoryStub<FormInputType>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<ClaimFormInput>, RepositoryStub<ClaimFormInput>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<ClaimResult>, RepositoryStub<ClaimResult>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<Form>, RepositoryStub<Form>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<FormOption>, RepositoryStub<FormOption>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<FormOptionType>, RepositoryStub<FormOptionType>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<ClaimInputResult>, RepositoryStub<ClaimInputResult>>();

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<Tracking>, RepositoryStub<Tracking>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<TrackingNote>, RepositoryStub<TrackingNote>>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IRepository<TrackingType>, RepositoryStub<TrackingType>>();
        }
    }
}
