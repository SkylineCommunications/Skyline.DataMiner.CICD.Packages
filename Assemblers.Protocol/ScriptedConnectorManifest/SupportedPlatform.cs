namespace Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest
{
    /// <summary>
    /// Represents a platform for which the Edge Node scripted connector runtime can resolve and install Python wheel dependencies.
    /// </summary>
    public enum SupportedPlatform
    {
        /// <summary>
        /// 64-bit Windows, wheels built against MSVC (platform tag <c>win_amd64</c>).
        /// </summary>
        Windows,

        /// <summary>
        /// 64-bit Linux, wheels built against glibc (platform tags <c>manylinux2014_x86_64</c> / <c>manylinux_2_&lt;x&gt;_x86_64</c>).
        /// </summary>
        Linux
    }

    /// <summary>
    /// Extension methods for <see cref="SupportedPlatform"/>.
    /// </summary>
    public static class SupportedPlatformExtensions
    {
        /// <summary>
        /// Gets the target triple used as the folder name inside the <c>dependencies</c> folder of a scripted connector package (e.g. <c>x86_64-windows-msvc</c>).
        /// </summary>
        /// <param name="platform">The platform.</param>
        /// <returns>The target triple.</returns>
        public static string ToTargetTriple(this SupportedPlatform platform)
        {
            return platform switch
            {
                SupportedPlatform.Windows => "x86_64-windows-msvc",
                SupportedPlatform.Linux => "x86_64-linux-gnu",
                _ => throw new System.ArgumentOutOfRangeException(nameof(platform), platform, "Unknown supported platform.")
            };
        }

        /// <summary>
        /// Parses a target triple (e.g. <c>x86_64-windows-msvc</c>) as used in <c>manifest.json</c>'s <c>supported_platforms</c> and the <c>dependencies</c> folder layout.
        /// </summary>
        /// <param name="targetTriple">The target triple to parse.</param>
        /// <returns>The corresponding <see cref="SupportedPlatform"/>.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="targetTriple"/> does not correspond to a known, supported platform.</exception>
        public static SupportedPlatform FromTargetTriple(string targetTriple)
        {
            return targetTriple switch
            {
                "x86_64-windows-msvc" => SupportedPlatform.Windows,
                "x86_64-linux-gnu" => SupportedPlatform.Linux,
                _ => throw new System.ArgumentException($"Unknown or unsupported platform target triple '{targetTriple}'.", nameof(targetTriple))
            };
        }
    }
}
