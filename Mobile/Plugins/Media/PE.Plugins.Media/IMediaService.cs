namespace PE.Plugins.Media
{
    public interface IMediaService
    {
        /// <summary>
        /// Resize and image to a new width and height
        /// </summary>
        byte[] ResizeImage(byte[] imageData, int width, int height);
        /// <summary>
        /// Rotate an image by a number of degrees
        /// </summary>
        byte[] RotateImage(byte[] imageData, int degrees);
        /// <summary>
        /// Rotate and resize and image. This may be more efficient than doing the two steps separately
        /// </summary>
        byte[] RotateAndResizeImage(byte[] imageData, int width, int height, int degrees);
    }
}
