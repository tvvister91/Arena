using System.Threading.Tasks;
using Lightning.Core.Resources;
using Lightning.Core.Services;
using Lightning.Core.ViewModels;
using Lightning.IntegrationTests.Stub;
using MvvmCross;
using NUnit.Framework;
using PE.Plugins.Dialogs;

namespace Lightning.UnitTests.Map
{
    public class MapViewModelTests : IntegrationTestBase
    {
        // SMS-42 Acceptance criteria 1
        [Test]
        public async Task UserWillSeeCorrectPermissionPopup()
        {
            // Given that Location Permission is not enabled
            var permissionProvider = Mvx.IoCProvider.Resolve<IPermissionService>() as PermissionStub;
            permissionProvider.TestPermissionConfiguration[Plugin.Permissions.Abstractions.Permission.LocationWhenInUse] =
                Plugin.Permissions.Abstractions.PermissionStatus.Unknown;

            // When I fist visit the map page
            var mapViewModel = Mvx.IoCProvider.IoCConstruct<MapViewModel>();
            await mapViewModel.ViewAppearedAsync();

            await Task.Delay(5000); // Give time for the dialog to show up.

            // Then i see a dialog explaining that I should enable permissions
            var dialogStub = Mvx.IoCProvider.Resolve<IDialogService>() as DialogStub;
            Assert.AreEqual(AppResources.LocationPermissionMessage, dialogStub.LastMessageShown);
            Assert.AreEqual(AppResources.LocationPermissionHeader, dialogStub.LastTitleShown);
        }
    }
}
