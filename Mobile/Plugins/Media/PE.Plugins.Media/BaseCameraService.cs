using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;

namespace PE.Plugins.Media
{
    public abstract class BaseCameraService
    {
        public bool IsCameraAvailable()
        {
            return CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported;
        }

        public async Task<byte[]> TakePhotoAsync(string directory)
        {
            var shouldSave = true;
            if (string.IsNullOrEmpty(directory))
            {
                shouldSave = false;
                directory = "temp";
            }


            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = directory,
                SaveToAlbum = shouldSave,
                CompressionQuality = 70,
                CustomPhotoSize = 80,
                PhotoSize = PhotoSize.MaxWidthHeight,
                MaxWidthHeight = 2000,
                DefaultCamera = CameraDevice.Rear
            });

            if (file == null)
            {
                return null;
            }

            using (var stream = file.GetStreamWithImageRotatedForExternalStorage())
            {
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                stream.Flush();
                stream.Close();
                return data;
            }
        }

        public bool IsPickPhotoSupported()
        {
            return CrossMedia.Current.IsPickPhotoSupported;
        }

        public async Task<byte[]> PickPhotoAsync()
        {
            var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
            });


            if (file == null)
            {
                return null;
            }

            using (var stream = file.GetStream())
            {
                var data = new byte[1024];
                while (true)
                {
                    var count = stream.Read(data, 0, 1024);
                    if (count < 1024) break;
                }
                return data;
            }
        }
    }
}

