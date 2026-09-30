namespace Skyline.DataMiner.CICD.DMProtocol
{
    using System;
    using System.Text.RegularExpressions;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;

    /// <summary>
    /// Helper to derive a concrete pip-compatible Python version (e.g. <c>"3.14"</c>) from the version constraint string
    /// declared in a scripted connector's <c>manifest.json</c> (e.g. <c>"&gt;=3.14;&lt;3.15"</c>).
    /// </summary>
    internal static class PythonVersionConstraint
    {
        private static readonly Regex MinimumVersionPattern = new Regex(@">=\s*(?<version>\d+\.\d+(\.\d+)?)", RegexOptions.Compiled);

        /// <summary>
        /// Extracts the minimum Python version (major.minor[.patch]) declared by a version constraint string.
        /// </summary>
        /// <param name="versionConstraint">The version constraint, e.g. <c>"&gt;=3.14;&lt;3.15"</c>.</param>
        /// <returns>The minimum version, e.g. <c>"3.14"</c>.</returns>
        /// <exception cref="InvalidManifestException">
        /// No minimum version (<c>&gt;=</c> constraint) could be extracted from <paramref name="versionConstraint"/>.
        /// </exception>
        public static string ExtractMinimumVersion(string versionConstraint)
        {
            if (String.IsNullOrWhiteSpace(versionConstraint))
            {
                throw new InvalidManifestException("The manifest.json file is missing the required 'runtime.python.version' field.");
            }

            Match match = MinimumVersionPattern.Match(versionConstraint);

            if (!match.Success)
            {
                throw new InvalidManifestException(
                    $"Could not determine a concrete Python version from the constraint '{versionConstraint}'. Specify the Python version explicitly instead.");
            }

            return match.Groups["version"].Value;
        }
    }
}
