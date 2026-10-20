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
        private static readonly Regex SupportedClausePattern = new Regex(@"^\s*(>=|<=?)\s*\d+\.\d+(\.\d+)?\s*$", RegexOptions.Compiled);

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

            foreach (string clause in versionConstraint.Split(';'))
            {
                if (String.IsNullOrWhiteSpace(clause))
                {
                    continue;
                }

                if (!SupportedClausePattern.IsMatch(clause))
                {
                    throw new InvalidManifestException(
                        $"The constraint '{versionConstraint}' contains an unsupported clause ('{clause.Trim()}'). " +
                        "Only '>=', '<' and '<=' clauses against a major.minor[.patch] version are supported.");
                }
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
        /// Checks whether <paramref name="versionConstraint"/>'s upper bound(s) are satisfiable for
        /// <paramref name="minimum"/> and pin the range to a single Python minor version. A clause that would
        /// exclude <paramref name="minimum"/> itself (e.g. <c>"&gt;=3.14;&lt;3.14"</c>, an empty range) always
        /// fails validation, even if another clause in the same constraint looks valid on its own.
        /// </summary>
        private static bool PinsToSingleMinorVersion(string versionConstraint, Version minimum)
        {
            Version normalizedMinimum = Normalize(minimum);
            bool pinsSingleMinor = false;

            foreach (Match upperBound in UpperBoundPattern.Matches(versionConstraint))
            {
                Version upper = new Version(upperBound.Groups["version"].Value);
                bool inclusive = upperBound.Groups["inclusive"].Success;
                Version normalizedUpper = Normalize(upper);

                int comparison = normalizedUpper.CompareTo(normalizedMinimum);
                bool excludesMinimum = inclusive ? comparison < 0 : comparison <= 0;

                if (excludesMinimum)
                {
                    // This clause contradicts the lower bound, making the overall range empty.
                    return false;
                }

                // Same minor version, only trimming the allowed patch range (e.g. ">=3.10;<=3.10.9").
                bool sameMinor = upper.Major == minimum.Major && upper.Minor == minimum.Minor;

                // Exclusive bound on the next minor with no patch (e.g. ">=3.10;<3.11") excludes all of 3.11+.
                bool nextMinorExclusive = !inclusive && upper.Major == minimum.Major && upper.Minor == minimum.Minor + 1 && upper.Build <= 0;

                if (sameMinor || nextMinorExclusive)
                {
                    pinsSingleMinor = true;
                }
            }

            return pinsSingleMinor;
        }

        /// <summary>
        /// Replaces unset (<c>-1</c>) version components with <c>0</c> so versions with a different number of
        /// components (e.g. <c>"3.14"</c> vs. <c>"3.14.0"</c>) compare equal.
        /// </summary>
        private static Version Normalize(Version version)
        {
            return new Version(version.Major, version.Minor, Math.Max(version.Build, 0));
        }
    }
}
