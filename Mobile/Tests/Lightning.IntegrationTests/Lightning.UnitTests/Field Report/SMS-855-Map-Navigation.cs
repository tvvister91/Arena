using System;
using System.Threading.Tasks;
using Lightning.Core.Presentation;
using Lightning.Core.Services;
using Lightning.Core.ViewModels.Reports;
using Lightning.IntegrationTests;
using MvvmCross;
using MvvmCross.Navigation;
using NUnit.Framework;

namespace Lightning.UnitTests.FieldReport
{
    [TestFixture]
    public class SMS_855_Map_Navigation : IntegrationTestBase
    {
        [Test]
        public async Task UserCanNavigateBackToMapFromFieldReport()
        {
            try
            {
                // Given I am in the Field Report page
                var mandatorySelectAddPageViewModel = Mvx.IoCProvider.IoCConstruct<MandatorySelectPageViewModel>();
                await mandatorySelectAddPageViewModel.Initialize();
                await mandatorySelectAddPageViewModel.ViewAppearedAsync();

                // When I hit the map button
                await mandatorySelectAddPageViewModel.MapToolbarItemClicked();

                // Then I am taken back to the map page
                var navigation = (Mvx.IoCProvider.Resolve<IMvxNavigationService>() as NavigationServiceStub);
                Assert.AreEqual(navigation.LastPresentationHint.GetType(), typeof(GoBackPresentationHint));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
