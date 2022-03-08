using Jering.IocServices.System.IO;
using SixLabors.ImageSharp;
using System;
using System.IO;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiPictureBlocks
{
    // We should be able to use ImageSharp, but keep up to date on licensing - https://github.com/SixLabors/ImageSharp/issues/1024.
    // If we can no longer use ImageSharp we might have to go the out of process route with imagemagick.
    /// <summary>
    /// The default implementation of <see cref="IImageService"/>.
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly IFileService _fileService;

        /// <summary>
        /// Creates an <see cref="ImageService"/>.
        /// </summary>
        /// <param name="fileService">The service for reading image files.</param>
        public ImageService(IFileService fileService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        /// <inheritdoc />
        public (int width, int height) GetImageDimensions(string path)
        {
            IImageInfo imageInfo = null;
            FileStream fileStream = null;
            try
            {
                fileStream = _fileService.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                imageInfo = Image.Identify(fileStream);
            }
            catch (Exception exception)
            {
                // From the source code, Identify throws a NotSupportedException if fileStream.CanRead is false.
                // File.Open may throw too, so just catch any exception and add context.
                throw new InvalidOperationException(string.Format(Strings.InvalidOperationsException_ImageService_ExceptionThrownWhileAttemptingToReadDimensionsFromLocalImageFile,
                        path),
                    exception);
            }
            finally
            {
                fileStream?.Dispose();
            }

            if (imageInfo == null)
            {
                // Identify returns null if no "suitable detector found" - https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Image.html#SixLabors_ImageSharp_Image_Identify_Stream_
                throw new InvalidOperationException(string.Format(Strings.InvalidOperationException_ImageService_UnableToReadDimensionsFromImageFile, path));
            }

            return (imageInfo.Width, imageInfo.Height);
        }
    }
}
