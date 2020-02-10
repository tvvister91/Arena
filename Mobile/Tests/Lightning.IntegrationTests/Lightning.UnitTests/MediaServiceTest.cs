using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lightning.Core.Repositories;
using Lightning.Core.Services;
using Lightning.IntegrationTests.Stub;
using Lightning.UnitTests.Stub;
using Moq;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using NUnit.Framework;
using PE.Modules.Sedgwick.Models;
using PE.Modules.Sedgwick.Models.Media;
using PE.Modules.Sedgwick.Models.Users;
using PE.Plugins.Network.Contracts;

namespace Lightning.UnitTests
{
    [TestFixture]
    public class MediaServiceTest : IntegrationTestBase
    {
        [Test]
        public async Task MediaServiceIsInitializedOnAuthenticationChanged()
        {
            var claimAssetCategoryRespository = Mvx.IoCProvider.Resolve<IRepository<ClaimAssetCategory>>();
            await claimAssetCategoryRespository.Initialize("test");
            var mediaAssetRespository = Mvx.IoCProvider.Resolve<IRepository<MediaAsset>>();
            await mediaAssetRespository.Initialize("test");
            // ARRANGE - Set up the authentication service, the media service and the assetcatagories repository.
            var authenticationService = Mvx.IoCProvider.Resolve<IAuthenticationService>() as AuthenticationServiceStub;
            var mediaService = Mvx.IoCProvider.Resolve<IMediaService>();

            var mediaCategoriesRespository = Mvx.IoCProvider.Resolve<IRepository<AssetCategory>>();
            await mediaCategoriesRespository.Initialize("test");

            // Media service contains no asset catagories and neither does the asset catagories repository
            Assert.True(mediaService.AssetCategories.Count == 0);
            var localAssetCategories = mediaCategoriesRespository.GetAllAsync().Result;
            Assert.IsTrue(localAssetCategories.Count == 0);

            // ACT - The user has authenticated, this should trigger the media service initialization
            authenticationService.RaiseAuthenticationChanged("test", true);

            // ASSERT - media service and the local repository have been populated with asset categories from the Api
            //Assert.That(() => mediaService.AssetCategories != null && mediaService.AssetCategories.Count > 0, Is.True.After(6000, 100));
            //Assert.That(() => mediaCategoriesRespository.GetAllAsync().Result.Count != 0, Is.True.After(6000, 100));
        }

        public async Task MediaServiceCanBeUsedToUploadPhotos()
        {
            // ARRANGE - Set up the authentication service, the media service and the assetcatagories repository.

            var userProvider = new Mock<IRepository<User>>();
            userProvider.Setup(s => s.GetAllAsync()).ReturnsAsync(() => new List<User> { new User { Alias = "a" } });

            Mvx.IoCProvider.RegisterSingleton<IRepository<User>>(userProvider.Object);

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IClaimsService, ClaimsServiceStub>();
            var authenticationService = Mvx.IoCProvider.Resolve<IAuthenticationService>() as AuthenticationServiceStub;

            var dataBaseInitiaser = Mvx.IoCProvider.Resolve<IDatabaseInitializer>();
            await dataBaseInitiaser.InitializeAsync("a");

            var mediaService = Mvx.IoCProvider.Resolve<IMediaService>();

            authenticationService.RaiseAuthenticationChanged("a", true);

            var rest = Mvx.IoCProvider.Resolve<IRestService>();
            rest.Reset("eyJ0eXAiOiJKV1QiLCJub25jZSI6IkFRQUJBQUFBQUFBUDB3TGxxZExWVG9PcEE0a3d6U254NG9YTFB4RnVQeUpMcEpPVHBmaEduLXlPejdpNTdVLU9NRHFOenlTMENiODhROGhabmhKZXdBZ0tKLU82bnpNRktiQTVzZ09fNk5vcnNlODV3NFgzZWlBQSIsImFsZyI6IlJTMjU2IiwieDV0IjoidTRPZk5GUEh3RUJvc0hqdHJhdU9iVjg0TG5ZIiwia2lkIjoidTRPZk5GUEh3RUJvc0hqdHJhdU9iVjg0TG5ZIn0.eyJhdWQiOiJodHRwczovL2dyYXBoLm1pY3Jvc29mdC5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9hZjFkMTkzZS00NmZkLTQ3NjUtOGVlYi03NjI4YTRiODc3YjUvIiwiaWF0IjoxNTYzMjAyMDcyLCJuYmYiOjE1NjMyMDIwNzIsImV4cCI6MTU2MzIwNTk3MiwiYWNjdCI6MCwiYWNyIjoiMSIsImFpbyI6IjQyRmdZSWllLzdNak5lbW5aSFhpWno2UHladWFKaGhYM3J0NTg0V2lCQWZ6Vzl1Rm1mY0EiLCJhbXIiOlsicHdkIl0sImFwcF9kaXNwbGF5bmFtZSI6IlNlZGd3aWNrIExpZ2h0bmluZyBEZXZlbG9wbWVudCBUZXN0IiwiYXBwaWQiOiIzOTlkZjU4Zi00NDAwLTQzMzItOTUwYS0wNGJlNGU3ZWVhYTIiLCJhcHBpZGFjciI6IjAiLCJpcGFkZHIiOiIyMTYuODAuOTMuMiIsIm5hbWUiOiJtcm9iYmlucyIsIm9pZCI6IjBlNGVjNjNhLTBjN2EtNGIzOS1iY2MzLTlmYTE4OWRjODBjYSIsInBsYXRmIjoiMiIsInB1aWQiOiIxMDAzMjAwMDRCMTM3MDZFIiwic2NwIjoib3BlbmlkIHByb2ZpbGUgVXNlci5SZWFkIGVtYWlsIiwic2lnbmluX3N0YXRlIjpbImttc2kiXSwic3ViIjoidHk2NlVMaHVhbzR1OUVWaTJkb0ZyeE9wbGtTZmI3Sm9rNE5kdS0tVExJdyIsInRpZCI6ImFmMWQxOTNlLTQ2ZmQtNDc2NS04ZWViLTc2MjhhNGI4NzdiNSIsInVuaXF1ZV9uYW1lIjoibXJvYmJpbnNAcHJvZHVjdGl2ZWVkZ2UuY29tIiwidXBuIjoibXJvYmJpbnNAcHJvZHVjdGl2ZWVkZ2UuY29tIiwidXRpIjoiLU9FOVNxMHdGa0dlYVBKd0JiaFlBQSIsInZlciI6IjEuMCIsInhtc19zdCI6eyJzdWIiOiJ6N2pyeHVZQ1ZXcURLOXcwLVpRdDJrcXhDX050bFpvWDl4eVlKTkJRZzRvIn0sInhtc190Y2R0IjoxNDM1Njc4Mjg5fQ.XUeqcHs0w9FZR0rbxjszOSd8JuBiuDHzQOMRjU-mNYGNy76-8e95Wy4Wy2ZV-wFtWvqP3QsQo_AA2o5eZcZtz0hJu9f1AJ-XWDpPg47bgCKYTPs4GQygmGAoL-jt_x73Sl_AVIlUATBmTPw-Y6rm1A7oQb6w0AQs6h2HPW3YM7AHrsdHy-KMmum1zkf5vbEh_4bkUsYSk5JYt0g2Gav70pXgfiPcGae_tI4_YrOXdD4_YNsTubRiAVfjLkMMD--kF2-Aas05P9y2NMCvpNiLs9IV47Lonk3h6Kc27LJ9qqZzjnsMtIZUrjxLxlEmSB2EW3AXNZiz-Nu4YDGalgWrIQ");

            Assert.That(() => mediaService.AssetCategories != null && mediaService.AssetCategories.Count > 0, Is.True.After(2000, 100));

            var mediaCategoriesRespository = Mvx.IoCProvider.Resolve<IRepository<AssetCategory>>();
            mediaCategoriesRespository.Initialize("test").Wait();

            var assetRepository = Mvx.IoCProvider.Resolve<IDatabaseInitializer>();
            await assetRepository.InitializeAsync("a");

            var claimService = Mvx.IoCProvider.Resolve<IClaimsService>() as ClaimsServiceStub;

            var basePath = "./../../../";

            var bytes = await File.ReadAllBytesAsync(basePath + "test_decrypted.png");

            await claimService.SelectClaim(claimService.ConstantSelectedClaim.Id.Value);
            await mediaService.Initialize(claimService.ConstantSelectedClaim.Id.Value);

            var result = await mediaService.AddPhoto(bytes, 1.02, 2.03);
            Assert.True(result.Status == PE.Shared.Enums.ServiceResultStatus.Success);
            Assert.NotNull(result.Payload);
            Assert.NotNull(result.Payload.Url);
        }
    }
}
