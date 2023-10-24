namespace Skyline.DataMiner.CICD.DMApp.Common
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a version of an app package.
    /// </summary>
    public class DMAppVersion
    {
        // Exactly X.X.X.X and X needs to be numeric
        private static readonly Regex ProtocolRegex = new Regex(@"^\d+\.\d+\.\d+\.\d+$", RegexOptions.Compiled, TimeSpan.FromMinutes(1));

        // Starts with X.X.X and X needs to be numeric
        private static readonly Regex DataMinerRegex = new Regex(@"^\d+\.\d+\.\d+", RegexOptions.Compiled, TimeSpan.FromMinutes(1));

        // Exactly X.X.X and X needs to be numeric
        private static readonly Regex DataMinerShortRegex = new Regex(@"^\d+\.\d+\.\d+$", RegexOptions.Compiled, TimeSpan.FromMinutes(1));

        // Exactly X-CUX and X needs to be numeric
        private static readonly Regex CURegex = new Regex(@"^\d+-CU\d+$", RegexOptions.Compiled, TimeSpan.FromMinutes(1));

        private readonly string _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="DMAppVersion"/> class.
        /// </summary>
        /// <param name="version">The version string.</param>
        private DMAppVersion(string version)
        {
            _version = version;
        }

        /// <summary>
        /// Generates a 16-bit number based on the input string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>16-bit number.</returns>
        private static UInt16 CreateUniqueID(string input)
        {
            using (var md5Hasher = MD5.Create())
            {
                var data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToUInt16(data, 0);
            }
        }

        /// <summary>
        /// Converts a string representation of a version that follows the protocol versioning scheme (A.B.C.D) to an app package version.
        /// </summary>
        /// <param name="protocolVersion">The version.</param>
        /// <returns>The corresponding <see cref="DMAppVersion"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="protocolVersion"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="protocolVersion"/> is empty or whitespace.</exception>
        /// <exception cref="FormatException"><paramref name="protocolVersion"/> has an invalid format.</exception>
        /// <remarks>The expected format for <paramref name="protocolVersion"/> is a sequence of four numbers separated by a dot (e.g. "10.2.1.4").</remarks>
        public static DMAppVersion FromProtocolVersion(string protocolVersion)
        {
            if (protocolVersion == null)
            {
                throw new ArgumentNullException(nameof(protocolVersion));
            }

            if (String.IsNullOrWhiteSpace(protocolVersion))
            {
                throw new ArgumentException("The specified version can not be empty or whitespace", nameof(protocolVersion));
            }

            string[] versionParts = protocolVersion.Split('.');

            if (!ProtocolRegex.IsMatch(protocolVersion))
            {
                throw new FormatException("The specified version has an invalid format.");
            }

            string packageVersion = versionParts[0] + "." + versionParts[1] + "." + versionParts[2] + "-CU" + versionParts[3];

            return new DMAppVersion(packageVersion);
        }

        /// <summary>
        /// Converts a string representation of a version that follows a pre-release scheme A.B.C-abc or A.B.C.D-abc
        /// </summary>
        /// <param name="prereleaseVersion">The version.</param>
        /// <returns>The corresponding <see cref="DMAppVersion"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="prereleaseVersion"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="prereleaseVersion"/> is empty or whitespace.</exception>
        public static DMAppVersion FromPreRelease(string prereleaseVersion)
        {
            if (prereleaseVersion == null)
            {
                throw new ArgumentNullException(nameof(prereleaseVersion));
            }

            if (String.IsNullOrWhiteSpace(prereleaseVersion))
            {
                throw new ArgumentException("The specified version can not be empty or whitespace", nameof(prereleaseVersion));
            }

            string[] prereleaseParts = prereleaseVersion.Split('-');

            if (prereleaseParts.Length != 2)
            {
                throw new ArgumentException($"prereleaseVersion:{prereleaseVersion} does not follow a pre-release scheme A.B.C-abc or A.B.C.D-abc", nameof(prereleaseVersion));
            }

            var uniqueNumberForPreRel = CreateUniqueID(prereleaseParts[1]);

            // We now have either 4 or 5 numbers that I need to change into 3 numbers for pre-releases.
            // A.B.C
            // A = combination of all 3 or 4 initial numbers
            // B = 0
            // C = uniqueID from pre-release

            // e.g. 1.0.1-beta1   turns into  101.0.53790

            ushort allowedNumber;

            // make sure created number is not too big
            unchecked
            {
                allowedNumber = UInt16.Parse(prereleaseParts[0].Replace(".", ""));
            }

            string packageVersion = allowedNumber + "." + 0 + "." + uniqueNumberForPreRel;

            return new DMAppVersion(packageVersion);
        }

        /// <summary>
        /// Converts a string representation of a version that follows the DataMiner versioning scheme (A.B.C or A.B.C-CUx) to an app package version.
        /// </summary>
        /// <param name="dmaVersion">The version.</param>
        /// <returns>The corresponding <see cref="DMAppVersion"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dmaVersion"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="dmaVersion"/> is empty or whitespace.</exception>
        /// <exception cref="FormatException"><paramref name="dmaVersion"/> has an invalid format.</exception>
        /// <remarks>The expected format for <paramref name="dmaVersion"/> is a sequence of three numbers separated by a dot (e.g. "10.2.1").</remarks>
        public static DMAppVersion FromDataMinerVersion(string dmaVersion)
        {
            if (dmaVersion == null)
            {
                throw new ArgumentNullException(nameof(dmaVersion));
            }

            if (String.IsNullOrWhiteSpace(dmaVersion))
            {
                throw new ArgumentException("The specified version can not be empty or whitespace", nameof(dmaVersion));
            }

            string[] versionParts = dmaVersion.Split('.');

            if (versionParts[2].Contains("-"))
            {
                if (!DataMinerRegex.IsMatch(dmaVersion))
                {
                    throw new FormatException("The specified version has an invalid format.");
                }

                if (!CURegex.IsMatch(versionParts[2]))
                {
                    throw new FormatException("The specified version has an invalid CU Format. Expected \"-CU[0-9]+\" ");
                }
            }
            else
            {
                if (!DataMinerShortRegex.IsMatch(dmaVersion))
                {
                    throw new FormatException("The specified version has an invalid format.");
                }
            }

            return new DMAppVersion(dmaVersion);
        }

        /// <summary>
        /// Converts a build number to an app package version.
        /// </summary>
        /// <param name="buildNumber">The build number</param>
        /// <returns>The corresponding app package version.</returns>
        public static DMAppVersion FromBuildNumber(int buildNumber)
        {
            if (buildNumber < 0)
            {
                throw new ArgumentException("Value can not be lower than zero.", nameof(buildNumber));
            }

            return FromBuildNumber((uint)buildNumber);
        }

        /// <summary>
        /// Converts a build number to an app package version.
        /// </summary>
        /// <param name="buildNumber">The build number</param>
        /// <returns>The corresponding app package version.</returns>
        public static DMAppVersion FromBuildNumber(uint buildNumber)
        {
            string packageVersion = "0.0.0-CU" + buildNumber;

            return new DMAppVersion(packageVersion);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return _version;
        }
    }
}
