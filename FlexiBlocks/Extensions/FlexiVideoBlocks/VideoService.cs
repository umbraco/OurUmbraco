using Markdig.Helpers;
using System;
using System.ComponentModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiVideoBlocks
{
    /// <summary>
    /// The default implementation of <see cref="IVideoService"/>.
    /// </summary>
    public class VideoService : IVideoService
    {
        internal const string GET_METADATA_ARGUMENTS_FORMAT = "-v error -of default=noprint_wrappers=1:nokey=1 -show_entries format=duration:stream=width,height \"{0}\"";
        internal const string GENERATE_POSTER_ARGUMENTS_FORMAT = "-v error -ss 0 -i \"{0}\" -frames:v 1 -y -f image2 \"{1}\"";

        private readonly IProcessService _processService;

        /// <summary>
        /// Creates a <see cref="VideoService"/>.
        /// </summary>
        /// <param name="processService">The service that handles FFmpeg processes.</param>
        public VideoService(IProcessService processService)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
        }

        /// <inheritdoc />
        public (double width, double height, double duration) GetVideoDimensionsAndDuration(string path)
        {
            string result = RunFfmpeg("ffprobe", string.Format(GET_METADATA_ARGUMENTS_FORMAT, path), 1000);

            // TODO can avoid allocations in netstandard 2.1 by using Parse(ReadOnlySpan<char>...) - https://docs.microsoft.com/en-us/dotnet/api/system.int32.parse?view=netstandard-2.1#System_Int32_Parse_System_ReadOnlySpan_System_Char__System_Globalization_NumberStyles_System_IFormatProvider_
            var lineReader = new LineReader(result);

            try
            {
                return (double.Parse(lineReader.ReadLine().ToString()),
                    double.Parse(lineReader.ReadLine().ToString()),
                    double.Parse(lineReader.ReadLine().ToString()));
            }
            catch
            {
                // If video has corrupt metadata an ArgumentNullException, FormatException or OverflowException may be thrown. Rethrow with proper context.
                throw new InvalidOperationException(string.Format(Strings.InvalidOperationException_VideoService_InvalidVideoMetadata, path, result));
            }
        }

        /// <inheritdoc />
        public void GeneratePoster(string videoPath, string posterPath)
        {
            // TODO whats the perf of this like? if its slow we should add a guid to both video file and poster files, and only regenerate if
            // new video file with no guid pops up
            RunFfmpeg("ffmpeg", string.Format(GENERATE_POSTER_ARGUMENTS_FORMAT, videoPath, posterPath), 1000);
        }

        /// <summary>
        /// Runs FFmpeg.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if FFmpeg is not available.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an exception is thrown while attempting to run FFmpeg.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the FFmpeg run fails (exit code > 0).</exception>
        internal virtual string RunFfmpeg(string executable, string arguments, int timeoutMS)
        {
            try
            {
                return _processService.Run(executable, arguments, timeoutMS);
            }
            catch (Win32Exception win32Exception)
            {
                // Good chance of this issue occuring and we can provide extra context, so wrap
                throw new InvalidOperationException(Strings.InvalidOperationException_VideoService_FfmpegRequired, win32Exception);
            }
        }
    }
}
