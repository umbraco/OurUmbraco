using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiVideoBlocks
{
    /// <summary>
    /// An abstraction for interacting with videos.
    /// </summary>
    public interface IVideoService
    {
        /// <summary>
        /// Gets the width, height and duration of a video.
        /// </summary>
        /// <param name="path">The video's file path.</param>
        /// <exception cref="InvalidOperationException">Thrown if FFmpeg is not available.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an exception is thrown while attempting to run FFmpeg.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the FFmpeg run fails (exit code > 0).</exception>
        /// <exception cref="InvalidOperationException">Thrown if the retrieved metadata is invalid.</exception>
        (double width, double height, double duration) GetVideoDimensionsAndDuration(string path);

        /// <summary>
        /// Generates a poster from the first frame of a video.
        /// </summary>
        /// <param name="videoPath">The video's file path.</param>
        /// <param name="posterPath">The generated poster's file path.</param>
        /// <exception cref="InvalidOperationException">Thrown if FFmpeg is not available.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an exception is thrown while attempting to run FFmpeg.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the FFmpeg run fails (exit code > 0).</exception>
        void GeneratePoster(string videoPath, string posterPath);
    }
}
