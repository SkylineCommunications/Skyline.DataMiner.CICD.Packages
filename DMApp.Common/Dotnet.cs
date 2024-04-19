namespace Skyline.DataMiner.CICD.DMApp.Common
{
    using System;
    using System.Diagnostics;

    internal class Dotnet : IDotnet
    {
        public bool Run(string command, out string output, out string errors)
        {
            string pathTo = "dotnet";
            ProcessStartInfo details = new ProcessStartInfo(pathTo, command);

            details.CreateNoWindow = true;
            details.UseShellExecute = false;
            details.RedirectStandardError = true;
            details.RedirectStandardOutput = true;

            bool success = false;

            using (var process = new Process())
            {
                process.StartInfo = details;
                if (process.Start())
                {
                    process.WaitForExit();
                    success = process.ExitCode == 0;
                    output = process.StandardOutput.ReadToEnd();
                    errors = process.StandardError.ReadToEnd();
                }
                else
                {
                    output = String.Empty;
                    errors = "Process Failed to Start.";
                }

                process.StandardError.Close();
                process.StandardOutput.Close();
            }

            return success;
        }
    }
}