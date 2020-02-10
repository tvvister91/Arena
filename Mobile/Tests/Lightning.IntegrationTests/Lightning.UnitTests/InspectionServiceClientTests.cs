using System.Collections.Generic;
using System.Threading.Tasks;

using MvvmCross;
using NUnit.Framework;
using PE.Modules.Sedgwick.Contracts;
using PE.Modules.Sedgwick.Models.Inspection;
using PE.Shared.Models;

namespace Lightning.IntegrationTests
{
    //public class InspectionServiceClientTests : IntegrationTestBase
    //{

        //[Ignore("Ignoring until we implement login in tests, this endpoint now requires a token")]
        //public Task TestGetClaimFormInput()
        //{
        //    return TestGetAll<ClaimFormInput>();
        //}

        //[Ignore("Ignoring until we implement login in tests, this endpoint now requires a token")]
        //public Task TestGetClaimInputResult()
        //{
        //    return TestGetAll<ClaimInputResult>();
        //}

        //[Ignore("Ignoring until we implement login in tests, this endpoint now requires a token")]
        //public Task TestGetClaimResult()
        //{
        //    return TestGetAll<ClaimResult>();
        //}

        //[Test]
        //public Task TestGetForm()
        //{
        //    return TestGetAll<Form>();
        //}

        //[Test]
        //public Task TestGetFormInput()
        //{
        //    return TestGetAll<FormInput>();
        //}

        //[Test]
        //public Task TestGetFormInputType()
        //{
        //    return TestGetAll<FormInputType>();
        //}

        //[Test]
        //public Task TestGetFormLineOfBusiness()
        //{
        //    return TestGetAll<FormLineOfBusiness>();
        //}

        //[Test]
        //public Task TestGetFormOption()
        //{
        //    return TestGetAll<FormOption>();
        //}

        //[Test]
        //public Task TestGetFormOptionType()
        //{
        //    return TestGetAll<FormOptionType>();
        //}

        //[Test]
        //public Task TestGetFormRule()
        //{
        //    return TestGetAll<FormRule>();
        //}

        //[Test]
        //public Task TestGetFormText()
        //{
        //    return TestGetAll<FormText>();
        //}

        //private async Task TestGetAll<T>() where T : IInspectionModel
        //{
        //    var inspectionServiceClient = Mvx.IoCProvider.Resolve<IInspectionServiceClient>();
        //    ServiceResult<IList<T>> result = await inspectionServiceClient.Retrieve<T>();
        //    Assert.That(result.Status == PE.Shared.Enums.ServiceResultStatus.Success);
        //    Assert.That(result.Payload != null);
        //}
    //}
}
