using Android.Graphics;
using System.IO;

namespace PE.Plugins.Media.Droid
{
    public class MediaService : IMediaService
    {
        #region Operations

        public byte[] RotateAndResizeImage(byte[] imageData, int width, int height, int degrees)
        {
            var scale = ((width != 0) && (height != 0));
            //  nothing to do
            if (!scale && (degrees == 0)) return imageData;

            using (var inStream = new MemoryStream(imageData))
            {
                Bitmap original = BitmapFactory.DecodeStream(inStream);

                float scaleWidth = original.Width;
                float scaleHeight = original.Height;
                if (scale)
                {
                    scaleWidth = (float)width / (float)original.Width;
                    scaleHeight = (float)height / (float)original.Height;
                }
                //  apply transformations
                using (var matrix = new Matrix())
                {
                    //  rotate
                    if (degrees > 0) matrix.PostRotate(degrees);
                    //  scale
                    if (scale) matrix.PostScale(scaleWidth, scaleHeight);
                    //  create the scaled bitmap
                    var resizedBitmap = Bitmap.CreateBitmap(original, 0, 0, width, height, matrix, false);
                    original.Recycle();
                    return resizedBitmap.ExportToArray();
                }
            }
        }

        public byte[] ResizeImage(byte[] imageData, int width, int height)
        {
            using (var inStream = new MemoryStream(imageData))
            {
                Bitmap original = BitmapFactory.DecodeStream(inStream);

                float scaleWidth = (float)width / (float)original.Width;
                float scaleHeight = (float)height / (float)original.Height;

                using (var matrix = new Matrix())
                {
                    //  scale the bitmap
                    matrix.PostScale(scaleWidth, scaleHeight);
                    //  create the scaled bitmap
                    var resizedBitmap = Bitmap.CreateBitmap(original, 0, 0, width, height, matrix, false);
                    original.Recycle();
                    return resizedBitmap.ExportToArray();
                }
            }
        }

        public byte[] RotateImage(byte[] imageData, int degrees)
        {
            using (var inStream = new MemoryStream(imageData))
            {
                Bitmap original = BitmapFactory.DecodeStream(inStream);

                Matrix matrix = new Matrix();
                matrix.PostRotate(degrees);

                Bitmap rotated = Bitmap.CreateBitmap(original, 0, 0, original.Width, original.Height, matrix, true);
                return rotated.ExportToArray();
            }
        }

        #endregion Operations
    }
}