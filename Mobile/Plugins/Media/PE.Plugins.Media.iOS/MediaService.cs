using CoreGraphics;

using System;
using System.Drawing;

using UIKit;

namespace PE.Plugins.Media.iOS
{
    public class MediaService : IMediaService
    {
        #region Operations

        public byte[] ResizeImage(byte[] imageData, int newWidth, int newheight)
        {
            //  get the original image from data
            UIImage original = ImageFromByteArray(imageData);
            if (original == null) return imageData;

            var sourceSize = original.Size;
            var maxResizeFactor = Math.Max(newWidth / sourceSize.Width, newheight / sourceSize.Height);
            if (maxResizeFactor > 1) return imageData;

            var width = maxResizeFactor * sourceSize.Width;
            var height = maxResizeFactor * sourceSize.Height;

            UIGraphics.BeginImageContext(new CGSize(width, height));
            original.Draw(new CGRect(0, 0, width, height));
            var newImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return newImage.AsPNG().ToArray(); ;
        }

        public byte[] RotateImage(byte[] imageData, int degrees)
        {
            //  get the original image from data
            UIImage originalImage = ImageFromByteArray(imageData);
            if (originalImage == null) return imageData;


            // calculate the size of the rotated view's containing box for our drawing space
            var rotated = new UIView(new CGRect(0, 0, originalImage.Size.Width, originalImage.Size.Height));
            CGAffineTransform t = CGAffineTransform.MakeRotation(DegreesToRadians(degrees));
            rotated.Transform = t;
            CGSize rotatedSize = rotated.Frame.Size;

            //  create the graphics context
            try
            {
                UIGraphics.BeginImageContext(rotatedSize);
                CGContext context = UIGraphics.GetCurrentContext();

                // Move the origin to the middle of the image so we will rotate and scale around the center.
                context.TranslateCTM(rotatedSize.Width / 2, rotatedSize.Height / 2);

                //   // Rotate the image context
                context.RotateCTM(DegreesToRadians(degrees));

                // Now, draw the rotated/scaled image into the context
                context.ScaleCTM(1.0f, -1.0f);
                context.DrawImage(new CGRect(-originalImage.Size.Width / 2, -originalImage.Size.Height / 2, originalImage.Size.Width, originalImage.Size.Height), originalImage.CGImage);

                var newImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                return newImage.AsPNG().ToArray();
                //}
            }
            catch
            {
                return null;
            }
            finally
            {
                UIGraphics.EndImageContext();
            }
        }

        public byte[] RotateAndResizeImage(byte[] imageData, int newWidth, int newHeight, int degrees)
        {
            //  get the original image from data
            UIImage original = ImageFromByteArray(imageData);
            if (original == null) return imageData;

            var shouldScale = ((newWidth != 0) && (newHeight != 0));
            //  nothing to do
            if (!shouldScale && (degrees == 0)) return imageData;
            nfloat width = (shouldScale) ? newWidth : original.Size.Width;
            nfloat height = (shouldScale) ? newHeight : original.Size.Height;

            //  scale multipliers
            nfloat sx = (!shouldScale) ? 1.0f : width / original.Size.Width;
            nfloat sy = (!shouldScale) ? -1.0f : -height / original.Size.Height;

            //  initialize graphics context
            try
            {
                UIGraphics.BeginImageContext(new CGSize(newWidth, newHeight));
                //  get context
                CGContext context = UIGraphics.GetCurrentContext();
                //  set centre point/rotate point of the image
                context.TranslateCTM(newWidth / 2, newHeight / 2);
                // rotate the image context
                context.RotateCTM(DegreesToRadians(degrees));
                // scale the image
                context.ScaleCTM(sx, sy);
                //  draw the original image at the new size and rotation
                original.Draw(new CGRect(0, 0, width, height));
                var newImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();
                return newImage.AsPNG().ToArray(); ;
            }
            finally
            {
                UIGraphics.EndImageContext();
            }
        }

        #endregion Operations

        #region Helpers

        private UIImage ImageFromByteArray(byte[] data)
        {
            if (data == null) return null;

            try
            {
                return new UIKit.UIImage(Foundation.NSData.FromArray(data));
            }
            catch
            {
                return null;
            }
        }

        private float DegreesToRadians(int degrees)
        {
            return (float)((Math.PI / 180) * degrees);
        }

        #endregion Helpers
    }
}