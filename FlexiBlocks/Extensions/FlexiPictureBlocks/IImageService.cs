using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiPictureBlocks
{
    /// <summary>
    /// An abstraction for interacting with images.
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Gets the width and height of an image.
        /// </summary>
        /// <param name="path">The image's file path.</param>
        /// <exception cref="InvalidOperationException">Thrown if an exception is thrown while attempting to read dimensions from the image.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the image is corrupted or encoded in an unsupported format.</exception>
        (int width, int height) GetImageDimensions(string path);
    }
}
