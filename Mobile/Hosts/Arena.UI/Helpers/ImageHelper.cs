using Xamarin.Forms;

namespace Arena.UI.Helpers
{
    public static class ImageHelper
    {
        public static string GetImagePath(string name, string folderName = default(string))
        {
            if (folderName != default(string))
            {
                folderName += "/";
            }

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                case Device.Android:
                    return name;
                case Device.UWP: return $"Assets/{folderName}{name}.png";
                default: return string.Empty;
            }
        }
    }
}
