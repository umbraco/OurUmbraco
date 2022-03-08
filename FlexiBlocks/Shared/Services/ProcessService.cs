using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// The default implementation of <see cref="IProcessService"/>.
    /// </summary>
    public class ProcessService : IProcessService
    {
        // TODO we can't test this without, ProcessWrapperFactory and ProcessWrapper types.
        /// <inheritdoc />
        public string Run(string executable, string arguments, int timeoutMS)
        {
            // Under the hood, Process uses SafeHandles for unmanaged resources, so we don't need to worry
            // about cleaning up if Dispose isn't called (e.g if an exception is thrown and never caught).
            int exitCode;
            string standardOutput = null;
            string errorOutput = null;
            Process process = null;

            try
            {
                process = new Process();
                ProcessStartInfo processStartInfo = process.StartInfo;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.FileName = executable;
                processStartInfo.Arguments = arguments;
                process.ErrorDataReceived += new DataReceivedEventHandler((_, dataReceivedEventArgs) => errorOutput += dataReceivedEventArgs.Data);

                process.Start();

                // To avoid deadlocks - https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.redirectstandardoutput?view=netstandard-2.0
                // - Call StandardOutput.ReadToEnd before WaitForExit
                // - Read error stream asynchronously
                process.BeginErrorReadLine();
                standardOutput = process.StandardOutput.ReadToEnd();
                process.WaitForExit(timeoutMS);
                exitCode = process.ExitCode;
            }
            // Win32Exception is thrown when the executable file can't be opened. In such cases we need to provide messages specific to the executable with 
            // instructions on how to aquire and install the it, so we let Win32Exceptions bubble.
            catch (Exception exception) when (!(exception is Win32Exception))
            {
                // Failed without exiting, we can provide extra context here, so wrap
                throw new InvalidOperationException(string.Format(Strings.InvalidOperationException_ProcessService_ExceptionThrownWhileAttemptingToRunExecutable, executable, arguments, timeoutMS, standardOutput), exception);
            }
            finally
            {
                process?.Dispose();
            }

            if (exitCode == 0)
            {
                return standardOutput;
            }

            throw new InvalidOperationException(string.Format(Strings.InvalidOperationException_ProcessService_ExecutableRunFailed, executable, arguments, exitCode, standardOutput, errorOutput));
        }
    }
}
