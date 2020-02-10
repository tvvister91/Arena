using System;
using Plugin.Permissions.Abstractions;

namespace Arena.Core.Services
{
    public interface ISettingsService
    {
        bool CheckFirstTimeUse(Permission permission);
    }
}
