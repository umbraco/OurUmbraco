using Jering.IocServices.System.IO;
using Markdig.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiVideoBlocks
{
    /// <summary>
    /// The implementation of <see cref="MediaBlockFactory{TBlock, TBlockOptions, TExtensionOptions}"/> for creating <see cref="FlexiVideoBlock"/>s.
    /// </summary>
    public class FlexiVideoBlockFactory : MediaBlockFactory<FlexiVideoBlock, IFlexiVideoBlockOptions, IFlexiVideoBlocksExtensionOptions>
    {
        private readonly IVideoService _videoService;
        private readonly IOptionsService<IFlexiVideoBlockOptions, IFlexiVideoBlocksExtensionOptions> _optionsService;

        /// <summary>
        /// Creates a <see cref="FlexiVideoBlockFactory"/>.
        /// </summary>
        /// <param name="videoService">The service that handles video file operations.</param>
        /// <param name="directoryService">The service that handles searching for video files.</param>
        /// <param name="optionsService">The service for creating <see cref="IFlexiVideoBlockOptions"/> and <see cref="IFlexiVideoBlocksExtensionOptions"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="videoService"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="directoryService"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="optionsService"/> is <c>null</c>.</exception>
        public FlexiVideoBlockFactory(IVideoService videoService,
            IDirectoryService directoryService,
            IOptionsService<IFlexiVideoBlockOptions, IFlexiVideoBlocksExtensionOptions> optionsService) :
            base(directoryService)
        {
            _optionsService = optionsService ?? throw new ArgumentNullException(nameof(optionsService));
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
        }

        /// <summary>
        /// Creates a <see cref="FlexiVideoBlock"/>.
        /// </summary>
        /// <param name="proxyJsonBlock">The <see cref="ProxyJsonBlock"/> containing data for the <see cref="FlexiVideoBlock"/>.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="FlexiVideoBlock"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="proxyJsonBlock"/> is <c>null</c>.</exception>S
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockProcessor"/> is <c>null</c>.</exception>
        /// <exception cref="OptionsException">Thrown if an option is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if FFmpeg is not available.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an exception is thrown while attempting to run FFmpeg.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an FFmpeg run fails (exit code > 0).</exception>
        /// <exception cref="InvalidOperationException">Thrown if metadata retrieved from the video is invalid.</exception>
        public override FlexiVideoBlock Create(ProxyJsonBlock proxyJsonBlock, BlockProcessor blockProcessor)
        {
            (IFlexiVideoBlockOptions flexiVideoBlockOptions, IFlexiVideoBlocksExtensionOptions flexiVideoBlocksExtensionOptions) = _optionsService.
                CreateOptions(blockProcessor, proxyJsonBlock);

            // Block name
            string blockName = ResolveBlockName(flexiVideoBlockOptions.BlockName);

            // Src
            string fileName = ValidateSrcAndResolveFileName(flexiVideoBlockOptions);

            // Type
            string type = ResolveType(fileName, flexiVideoBlockOptions.Type, flexiVideoBlocksExtensionOptions.MimeTypes);

            // Enable file operations
            double width = flexiVideoBlockOptions.Width;
            double height = flexiVideoBlockOptions.Height;
            double duration = flexiVideoBlockOptions.Duration;
            bool generatePoster = flexiVideoBlockOptions.GeneratePoster;
            string poster = flexiVideoBlockOptions.Poster;
            bool enableFileOperations = ResolveEnableFileOperations(flexiVideoBlockOptions.EnableFileOperations,
                flexiVideoBlocksExtensionOptions.LocalMediaDirectory,
                generatePoster,
                poster,
                width,
                height,
                duration);

            // Source local absolute path
            string localAbsolutePath = ResolveLocalAbsolutePath(enableFileOperations,
                fileName,
                flexiVideoBlocksExtensionOptions);

            // Dimensions and duration
            (double resolvedWidth, double resolvedHeight, double aspectRatio, double resolvedDuration) = ResolveDimensionsAndDuration(localAbsolutePath, width, height, duration);

            // Poster
            string src = flexiVideoBlockOptions.Src;
            string resolvedPoster = ResolvePoster(localAbsolutePath, src, poster, generatePoster);

            // Create block
            return new FlexiVideoBlock(blockName,
                src,
                type,
                resolvedPoster,
                resolvedWidth,
                resolvedHeight,
                aspectRatio,
                resolvedDuration,
                flexiVideoBlockOptions.Spinner,
                flexiVideoBlockOptions.PlayIcon,
                flexiVideoBlockOptions.PauseIcon,
                flexiVideoBlockOptions.FullscreenIcon,
                flexiVideoBlockOptions.ExitFullscreenIcon,
                flexiVideoBlockOptions.ErrorIcon,
                flexiVideoBlockOptions.Attributes,
                proxyJsonBlock.Parser)
            {
                Column = proxyJsonBlock.Column,
                Line = proxyJsonBlock.Line,
                Span = proxyJsonBlock.Span
            };
        }

        internal virtual string ResolveBlockName(string blockName)
        {
            return string.IsNullOrWhiteSpace(blockName) ? "flexi-video" : blockName;
        }

        // Types - https://www.iana.org/assignments/media-types/media-types.xhtml#video
        internal virtual string ResolveType(string fileName, string type, ReadOnlyDictionary<string, string> mimeTypes)
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                return type;
            }

            foreach (KeyValuePair<string, string> extensionTypePair in mimeTypes)
            {
                string key = extensionTypePair.Key;
                if (fileName.EndsWith(key, StringComparison.OrdinalIgnoreCase))
                {
                    return extensionTypePair.Value;
                }
            }

            return null;
        }

        internal virtual bool ResolveEnableFileOperations(bool enableFileOperations,
            string localMediaDirectory,
            bool generatePoster,
            string poster,
            double width,
            double height,
            double duration)
        {
            return enableFileOperations &&
                !string.IsNullOrWhiteSpace(localMediaDirectory) &&
                (width <= 0 || height <= 0 || duration <= 0 || generatePoster && string.IsNullOrWhiteSpace(poster));
        }

        // Refer to fpb resolve dimensions, copy it
        internal virtual (double width, double height, double aspectRatio, double duration) ResolveDimensionsAndDuration(string localAbsolutePath,
            double specifiedWidth,
            double specifiedHeight,
            double specifiedDuration)
        {
            bool widthSpecified = specifiedWidth > 0;
            bool heightSpecified = specifiedHeight > 0;
            bool durationSpecified = specifiedDuration > 0;

            if (specifiedWidth > 0 && specifiedHeight > 0 && specifiedDuration > 0) // All specified
            {
                return (specifiedWidth, specifiedHeight, specifiedHeight / specifiedWidth * 100, specifiedDuration);
            }

            if (localAbsolutePath == null) // Can't retrieve anything from file
            {
                return (widthSpecified ? specifiedWidth : 0,
                    heightSpecified ? specifiedHeight : 0,
                    widthSpecified && heightSpecified ? specifiedHeight / specifiedWidth * 100 : 0,
                    durationSpecified ? specifiedDuration : 0);
            }

            (double retrievedWidth, double retrievedHeight, double retrievedDuration) = _videoService.GetVideoDimensionsAndDuration(localAbsolutePath);

            double width = widthSpecified ? specifiedWidth : retrievedWidth,
                   height = heightSpecified ? specifiedHeight : retrievedHeight;

            return (width, height, width == 0 || height == 0 ? 0 : height / width * 100, durationSpecified ? specifiedDuration : retrievedDuration);
        }

        internal virtual string ResolvePoster(string localAbsolutePath,
            string src,
            string poster,
            bool generatePoster)
        {
            if (!string.IsNullOrWhiteSpace(poster))
            {
                return poster;
            }

            if (!generatePoster || localAbsolutePath == null)
            {
                return null;
            }

            // TODO we could use ReadOnlySpan<char> to avoid allocations, however we need some way to concat spans, doing that by hand isn't worth the time (considering perf requirements).
            // Also, it looks like Concat(ReadOnlySpan<char>...) will be a framework method eventually - https://github.com/dotnet/corefx/issues/34330, perhaps before 
            // this project's first production ready release.

            // Generate poster local absolute path
            int extensionLength = Path.GetExtension(localAbsolutePath).Length; // We can't use LastIndexOf('.') because file might have no extension but there may be a '.' somewhere else
            string posterLocalAbsolutePath = $"{(extensionLength == 0 ? localAbsolutePath : localAbsolutePath.Substring(0, localAbsolutePath.Length - extensionLength))}_poster.png";

            // Generate new poster
            _videoService.GeneratePoster(localAbsolutePath, posterLocalAbsolutePath);

            // Poster attribute value
            return $"{(extensionLength == 0 ? src : src.Substring(0, src.Length - extensionLength))}_poster.png";
        }
    }
}
