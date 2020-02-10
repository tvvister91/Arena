using Android.Graphics;
using System.IO;

namespace PE.Plugins.Media.Droid
{
    public static class BitmapExtensions
    {
        public static byte[] ExportToArray(this Bitmap bitmap)
        {
            using (var outStream = new MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat.Png, 100, outStream);
                outStream.Flush();
                return outStream.ToArray();
            }
        }
    }
}