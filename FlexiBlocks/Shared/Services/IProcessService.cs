using System;
using System.ComponentModel;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction for managing processes.
    /// </summary>
    public interface IProcessService
    {
        /// <summary>
        /// Runs an executable.
        /// </summary>
        /// <param name="executable">The executable to run.</param>
        /// <param name="arguments">The arguments for the executable.</param>
        /// <param name="timeoutMS">The period of time to wait before timing out.</param>
        /// <exception cref="Win32Exception">Thrown if the executable file cannot be opened.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an exception is thrown while attempting to run the executable.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the run failed (exit code > 0).</exception>
        string Run(string executable, string arguments, int timeoutMS);
    }
}
