using System;
using Arena.Core.Enums;

namespace Arena.Core.Services
{
    public class BuildVersionProviderService : IBuildVersionProvider
    {
        public BuildVersionEnum Version => BuildVersion.Version;

        private static class BuildVersion
        {
            /// <summary>
            /// The type of the current build. This is a compile time constant.
            /// </summary>
#if BUILD_SERVER
        public const BuildVersionEnum Version = BuildVersionEnum.TYPE_OF_BUILD;
#elif DEBUG
            public const BuildVersionEnum Version = BuildVersionEnum.Debug;
#else
        public const BuildVersionEnum Version = BuildVersionEnum.Dev;
#endif
        }
    }
}
