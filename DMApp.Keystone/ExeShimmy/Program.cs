namespace ExeShim
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    class Program
    {
        static void Main(string[] args)
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pathTo = Path.Combine(assemblyFolder, "$ProgramNameShimmy$", "$ProgramName$.exe"); // Specify the path to the executable
            string arguments = string.Join(" ", args.Select(arg => $"\"{arg}\"")); // Prepare the arguments
            ProcessStartInfo details = new ProcessStartInfo(pathTo)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = arguments,
            };

            using (var process = new Process())
            {
                process.StartInfo = details;

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine(e.Data); // Write the output data to the console
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.Error.WriteLine(e.Data); // Write the error data to the console
                    }
                };

                if (!process.Start())
                {
                    Console.WriteLine("ShimExe: Failed to start user application $ProgramName$!");
                }
                else
                {
                    // Start reading from the streams
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit(); // Wait for the process to exit
                }
            }
        }
    }
}
