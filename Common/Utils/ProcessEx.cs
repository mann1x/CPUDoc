using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPUDoc
{
    public static class ProcessEx
    {
        /// <summary>
        /// The article/forum: http://alabaxblog.info/2013/06/redirectstandardoutput-beginoutputreadline-pattern-broken/
        /// - issues with BeginOutputReadLine and EOF (possibility to loose EOF)
        /// - issue with OutputDataReceived (hevy load)
        /// - FileStream (CopyAsynchTo & WriteAsynch) does support cancellation only at beginning...
        /// https://gist.github.com/hidegh/07d5588702e2b56a3fc2a3d73848d9f3
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="arguments"></param>
        /// <param name="timeout"></param>
        /// <param name="priority"></param>
        /// <param name="standardInput"></param>
        /// <param name="standardOutput"></param>
        /// <param name="standardError"></param>
        /// <returns></returns>

        public static int ExecuteProcess(
            string fileName,
            string arguments,
            int timeout,
            ProcessPriorityClass priority,
            Stream standardInput,
            Stream standardOutput,
            out string standardError)
        {
            int exitCode;

            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = arguments;

                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.Start();
                process.PriorityClass = priority;

                //
                // Write input stream...then close it!
                Object writerThreadLock = new object();
                Thread writerThread = null;

                using (Task inputWriter = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        // Mark as running...
                        lock (writerThreadLock)
                            writerThread = Thread.CurrentThread;

                        // NOTE: Closing the input (process.StandardInput.BaseStream) after write op. is done (or aborted) is important!
                        using (var processInputStream = process.StandardInput.BaseStream)
                            standardInput.CopyTo(processInputStream);

                        // Mark as finished
                        lock (writerThreadLock)
                            writerThread = null;
                    }
                    catch (ThreadAbortException)
                    {
                        // NOTE: to be able to "abort" the copy process and quit without additional exception we need to reset!
                        //Thread.ResetAbort();
                    }
                }))

                //
                // Read output stream and error string (both async)...
                using (Task<bool> processWaiter = Task.Factory.StartNew(() => process.WaitForExit(timeout)))
                using (Task outputReader = Task.Factory.StartNew(() => { process.StandardOutput.BaseStream.CopyTo(standardOutput); }))
                using (Task<string> errorReader = Task.Factory.StartNew(() => process.StandardError.ReadToEnd()))
                {
                    // Check result (whether process finished) from processWaiter...
                    bool processFinished = processWaiter.Result;

                    // If we got timeout, we need to kill the process...
                    if (!processFinished)
                    {
                        /*
                        lock (writerThreadLock)
                        {
                            // NOTE: a non-null waitherThread signalizes that a inputWriter thread is still running (which we need to abort)...
                            writerThread?.Abort();
                        }
                        */

                        process.Kill();
                    }

                    // NOTE: even after calling process kill (asynchronously) - not just on success - , make sure we wait for the process to finish.
                    // See: https://msdn.microsoft.com/en-us/library/system.diagnostics.process.kill.aspx
                    process.WaitForExit();

                    // Make sure everything is read from the streams...
                    Task.WaitAll(outputReader, errorReader, inputWriter);

                    // Timeout-ed process has to raise an exception...
                    if (!processFinished)
                        throw new TimeoutException("Process wait timeout expired");

                    // Success...
                    standardError = errorReader.Result;

                    exitCode = process.ExitCode;
                }
            }

            return exitCode;
        }

    }
}