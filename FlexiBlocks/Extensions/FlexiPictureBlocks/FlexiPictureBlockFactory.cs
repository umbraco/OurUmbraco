using Jering.IocServices.System.IO;
using Markdig.Parsers;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiPictureBlocks
{
    /// <summary>
    /// The implementation of <see cref="MediaBlockFactory{TBlock, TBlockOptions, TExtensionOptions}"/> for creating <see cref="FlexiPictureBlock"/>s.
    /// </summary>
    public class FlexiPictureBlockFactory : MediaBlockFactory<FlexiPictureBlock, IFlexiPictureBlockOptions, IFlexiPictureBlocksExtensionOptions>
    {
        private readonly IOptionsService<IFlexiPictureBlockOptions, IFlexiPictureBlocksExtensionOptions> _optionsService;
        private readonly IImageService _imageService;

        /// <summary>
        /// Creates a <see cref="FlexiPictureBlockFactory"/>.
        /// </summary>
        /// <param name="imageService">The service that handles image file operations.</param>
        /// <param name="directoryService">The service that handles searching for image files.</param>
        /// <param name="optionsService">The service for creating <see cref="IFlexiPictureBlockOptions"/> and <see cref="IFlexiPictureBlocksExtensionOptions"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="imageService"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="directoryService"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="optionsService"/> is <c>null</c>.</exception>
        public FlexiPictureBlockFactory(IImageService imageService,
            IDirectoryService directoryService,
            IOptionsService<IFlexiPictureBlockOptions, IFlexiPictureBlocksExtensionOptions> optionsService) :
            base(directoryService)
        {
            _optionsService = optionsService ?? throw new ArgumentNullException(nameof(optionsService));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }

        /// <summary>
        /// Creates a <see cref="FlexiPictureBlock"/>.
        /// </summary>
        /// <param name="proxyJsonBlock">The <see cref="ProxyJsonBlock"/> containing data for the <see cref="FlexiPictureBlock"/>.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiPictureBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="proxyJsonBlock"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        /// <exception cref="OptionsException">Thrown if an option is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if dimensions cannot be read from the local image file.</exception>
        public override FlexiPictureBlock Create(ProxyJsonBlock proxyJsonBlock, BlockProcessor blockProcessor)
        {
            (IFlexiPictureBlockOptions flexiPictureBlockOptions, IFlexiPictureBlocksExtensionOptions flexiPictureBlocksExtensionOptions) = _optionsService.
                CreateOptions(blockProcessor, proxyJsonBlock);

            // Block name
            string blockName = ResolveBlockName(flexiPictureBlockOptions.BlockName);

            // Src
            string fileName = ValidateSrcAndResolveFileName(flexiPictureBlockOptions);

            // Enable file operations
            double height = flexiPictureBlockOptions.Height;
            double width = flexiPictureBlockOptions.Width;
            bool enableFileOperations = ResolveEnableFileOperations(flexiPictureBlockOptions.EnableFileOperations,
                flexiPictureBlocksExtensionOptions.LocalMediaDirectory,
                width,
                height);

            // Source local absolute path
            string localAbsolutePath = ResolveLocalAbsolutePath(enableFileOperations, fileName, flexiPictureBlocksExtensionOptions);

            // Resolve intrinsic width and height
            (double resolvedWidth, double resolvedHeight, double aspectRatio) = ResolveDimensions(localAbsolutePath, width, height);

            // Create block
            return new FlexiPictureBlock(blockName,
                flexiPictureBlockOptions.Src,
                flexiPictureBlockOptions.Alt,
                flexiPictureBlockOptions.Lazy,
                resolvedWidth,
                resolvedHeight,
                aspectRatio,
                flexiPictureBlockOptions.ExitFullscreenIcon,
                flexiPictureBlockOptions.ErrorIcon,
                flexiPictureBlockOptions.Spinner,
                flexiPictureBlockOptions.Attributes,
                proxyJsonBlock.Parser)
            {
                Column = proxyJsonBlock.Column,
                Line = proxyJsonBlock.Line,
                Span = proxyJsonBlock.Span
            };
        }

        internal virtual string ResolveBlockName(string blockName)
        {
            return string.IsNullOrWhiteSpace(blockName) ? "flexi-picture" : blockName;
        }

        internal virtual bool ResolveEnableFileOperations(bool enableFileOperations, string localMediaDirectory, double width, double height)
        {
            return enableFileOperations && !string.IsNullOrWhiteSpace(localMediaDirectory) && (width <= 0 || height <= 0);
        }

        internal virtual (double width, double height, double aspectRatio) ResolveDimensions(string localAbsolutePath, double specifiedWidth, double specifiedHeight)
        {
            bool widthSpecified = specifiedWidth > 0;
            bool heightSpecified = specifiedHeight > 0;

            if (widthSpecified && heightSpecified) // Both specified
            {
                return (specifiedWidth, specifiedHeight, specifiedHeight / specifiedWidth * 100);
            }

            if (localAbsolutePath == null) // Can't retrieve dimensions from file
            {
                return (widthSpecified ? specifiedWidth : 0, heightSpecified ? specifiedHeight : 0, 0);
            }

            // Get width and/or height from file
            (int retrievedWidth, int retrievedHeight) = _imageService.GetImageDimensions(localAbsolutePath);

            double width = widthSpecified ? specifiedWidth : retrievedWidth,
                   height = heightSpecified ? specifiedHeight : retrievedHeight;

            // In the unlikely event that image metadata is corrupt, width or height might be 0. Avoid divide by 0 exceptions
            return (width, height, width == 0 || height == 0 ? 0 : height / width * 100);
        }
    }
}
