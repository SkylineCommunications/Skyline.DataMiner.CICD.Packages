namespace Skyline.DataMiner.CICD.Assemblers.Protocol
{
    using System;
    using System.Text.RegularExpressions;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;

    /// <summary>
    /// Helper to derive a concrete pip-compatible Python version (e.g. <c>"3.14"</c>) from a manifest.json version
    /// constraint (e.g. <c>"&gt;=3.14;&lt;3.15"</c>).
    /// </summary>
    internal static class PythonVersionConstraint
    {
        private static readonly Regex MinimumVersionPattern = new Regex(@">=\s*(?<version>\d+\.\d+(\.\d+)?)", RegexOptions.Compiled);
        private static readonly Regex UpperBoundPattern = new Regex(@"<(?<inclusive>=)?\s*(?<version>\d+\.\d+(\.\d+)?)", RegexOptions.Compiled);

        /// <summary>
        /// Extracts the minimum Python version (major.minor[.patch]) declared by a version constraint string.
        /// </summary>
        /// <param name="versionConstraint">The version constraint, e.g. <c>"&gt;=3.14;&lt;3.15"</c>.</param>
        /// <returns>The minimum version, e.g. <c>"3.14"</c>.</returns>
        /// <exception cref="InvalidManifestException">
        /// No minimum version (<c>&gt;=</c> constraint) could be extracted, or the constraint does not pin a
        /// single Python minor version (e.g. <c>"&gt;=3.10"</c> or <c>"&gt;=3.10;&lt;=3.12"</c>).
        /// </exception>
        public static string ExtractMinimumVersion(string versionConstraint)
        {
            if (String.IsNullOrWhiteSpace(versionConstraint))
            {
                throw new InvalidManifestException("The manifest.json file is missing the required 'runtime.python.version' field.");
            }

            Match match = null;
            foreach (Match candidate in MinimumVersionPattern.Matches(versionConstraint))
            {
                if (match == null || new Version(candidate.Groups["version"].Value).CompareTo(new Version(match.Groups["version"].Value)) > 0)
                {
                    match = candidate;
                }
            }

            if (match == null)
            {
                throw new InvalidManifestException(
                    $"Could not determine a concrete Python version from the constraint '{versionConstraint}'. Specify the Python version explicitly instead.");
            }

            Version minimum = new Version(match.Groups["version"].Value);

            if (!PinsToSingleMinorVersion(versionConstraint, minimum))
            {
                throw new InvalidManifestException(
                    $"The constraint '{versionConstraint}' does not pin a single Python minor version: its upper bound ('<' or '<=') " +
                    "either is missing or still spans more than one minor version. Dependencies are resolved for one specific Python " +
                    $"version, so narrow the constraint to a single minor version (e.g. \">={match.Groups["version"].Value};<{minimum.Major}.{minimum.Minor + 1}\").");
            }

            return match.Groups["version"].Value;
        }

        /// <summary>
        /// Checks whether an upper bound clause narrows the range to <paramref name="minimum"/>'s minor version:
        /// either a patch-level bound in that same minor (e.g. <c>"&lt;=3.10.9"</c>), or an exclusive bound on the
        /// next minor (e.g. <c>"&lt;3.11"</c>). Clauses are ANDed together, so one matching clause is sufficient.
        /// </summary>
        private static bool PinsToSingleMinorVersion(string versionConstraint, Version minimum)
        {
            foreach (Match upperBound in UpperBoundPattern.Matches(versionConstraint))
            {
                Version upper = new Version(upperBound.Groups["version"].Value);
                bool inclusive = upperBound.Groups["inclusive"].Success;

                // Same minor version, only trimming the allowed patch range (e.g. ">=3.10;<=3.10.9").
                if (upper.Major == minimum.Major && upper.Minor == minimum.Minor)
                {
                    return true;
                }

                // Exclusive bound on the next minor with no patch (e.g. ">=3.10;<3.11") excludes all of 3.11+.
                if (!inclusive && upper.Major == minimum.Major && upper.Minor == minimum.Minor + 1 && upper.Build <= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
