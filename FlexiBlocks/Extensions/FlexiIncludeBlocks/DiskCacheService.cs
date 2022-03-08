using Jering.IocServices.System.IO;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IDiskCacheService"/>.</para>
    /// <para>This is an extremely simple caching service, it lacks policies (expiration etc), size limits and other bells and whistles. Additional features 
    /// will be added as the need for them arises.</para>
    /// </summary>
    public class DiskCacheService : IDiskCacheService
    {
        private static readonly Dictionary<int, char> _decimalToHex = new Dictionary<int, char>
        {
            {0, '0' }, {1, '1' }, {2, '2' }, {3, '3' }, {4, '4' }, {5, '5' }, {6, '6' }, {7, '7' },
            {8, '8' }, {9, '9' }, { 10, 'A' }, {11, 'B'}, {12, 'C'}, {13, 'D'}, {14, 'E'}, {15, 'F'}
        };

        private readonly MD5 _mD5 = MD5.Create();
        private readonly ILogger<DiskCacheService> _logger;
        private readonly IFileService _fileService;
        private readonly IDirectoryService _directoryService;
        private readonly ArrayPool<byte> _defaultByteArrayPool;
        private readonly bool _warningLoggingEnabled;

        /// <summary>
        /// Creates a <see cref="DiskCacheService"/>.
        /// </summary>
        /// <param name="fileService">The service that will handle file operations.</param>
        /// <param name="directoryService">The service that will handle directory operations.</param>
        /// <param name="loggerFactory">The factory for <see cref="ILogger"/>s.</param>
        public DiskCacheService(IFileService fileService, IDirectoryService directoryService, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<DiskCacheService>();
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _directoryService = directoryService ?? throw new ArgumentNullException(nameof(directoryService));
            _defaultByteArrayPool = ArrayPool<byte>.Shared;
            _warningLoggingEnabled = _logger.IsEnabled(LogLevel.Warning);
        }

        /// <inheritdoc />
        public FileStream TryGetCacheFile(string source, string cacheDirectory)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException(string.Format(Strings.ArgumentException_Shared_ValueCannotBeNullWhitespaceOrAnEmptyString, nameof(source)));
            }

            if (string.IsNullOrWhiteSpace(cacheDirectory))
            {
                throw new ArgumentException(string.Format(Strings.ArgumentException_Shared_ValueCannotBeNullWhitespaceOrAnEmptyString, nameof(cacheDirectory)));
            }

            string filePath = CreatePath(source, cacheDirectory);

            if (!_fileService.Exists(filePath))
            {
                return null;
            }

            try
            {
                return GetStream(filePath,
                        FileMode.Open, // Throw if file was removed between _fileService.Exists and this call
                        FileAccess.Read, // Read only access
                        FileShare.Read); // Don't allow other threads to write to the file while we read from it
            }
            catch (Exception exception) when (exception is DirectoryNotFoundException || exception is FileNotFoundException)
            {
                // File does not exist (something happened to it between the _fileService.Exists call and the _fileService.Open call in GetStream)
                return null;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(string.Format(Strings.InvalidOperationException_DiskCacheService_UnexpectedDiskCacheException, source, filePath), exception);
            }
        }

        /// <inheritdoc />
        public FileStream CreateOrGetCacheFile(string source, string cacheDirectory)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException(string.Format(Strings.ArgumentException_Shared_ValueCannotBeNullWhitespaceOrAnEmptyString, nameof(source)));
            }

            if (string.IsNullOrWhiteSpace(cacheDirectory))
            {
                throw new ArgumentException(string.Format(Strings.ArgumentException_Shared_ValueCannotBeNullWhitespaceOrAnEmptyString, nameof(cacheDirectory)));
            }

            // Ensure that cache directory string is valid and that the directory exists
            try
            {
                _directoryService.CreateDirectory(cacheDirectory);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(string.Format(Strings.InvalidOperationException_DiskCacheService_InvalidDiskCacheDirectory, cacheDirectory), exception);
            }

            string filePath = CreatePath(source, cacheDirectory);

            try
            {
                return GetStream(filePath,
                    FileMode.OpenOrCreate, // Create file if it doesn't already exist
                    FileAccess.ReadWrite, // Read and write access
                    FileShare.None); // Don't allow other threads to access the file while we write to it
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(string.Format(Strings.InvalidOperationException_DiskCacheService_UnexpectedDiskCacheException, source, filePath), exception);
            }
        }

        // Open a file stream, retrying up to 3 times if the file is in use.
        internal virtual FileStream GetStream(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            int remainingTries = 3;

            while (true)
            {
                remainingTries--;

                try
                {
                    return _fileService.Open(path, fileMode, fileAccess, fileShare);
                }
                catch (IOException)
                {
                    if (_warningLoggingEnabled)
                    {
                        _logger.LogWarning(string.Format(Strings.LogWarning_DiskCacheService_FileInUse, path, remainingTries));
                    }

                    if (remainingTries == 0)
                    {
                        throw;
                    }

                    Thread.Sleep(50);
                }

                // Other exceptions (FileNotFoundException, DirectoryNotFoundException, PathTooLongException, UnauthorizedAccessException and ArgumentException) 
                // will almost certainly continue to be thrown however many times we retry, so just let them propagate.
            }
        }

        internal virtual string CreatePath(string sourceUri, string cacheDirectory)
        {
            return Path.Combine(cacheDirectory, $"{GetCacheIdentifier(sourceUri)}.txt");
        }

        // Hashes a URI to create a unique cache identifier that does not contain illegal characters and has a fixed length.
        internal virtual string GetCacheIdentifier(string sourceUri)
        {
            int byteCount = Encoding.UTF8.GetByteCount(sourceUri);
            byte[] bytes = null;
            try
            {
                bytes = _defaultByteArrayPool.Rent(byteCount);
                Encoding.UTF8.GetBytes(sourceUri, 0, sourceUri.Length, bytes, 0);
                byte[] hashBytes = _mD5.ComputeHash(bytes, 0, byteCount);

                var hex = new StringBuilder();
                foreach (byte hashByte in hashBytes)
                {
                    hex.Append(_decimalToHex[(hashByte / 16) % 16]);
                    hex.Append(_decimalToHex[hashByte % 16]);
                }

                return hex.ToString();
            }
            finally
            {
                if (bytes != null)
                {
                    _defaultByteArrayPool.Return(bytes);
                }
            }
        }
    }
}
