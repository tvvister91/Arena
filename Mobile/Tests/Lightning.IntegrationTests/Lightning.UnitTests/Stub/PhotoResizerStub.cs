using System;
using Lightning.Core.Services;

namespace Lightning.UnitTests.Stub
{
    public class PhotoResizerStub : IImageResizerService
    {
        public byte[] ResizeImage(byte[] imageData, float width, float height)
        {
            return imageData;
        }
    }
}
