using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace PE.Plugins.Media.WindowsCommon
{
    class CameraService : BaseCameraService
    {
        public override byte[] ResizeImage(byte[] imageData, float width = -1F, float height = -1F)
        {
            byte[] resizedData = null;

            Task<byte[]> task = Task.Run<byte[]>(async () => await ResizeImageAsync(imageData, width, height));
            resizedData = task.Result;
            
            return resizedData == null ? imageData : resizedData;
        }

        private async Task<byte[]> ResizeImageAsync(byte[] imageData, float width = -1F, float height = -1F)
        {
            BitmapImage image = new BitmapImage();
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(imageData.AsBuffer());
                stream.Seek(0);
                await image.SetSourceAsync(stream);
            }
            int h = image.PixelHeight;
            int w = image.PixelWidth;

            if (h * w > 25 * 1024 * 1024)
            {
                byte[] resizedData;
                using (var streamIn = new MemoryStream(imageData))
                {
                    using (var imageStream = streamIn.AsRandomAccessStream())
                    {
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(imageStream);
                        var resizedStream = new InMemoryRandomAccessStream();
                        var encoder = await BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder);
                        encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                        encoder.BitmapTransform.ScaledHeight = (uint)height;
                        encoder.BitmapTransform.ScaledWidth = (uint)width;
                        await encoder.FlushAsync();
                        resizedStream.Seek(0);
                        resizedData = new byte[resizedStream.Size];
                        await resizedStream.ReadAsync(resizedData.AsBuffer(), (uint)resizedStream.Size, InputStreamOptions.None);
                    }
                }

                return resizedData;
            }
            else
            {
                return imageData;
            }
        }
    }
}
