using System.Threading.Tasks;

namespace PE.Plugins.Media
{
    public interface ICameraService
    {
        bool IsCameraAvailable();

        Task<byte[]> TakePhotoAsync(string directory);

        bool IsPickPhotoSupported();

        Task<byte[]> PickPhotoAsync();
    }
}
