using System;
using Arena.Core.Enums;

namespace Arena.Core.Services
{
    public interface IBuildVersionProvider
    {
        BuildVersionEnum Version { get; }
    }
}
