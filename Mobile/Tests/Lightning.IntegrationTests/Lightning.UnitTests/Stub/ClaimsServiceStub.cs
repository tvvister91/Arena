using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

using PE.Modules.Sedgwick.Models.Claims;
using PE.Shared.Models;

using Lightning.Core.Enums;
using Lightning.Core.Infrastructure;
using Lightning.Core.Services;

namespace Lightning.IntegrationTests.Stub
{
    public class ClaimsServiceStub : IClaimsService
    {
        private Claim SelectedClaim;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        public event EventHandler UpdateReceived;
        public event EventHandler<ClaimChangedEventArgs> SelectedClaimUpdated;

        public Claim ConstantSelectedClaim = new Claim
            {Latitude = 41.890773, Longitude = -87.627432, Id = Guid.Parse("86a49d59-521d-43b6-af2b-349e5c59ac46")};

        public enum ClaimReturnSetting
        {
            NoClaims,
            FourClaims,
        }

        public ClaimReturnSetting ClaimReturn { get; set; }

        public static decimal MaxLatitude => 41.893559M;
        public static decimal MinLatitude => 41.890509M;

        public static decimal MaxLongitude => -87.626284M;
        public static decimal MinLongitude => -87.630565M;

        Claim IClaimsService.SelectedClaim
        {
            get => SelectedClaim;
            set => SelectedClaim = value;
        }

        event EventHandler<BaseClaimPayload> IClaimsService.UpdateReceived
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public Task<IEnumerable<Claim>> GetClaimsAsync()
        {
            if (ClaimReturn == ClaimReturnSetting.NoClaims)
            {
                return Task.FromResult<IEnumerable<Claim>>(new List<Claim>());
            }

            var claims = new List<Claim>()
            {
                new Claim {Latitude = 41.890773, Longitude = -87.627432},
                new Claim {Latitude = 41.890509, Longitude = -87.630565},
                new Claim {Latitude = 41.893208, Longitude = -87.630404},
                new Claim {Latitude = 41.893559, Longitude = -87.626284}
            };

            return Task.FromResult((IEnumerable<Claim>) claims);
        }

        public Task<ServiceResult<IList<Claim>>> GetClaimsWhereAsync(Expression<Func<Claim, bool>> predicate, bool force = false)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<Claim>> GetClaim(Guid id, bool useLocal = true)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<Claim>> SelectClaim(Guid id)
        {
            SelectedClaim = new Claim {Latitude = 41.890773, Longitude = -87.627432, Id = id};
            return Task.FromResult(new ServiceResult<Claim>
                {Status = PE.Shared.Enums.ServiceResultStatus.Success, Payload = SelectedClaim});
        }

        public void ResetSelectedClaim()
        {
            SelectedClaim = null;
        }

        public Claim GetSelectedClaim()
        {
            return SelectedClaim;
        }

        public bool IsSelectedClaimSubmitted =>
            ConstantSelectedClaim.StatusId == ClaimStatusId.FieldReportComplete.GetGuid();

        public ClaimUpdateState UpdateState => ClaimUpdateState.Undefined;

        public Task UpdateSelectedClaim(ClaimChangedValues changedValues = ClaimChangedValues.All)
        {
            throw new NotImplementedException();
        }

        public async Task SetSelectedClaimById(Guid id)
        {
            SelectedClaim = new Claim { Latitude = 41.890773, Longitude = -87.627432, Id = id };
        }

        public Task<ServiceResult<IList<Claim>>> UpdateClaims(bool force)
        {
            throw new NotImplementedException();
        }

        public Task UpdatedApplied()
        {
            throw new NotImplementedException();
        }

        public void ChangeTrackingSubscription(ITrackingService trackingService, bool add)
        {
            throw new NotImplementedException();
        }
    }
}