using Lightning.Core.Enums;
using Lightning.Core.Services;

namespace Lightning.UnitTests
{
    public class BuildVersionProviderStub : IBuildVersionProvider
    {
        public BuildVersionEnum Version { get; set; }
    }
}