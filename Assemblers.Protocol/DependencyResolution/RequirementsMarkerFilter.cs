namespace Skyline.DataMiner.CICD.Assemblers.Protocol.DependencyResolution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;
    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Pre-filters a <c>requirements.txt</c> file's PEP 508 environment markers (e.g. <c>sys_platform == "win32"</c>)
    /// against a synthetic environment for a specific target platform, stripping the marker from kept requirements
    /// so pip never re-evaluates (and potentially mis-evaluates) it against the host it runs on.
    /// </summary>
    internal static class RequirementsMarkerFilter
    {
        /// <summary>
        /// Creates a filtered copy of <paramref name="requirementsFilePath"/> under <paramref name="destinationDirectory"/>,
        /// keeping only the requirements whose environment marker (if any) evaluates to <see langword="true"/> for
        /// <paramref name="targetPlatform"/> and <paramref name="pythonVersion"/>. Kept requirements have their
        /// marker stripped, so pip does not re-evaluate it against the host environment.
        /// </summary>
        /// <param name="fileSystem">The file system abstraction to use.</param>
        /// <param name="requirementsFilePath">Path to the original requirements file.</param>
        /// <param name="pythonVersion">Target Python version (pip format, e.g. "3.14").</param>
        /// <param name="targetPlatform">The target platform to filter markers for.</param>
        /// <param name="destinationDirectory">Directory in which the filtered requirements file is created.</param>
        /// <returns>The full path to the filtered requirements file.</returns>
        public static string CreateFilteredRequirementsFile(IFileSystem fileSystem, string requirementsFilePath, string pythonVersion, SupportedPlatform targetPlatform, string destinationDirectory)
        {
            IReadOnlyDictionary<string, string> environment = BuildEnvironment(pythonVersion, targetPlatform);

            string content = fileSystem.File.ReadAllText(requirementsFilePath);
            string[] lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            var filteredLines = new List<string>();

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                int markerSeparatorIndex = FindMarkerSeparatorIndex(line);

                if (markerSeparatorIndex < 0)
                {
                    filteredLines.Add(line);
                    continue;
                }

                string requirementPart = line.Substring(0, markerSeparatorIndex).TrimEnd();
                string markerPart = line.Substring(markerSeparatorIndex + 1).Trim();

                if (MarkerEvaluator.Evaluate(markerPart, environment))
                {
                    filteredLines.Add(requirementPart);
                }
            }

            fileSystem.Directory.CreateDirectory(destinationDirectory);
            string filteredFilePath = fileSystem.Path.Combine(destinationDirectory, $"requirements.{targetPlatform.ToTargetTriple()}.txt");
            fileSystem.File.WriteAllText(filteredFilePath, String.Join(Environment.NewLine, filteredLines));

            return filteredFilePath;
        }

        /// <summary>
        /// Finds the index of the <c>;</c> that separates a requirement specifier from its environment marker,
        /// ignoring any <c>;</c> that appears inside a quoted string (e.g. an extra's own marker literal).
        /// </summary>
        private static int FindMarkerSeparatorIndex(string line)
        {
            char? openQuote = null;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (openQuote != null)
                {
                    if (c == openQuote.Value)
                    {
                        openQuote = null;
                    }

                    continue;
                }

                if (c == '\'' || c == '"')
                {
                    openQuote = c;
                    continue;
                }

                if (c == ';')
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Builds a synthetic PEP 508 marker environment for the given target platform and Python version.
        /// </summary>
        private static IReadOnlyDictionary<string, string> BuildEnvironment(string pythonVersion, SupportedPlatform targetPlatform)
        {
            string majorMinorVersion = String.Join(".", pythonVersion.Split('.').Take(2));

            var environment = new Dictionary<string, string>
            {
                ["python_version"] = majorMinorVersion,
                ["python_full_version"] = pythonVersion,
                ["implementation_name"] = "cpython",
                ["implementation_version"] = pythonVersion,
                ["platform_python_implementation"] = "CPython",
                ["platform_release"] = String.Empty,
                ["platform_version"] = String.Empty,
                ["extra"] = String.Empty,
            };

            switch (targetPlatform)
            {
                case SupportedPlatform.Windows:
                    environment["os_name"] = "nt";
                    environment["sys_platform"] = "win32";
                    environment["platform_system"] = "Windows";
                    environment["platform_machine"] = "AMD64";
                    break;

                case SupportedPlatform.Linux:
                    environment["os_name"] = "posix";
                    environment["sys_platform"] = "linux";
                    environment["platform_system"] = "Linux";
                    environment["platform_machine"] = "x86_64";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(targetPlatform), targetPlatform, "Unknown supported platform.");
            }

            return environment;
        }
    }
}
