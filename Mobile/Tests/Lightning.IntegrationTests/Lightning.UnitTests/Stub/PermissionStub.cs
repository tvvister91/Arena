using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lightning.Core.Services;
using PE.Plugins.Analytics;
using PE.Plugins.Dialogs;
using Plugin.Permissions.Abstractions;

namespace Lightning.IntegrationTests.Stub
{
    public class PermissionStub : PermissionService, IPermissionService
    {
        public Dictionary<Permission, PermissionStatus> TestPermissionConfiguration { get; } = new Dictionary<Permission, PermissionStatus>();

        public PermissionStub(IAnalyticsService analyticsService, IDialogService dialogService, ISettingsService settingsService) : base(analyticsService, dialogService, settingsService)
        {
        }

        public override Task<PermissionStatus> CheckPermissionStatusAsync(Permission permission)
        {
            return TestPermissionConfiguration.ContainsKey(permission) ? Task.FromResult(TestPermissionConfiguration[permission]) : Task.FromResult(PermissionStatus.Unknown);
        }

        public override bool OpenAppSettings()
        {
            throw new NotImplementedException();
        }

        public override Task<Dictionary<Permission, PermissionStatus>> RequestPermissionsAsync(Permission permission)
        {
            return Task.FromResult(TestPermissionConfiguration);
        }
    }
}
