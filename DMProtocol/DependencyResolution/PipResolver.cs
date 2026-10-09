namespace Skyline.DataMiner.CICD.DMProtocol.DependencyResolution
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    using Skyline.DataMiner.CICD.DMProtocol.DependencyResolution.Exceptions;

    /// <summary>
    /// Utility class to resolve the pip invocation to use for dependency resolution and wheel downloading.
    /// </summary>
    internal static class PipResolver
    {
        /// <summary>
        /// The candidates that are tried, in order of preference, to find a working pip invocation.
        /// </summary>
        private static readonly PipCommand[] Candidates =
        {
            new PipCommand("python", new[] { "-m", "pip" }),
            new PipCommand("python3", new[] { "-m", "pip" }),
            new PipCommand("pip3", Array.Empty<string>()),
            new PipCommand("pip", Array.Empty<string>()),
        };

        /// <summary>
        /// Resolves the pip invocation to use.
        /// </summary>
        /// <returns>The <see cref="PipCommand"/> to use to invoke pip.</returns>
        /// <exception cref="PipNotFoundException">No valid pip executable was found.</exception>
        public static PipCommand ResolvePip()
        {
            foreach (var candidate in Candidates)
            {
                if (IsAvailable(candidate))
                {
                    return candidate;
                }
            }

            throw new PipNotFoundException();
        }

        /// <summary>
        /// Checks whether the specified pip command is available on the current system by invoking it with <c>--version</c>.
        /// </summary>
        /// <param name="command">The pip command to check.</param>
        /// <returns><see langword="true"/> if the command is available; otherwise, <see langword="false"/>.</returns>
        private static bool IsAvailable(PipCommand command)
        {
            try
            {
                var startInfo = command.CreateProcessStartInfo(new[] { "--version" });

                using var process = Process.Start(startInfo);
                if (process is null)
                {
                    return false;
                }

                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch (Win32Exception)
            {
                // Thrown when the executable could not be found.
                return false;
            }
        }
    }
}
