using Jering.IocServices.System.IO;
using Markdig.Parsers;
using Markdig.Syntax;
using System;
using System.IO;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// The implementation of <see cref="IJsonBlockFactory{TMain, TProxy}"/> for creating media blocks.
    /// </summary>
    public abstract class MediaBlockFactory<TBlock, TBlockOptions, TExtensionOptions> : IJsonBlockFactory<TBlock, ProxyJsonBlock>
        where TBlock : Block
        where TBlockOptions : IMediaBlockOptions<TBlockOptions>
        where TExtensionOptions : IMediaBlocksExtensionOptions<TBlockOptions>
    {
        private static readonly Uri _defaultBaseUri = new Uri("file:///");
        private readonly IDirectoryService _directoryService;

        /// <summary>
        /// Creates a <see cref="MediaBlockFactory{TBlock, TBlockOptions, TExtensionOptions}"/>.
        /// </summary>
        /// <param name="directoryService">The service that handles searching for media.</param>
        protected MediaBlockFactory(IDirectoryService directoryService)
        {
            _directoryService = directoryService ?? throw new ArgumentNullException(nameof(directoryService));
        }

        /// <summary>
        /// Creates a <see cref="ProxyJsonBlock"/>.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <see cref="ProxyJsonBlock"/>.</param>
        /// <param name="blockParser">The <see cref="BlockParser"/> parsing the <see cref="ProxyJsonBlock"/>.</param>
        public ProxyJsonBlock CreateProxyJsonBlock(BlockProcessor blockProcessor, BlockParser blockParser)
        {
            if (blockProcessor == null)
            {
                throw new ArgumentNullException(nameof(blockProcessor));
            }

            return new ProxyJsonBlock(typeof(TBlock).Name, blockParser) // TODO nameof(TBlock) test
            {
                Column = blockProcessor.Column,
                Span = { Start = blockProcessor.Start } // JsonBlockParser.ParseLine will update the span's end
                // Line is assigned by BlockProcessor
            };
        }

        /// <summary>
        /// Creates a <typeparamref name="TBlock"/>.
        /// </summary>
        /// <param name="proxyJsonBlock">The <see cref="ProxyJsonBlock"/> containing data for the <typeparamref name="TBlock"/>.</param>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> processing the <typeparamref name="TBlock"/>.</param>
        public abstract TBlock Create(ProxyJsonBlock proxyJsonBlock, BlockProcessor blockProcessor);

        /// <summary>
        /// Validates src and resolves file name.
        /// </summary>
        protected virtual string ValidateSrcAndResolveFileName(TBlockOptions blockOptions)
        {
            // Can't be null, empty or whitespace - https://html.spec.whatwg.org/multipage/embedded-content.html#attr-img-src
            string src = blockOptions.Src;
            if (string.IsNullOrWhiteSpace(src))
            {
                throw new OptionsException(nameof(blockOptions.Src), Strings.OptionsException_Shared_ValueMustNotBeNullWhitespaceOrAnEmptyString);
            }

            // src must be a valid absolute or relative URI
            if (!Uri.TryCreate(src, UriKind.Absolute, out Uri result) && !Uri.TryCreate(_defaultBaseUri, src, out result))
            {
                throw new OptionsException(nameof(blockOptions.Src), Strings.OptionsException_Shared_ValueMustBeAValidUri);
            }

            // Get file name and verify that URI points at a file
            string fileName;
            try
            {
                fileName = Path.GetFileName(result.LocalPath); // Doesn't touch the file system, so we don't need to wrap it in a service
            }
            catch
            {
                // According to the documentation - https://docs.microsoft.com/en-us/dotnet/api/system.io.path.getfilename?view=netstandard-2.1, 
                // Path.GetFileName throws an ArgumentException if LocalPath contains invalid characters
                throw new OptionsException(nameof(blockOptions.Src), string.Format(Strings.OptionsException_Shared_UriContainsInvalidCharacters, src));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new OptionsException(nameof(blockOptions.Src), string.Format(Strings.OptionsException_Shared_ValueMustPointToAFile, src));
            }

            return fileName;
        }

        /// <summary>
        /// Resolves local absolute path.
        /// </summary>
        protected virtual string ResolveLocalAbsolutePath(bool enableFileOperations,
            string fileName,
            TExtensionOptions extensionOptions)
        {
            if (!enableFileOperations)
            {
                return null;
            }

            // Must be absolute
            string localMediaDirectory = extensionOptions.LocalMediaDirectory;
            if (!Uri.TryCreate(localMediaDirectory, UriKind.Absolute, out Uri localMediaDirectoryUri))
            {
                throw new OptionsException(nameof(extensionOptions.LocalMediaDirectory),
                    string.Format(Strings.OptionsException_Shared_ValueMustBeAnAbsoluteUri, localMediaDirectory));
            }

            // Must have scheme FILE
            if (localMediaDirectoryUri.Scheme != Uri.UriSchemeFile)
            {
                throw new OptionsException(nameof(extensionOptions.LocalMediaDirectory),
                    string.Format(Strings.OptionsException_Shared_ValueMustBeAUriWithASupportedScheme, localMediaDirectory, localMediaDirectoryUri.Scheme, "FILE"));
            }

            // Search for file
            string[] files;
            try
            {
                // We look through entire directory to ensure there aren't multiple files with the same name, so EnumerateFiles isn't any faster.
                files = _directoryService.GetFiles(localMediaDirectoryUri.AbsolutePath, fileName, SearchOption.AllDirectories);
            }
            catch (Exception exception)
            {
                // Directory.GetFiles throws for a variety of reasons - unauthorized, invalid URI, directory not found
                throw new OptionsException(nameof(extensionOptions.LocalMediaDirectory),
                    string.Format(Strings.OptionsException_Shared_UnableToRetrieveFilesFromDirectory, localMediaDirectory),
                    exception);
            }

            int length = files.Length;

            if (length == 1)
            {
                return files[0];
            }

            if (length == 0)
            {
                throw new OptionsException(nameof(extensionOptions.LocalMediaDirectory),
                    string.Format(Strings.OptionsException_Shared_FileNotFoundInDirectory, fileName, localMediaDirectory));
            }

            // length > 1
            throw new OptionsException(nameof(extensionOptions.LocalMediaDirectory),
                string.Format(Strings.OptionsException_Shared_MultipleFilesFoundInDirectory, length, fileName, localMediaDirectory, string.Join("\n", files)));
        }
    }
}
