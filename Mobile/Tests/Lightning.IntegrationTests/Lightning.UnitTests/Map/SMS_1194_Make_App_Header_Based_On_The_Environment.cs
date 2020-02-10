using System;
using System.Threading.Tasks;
using Lightning.Core.Services;
using Lightning.Core.ViewModels;
using Lightning.IntegrationTests;
using MvvmCross;
using NUnit.Framework;

namespace Lightning.UnitTests.Map
{
    [TestFixture]
    public class SMS_1194_Make_App_Header_Based_On_The_Environment : IntegrationTestBase
    {
        [Test]
        public async Task ShowHeaderInDevBuild()
        {
            // Given I am in a Dev Build
            var buildVersion = Mvx.IoCProvider.Resolve<IBuildVersionProvider>() as BuildVersionProviderStub;
            buildVersion.Version = Core.Enums.BuildVersionEnum.Dev;

            // When I visit the map page
            var mapViewModel = Mvx.IoCProvider.IoCConstruct<MapViewModel>();
            await mapViewModel.Initialize();
            await mapViewModel.ViewAppearedAsync();

            // Then I do see the header
            Assert.IsTrue(mapViewModel.VersionHeaderVisible);
        }

        [Test]
        public async Task ShowHeaderInUATBuild()
        {
            // Given I am in a uat Build
            var buildVersion = Mvx.IoCProvider.Resolve<IBuildVersionProvider>() as BuildVersionProviderStub;
            buildVersion.Version = Core.Enums.BuildVersionEnum.UAT;

            // When I visit the map page
            var mapViewModel = Mvx.IoCProvider.IoCConstruct<MapViewModel>();
            await mapViewModel.Initialize();
            await mapViewModel.ViewAppearedAsync();

            // Then I do see the header
            Assert.IsTrue(mapViewModel.VersionHeaderVisible);
        }

        [Test]
        public async Task ShowHeaderInQABuild()
        {
            // Given I am in a qa Build
            var buildVersion = Mvx.IoCProvider.Resolve<IBuildVersionProvider>() as BuildVersionProviderStub;
            buildVersion.Version = Core.Enums.BuildVersionEnum.QA;

            // When I visit the map page
            var mapViewModel = Mvx.IoCProvider.IoCConstruct<MapViewModel>();
            await mapViewModel.Initialize();
            await mapViewModel.ViewAppearedAsync();

            // Then I do see the header
            Assert.IsTrue(mapViewModel.VersionHeaderVisible);
        }

        [Test]
        public async Task DontShowHeaderInProdBuild()
        {
            // Given I am in a prod Build
            var buildVersion = Mvx.IoCProvider.Resolve<IBuildVersionProvider>() as BuildVersionProviderStub;
            buildVersion.Version = Core.Enums.BuildVersionEnum.Release;

            // When I visit the map page
            var mapViewModel = Mvx.IoCProvider.IoCConstruct<MapViewModel>();
            await mapViewModel.Initialize();
            await mapViewModel.ViewAppearedAsync();

            // Then I dont see the header
            Assert.IsFalse(mapViewModel.VersionHeaderVisible);
        }

        [Test]
        public async Task DontShowHeaderInDemoBuild()
        {
            // Given I am in a demo Build
            var buildVersion = Mvx.IoCProvider.Resolve<IBuildVersionProvider>() as BuildVersionProviderStub;
            buildVersion.Version = Core.Enums.BuildVersionEnum.Demo;

            // When I visit the map page
            var mapViewModel = Mvx.IoCProvider.IoCConstruct<MapViewModel>();
            await mapViewModel.Initialize();
            await mapViewModel.ViewAppearedAsync();

            // Then I dont see the header
            Assert.IsFalse(mapViewModel.VersionHeaderVisible);
        }
    }
}
