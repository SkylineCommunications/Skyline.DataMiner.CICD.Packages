namespace Skyline.DataMiner.CICD.DMApp.Common
{
    using System;
    using System.Diagnostics;
    using System.Text;

    internal class Dotnet : IDotnet
    {
        private readonly DataReceivedEventHandler onOutput;
        private readonly DataReceivedEventHandler onError;

        public Dotnet(DataReceivedEventHandler onOutput, DataReceivedEventHandler onError)
        {
            this.onOutput = onOutput;
            this.onError = onError;
        }

        public bool Run(string command, bool ignoreOutput)
        {
            bool success = ignoreOutput
                ? Run(command, null, null)
                : Run(command).succes;

            return success;
        }

        public (bool succes, string output, string errors) Run(string command)
        {
            StringBuilder totalOutput = new StringBuilder();
            StringBuilder totalErrors = new StringBuilder();

            bool success = Run(command, OnOutputWrapped, OnErrorWrapped);

            return (success, totalOutput.ToString(), totalErrors.ToString());

            void OnOutputWrapped(object sender, DataReceivedEventArgs e)
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    totalOutput.AppendLine(e.Data);
                    onOutput(sender, e);
                }
            }

            void OnErrorWrapped(object sender, DataReceivedEventArgs e)
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    totalErrors.AppendLine(e.Data);
                    onError(sender, e);
                }
            }
        }

        private static bool Run(string command, DataReceivedEventHandler overrideOnOutput, DataReceivedEventHandler overrideOnError)
        {
            bool useOutput = overrideOnOutput != null;
            bool useError = overrideOnError != null;

            const string pathTo = "dotnet";
            ProcessStartInfo details = new ProcessStartInfo(pathTo, command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = useError,
                RedirectStandardOutput = useOutput
            };

            bool success;
            using (var process = new Process())
            {
                process.StartInfo = details;

                if (useError)
                {
                    process.ErrorDataReceived += overrideOnError;
                }

                if (useOutput)
                {
                    process.OutputDataReceived += overrideOnOutput;
                }

                if (process.Start())
                {
                    if (useOutput)
                    {
                        process.BeginOutputReadLine();
                    }

                    if (useError)
                    {
                        process.BeginErrorReadLine();
                    }

                    process.WaitForExit();
                    success = process.ExitCode == 0;
                }
                else
                {
                    success = false;
                }
            }

            return success;
        }
    }
}