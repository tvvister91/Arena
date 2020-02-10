using System;
using Lightning.Core.Services;

namespace Lightning.UnitTests.Stub
{
    public class EncryptionKeyProviderStub : IEncryptionKeyProvider
    {
        private byte[] _Key = { 143, 61, 212, 1, 4, 97, 79, 213, 38, 251, 242, 59, 51, 209, 198, 92, 145, 81, 167, 111, 206, 175, 133, 1, 117, 196, 136, 21, 128, 206, 9, 147 };
        private byte[] _IV = { 138, 51, 115, 111, 244, 28, 192, 0, 116, 19, 70, 149, 100, 88, 74, 53 };

        public (byte[] Key, byte[] IV) KeyPair => (_Key, _IV);
    }
}
