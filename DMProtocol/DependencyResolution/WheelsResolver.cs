namespace Skyline.DataMiner.CICD.DMProtocol.DependencyResolution
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;
    using Skyline.DataMiner.CICD.DMProtocol.DependencyResolution.Exceptions;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;

    /// <summary>
    /// Resolves and downloads Python wheels for Windows and Linux and organizes them into platform-specific and universal
    /// directories, ready to be embedded in the <c>Scripts/{guid}/dependencies</c> folder of a <c>.dmprotocol</c> package.
    /// </summary>
    /// <remarks>
    /// This is a C# port of the reference implementation available at
    /// https://github.com/SkylineCommunications/PythonScriptedConnectorWheelsResolver. It still relies on an external
    /// <c>pip</c> executable being available on the system to perform the actual dependency resolution/download.
    /// </remarks>
    internal sealed class WheelsResolver
    {
        private const int MinimumCompatibleGlibc2Version = 17;

        /// <summary>
        /// Fallback glibc 2.X version to use in case the latest available version could not be resolved online.
        /// </summary>
        private const int FallbackLatestGlibc2Version = 43;

        private const string GlibcMirrorUrl = "https://ftpmirror.gnu.org/glibc";

        /// <summary>
        /// Maximum number of HTTP redirects to follow while resolving the latest glibc version.
        /// </summary>
        private const int MaxRedirects = 10;

        private readonly ILogCollector _logCollector;
        private readonly IFileSystem _fileSystem;
        private readonly PipCommand _pipCommand;

        private readonly string _dependenciesDirectory;
        private readonly string _windowsDirectory;
        private readonly string _linuxDirectory;
        private readonly string _universalDirectory;

        /// <summary>
        /// Gets the Windows-specific wheels directory. Exposed internally for unit testing.
        /// </summary>
        internal string WindowsDirectory => _windowsDirectory;

        /// <summary>
        /// Gets the Linux-specific wheels directory. Exposed internally for unit testing.
        /// </summary>
        internal string LinuxDirectory => _linuxDirectory;

        /// <summary>
        /// Gets the universal wheels directory. Exposed internally for unit testing.
        /// </summary>
        internal string UniversalDirectory => _universalDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="WheelsResolver"/> class.
        /// </summary>
        /// <param name="logCollector">The log collector used to report progress and errors.</param>
        /// <param name="outputDirectory">
        /// The output directory. A <c>dependencies</c> subfolder will be (re)created here, containing the
        /// <c>x86_64-windows-msvc</c>, <c>x86_64-linux-gnu</c> and <c>universal</c> folders.
        /// </param>
        /// <exception cref="PipNotFoundException">No valid pip executable could be found on the current system.</exception>
        public WheelsResolver(ILogCollector logCollector, string outputDirectory)
            : this(logCollector, FileSystem.Instance, outputDirectory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WheelsResolver"/> class.
        /// </summary>
        /// <param name="logCollector">The log collector used to report progress and errors.</param>
        /// <param name="fileSystem">The file system abstraction to use.</param>
        /// <param name="outputDirectory">
        /// The output directory. A <c>dependencies</c> subfolder will be (re)created here, containing the
        /// <c>x86_64-windows-msvc</c>, <c>x86_64-linux-gnu</c> and <c>universal</c> folders.
        /// </param>
        /// <exception cref="PipNotFoundException">No valid pip executable could be found on the current system.</exception>
        internal WheelsResolver(ILogCollector logCollector, IFileSystem fileSystem, string outputDirectory)
            : this(logCollector, fileSystem, outputDirectory, PipResolver.ResolvePip())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WheelsResolver"/> class with an explicit pip command, bypassing
        /// pip resolution. Intended for unit testing the file-system-only logic (e.g. <see cref="ProcessDownloadedWheels"/>)
        /// without requiring pip to be installed on the machine running the tests.
        /// </summary>
        internal WheelsResolver(ILogCollector logCollector, IFileSystem fileSystem, string outputDirectory, PipCommand pipCommand)
        {
            _logCollector = logCollector ?? throw new ArgumentNullException(nameof(logCollector));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            if (String.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new ArgumentException("Output directory cannot be null, empty or whitespace.", nameof(outputDirectory));
            }

            _pipCommand = pipCommand ?? throw new ArgumentNullException(nameof(pipCommand));

            _dependenciesDirectory = _fileSystem.Path.Combine(outputDirectory, "dependencies");
            _windowsDirectory = _fileSystem.Path.Combine(_dependenciesDirectory, SupportedPlatform.Windows.ToTargetTriple());
            _linuxDirectory = _fileSystem.Path.Combine(_dependenciesDirectory, SupportedPlatform.Linux.ToTargetTriple());
            _universalDirectory = _fileSystem.Path.Combine(_dependenciesDirectory, "universal");

            // If it already exists, delete it to be sure that it is empty.
            if (_fileSystem.Directory.Exists(_dependenciesDirectory))
            {
                _fileSystem.Directory.DeleteDirectory(_dependenciesDirectory);
            }

            _fileSystem.Directory.CreateDirectory(outputDirectory);
            _fileSystem.Directory.CreateDirectory(_dependenciesDirectory);
            _fileSystem.Directory.CreateDirectory(_windowsDirectory);
            _fileSystem.Directory.CreateDirectory(_linuxDirectory);
            _fileSystem.Directory.CreateDirectory(_universalDirectory);
        }

        /// <summary>
        /// Resolves the dependencies declared in <paramref name="requirementsFilePath"/> and downloads the necessary wheels
        /// for Windows and Linux. The downloaded wheels are organized into platform-specific and universal directories.
        /// </summary>
        /// <param name="requirementsFilePath">Path to the requirements file (containing only direct dependencies).</param>
        /// <param name="pythonVersion">Target Python version for which dependencies should be resolved (pip format, e.g. "3.14").</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The platforms for which wheels could successfully be downloaded.</returns>
        /// <exception cref="RequirementsNotFoundException">The requirements file does not exist.</exception>
        /// <exception cref="ConflictingDependenciesException">Pip detected conflicting dependencies.</exception>
        public async Task<IReadOnlyList<SupportedPlatform>> DownloadWheelsAsync(string requirementsFilePath, string pythonVersion, CancellationToken cancellationToken = default)
        {
            if (!_fileSystem.File.Exists(requirementsFilePath))
            {
                throw new RequirementsNotFoundException(requirementsFilePath);
            }

            await CheckConflictingDependenciesAsync(requirementsFilePath, pythonVersion, cancellationToken).ConfigureAwait(false);

            var supportedPlatforms = new List<SupportedPlatform>();

            if (await DownloadWindowsWheelsAsync(requirementsFilePath, pythonVersion, cancellationToken).ConfigureAwait(false))
            {
                supportedPlatforms.Add(SupportedPlatform.Windows);
            }

            if (await DownloadLinuxWheelsAsync(requirementsFilePath, pythonVersion, cancellationToken).ConfigureAwait(false))
            {
                supportedPlatforms.Add(SupportedPlatform.Linux);
            }

            ProcessDownloadedWheels();

            return supportedPlatforms;
        }

        /// <summary>
        /// Performs a pip dry-run install to detect dependency conflicts before attempting to download anything.
        /// </summary>
        private async Task CheckConflictingDependenciesAsync(string requirementsFilePath, string pythonVersion, CancellationToken cancellationToken)
        {
            _logCollector.ReportStatus("Checking for possible conflicting dependencies...");

            var arguments = new List<string>
            {
                "install",
                "--break-system-packages", // Necessary in case the Python running pip is externally managed (e.g. installed through uv). Safe here since this is a dry-run.
                "--dry-run",
                "--only-binary=:all:",
                "-r",
                requirementsFilePath,
                "--implementation",
                "cp",
                "--python-version",
                pythonVersion,
            };

            var result = await RunPipAsync(arguments, cancellationToken).ConfigureAwait(false);

            if (result.ExitCode != 0 && result.StandardError.IndexOf("conflicting dependencies", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                throw new ConflictingDependenciesException(result.StandardOutput, result.StandardError);
            }
        }

        /// <summary>
        /// Downloads wheels compatible with Windows.
        /// </summary>
        private async Task<bool> DownloadWindowsWheelsAsync(string requirementsFilePath, string pythonVersion, CancellationToken cancellationToken)
        {
            _logCollector.ReportStatus("Downloading Windows wheels...");

            var result = await RunPipDownloadAsync(requirementsFilePath, pythonVersion, new[] { "win_amd64" }, _windowsDirectory, cancellationToken).ConfigureAwait(false);
            if (result.ExitCode != 0)
            {
                _logCollector.ReportError($"Could not download the dependencies for Windows{Environment.NewLine}stdout: {result.StandardOutput}{Environment.NewLine}stderr: {result.StandardError}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Downloads wheels compatible with Linux using manylinux platform tags.
        /// </summary>
        private async Task<bool> DownloadLinuxWheelsAsync(string requirementsFilePath, string pythonVersion, CancellationToken cancellationToken)
        {
            _logCollector.ReportStatus("Downloading Linux wheels...");

            // Resolve the max available glibc version so we know which platform tags exist and we should try.
            int? latestGlibc2 = await ResolveLatestGlibc2VersionAsync(cancellationToken).ConfigureAwait(false);
            if (latestGlibc2 is null)
            {
                _logCollector.ReportWarning($"Could not resolve the latest available glibc 2.X version, falling back to 2.{FallbackLatestGlibc2Version} as latest known version");
                latestGlibc2 = FallbackLatestGlibc2Version;
            }

            // Try to download the dependencies, preferring the platform with the lowest glibc version.
            // When providing multiple "valid" platform tags, pip automatically takes the first one in the list that works for each package.
            // Since this list contains all platforms in order old -> new, this maximizes compatibility for the packages.
            var platformTags = new List<string> { "manylinux2014_x86_64" }; // Old name for manylinux_2_17_x86_64, so also valid for our use case.
            for (int version = MinimumCompatibleGlibc2Version; version < latestGlibc2.Value; version++)
            {
                platformTags.Add($"manylinux_2_{version}_x86_64");
            }

            var result = await RunPipDownloadAsync(requirementsFilePath, pythonVersion, platformTags, _linuxDirectory, cancellationToken).ConfigureAwait(false);
            if (result.ExitCode != 0)
            {
                _logCollector.ReportError($"Could not download the dependencies for Linux{Environment.NewLine}stdout: {result.StandardOutput}{Environment.NewLine}stderr: {result.StandardError}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Detects the latest available glibc 2.X version from the GNU mirror listing.
        /// </summary>
        private async Task<int?> ResolveLatestGlibc2VersionAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var handler = new HttpClientHandler
                {
                    // The mirror redirect service can redirect to a plain HTTP mirror (and HttpClient refuses to
                    // auto-follow an HTTPS -> HTTP downgrade redirect), and some HTTPS mirrors don't have a fully
                    // valid certificate. Redirects are therefore followed manually below, and certificate validation
                    // is disabled, mirroring the reference Python implementation's use of an unverified SSL context
                    // for this specific lookup.
                    AllowAutoRedirect = false,
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
                };

                using var httpClient = new HttpClient(handler);

                string html = await GetFollowingRedirectsAsync(httpClient, new Uri(GlibcMirrorUrl), cancellationToken).ConfigureAwait(false);
                if (html == null)
                {
                    return null;
                }

                var matches = Regex.Matches(html, @"glibc-2\.([0-9]+)(\.[0-9]+)?\.tar\.xz");
                if (matches.Count == 0)
                {
                    return null;
                }

                int latestVersion = matches
                    .Cast<Match>()
                    .Select(m => Int32.Parse(m.Groups[1].Value))
                    .Distinct()
                    .Max();

                _logCollector.ReportDebug($"Found latest glibc version 2.{latestVersion}");
                return latestVersion;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
            {
                _logCollector.ReportWarning($"An exception occurred while resolving the latest glibc version: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Issues a GET request to <paramref name="uri"/>, manually following up to <see cref="MaxRedirects"/>
        /// redirects (including HTTPS -> HTTP downgrades, which <see cref="HttpClient"/> does not auto-follow).
        /// </summary>
        /// <returns>The response body, or <see langword="null"/> if the response did not resolve to a 200 OK.</returns>
        private static async Task<string> GetFollowingRedirectsAsync(HttpClient httpClient, Uri uri, CancellationToken cancellationToken)
        {
            for (int redirectCount = 0; redirectCount <= MaxRedirects; redirectCount++)
            {
                using var response = await httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);

                if (IsRedirect(response.StatusCode))
                {
                    Uri location = response.Headers.Location;
                    if (location == null)
                    {
                        return null;
                    }

                    uri = location.IsAbsoluteUri ? location : new Uri(uri, location);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return null;
        }

        private static bool IsRedirect(System.Net.HttpStatusCode statusCode)
        {
            int code = (int)statusCode;
            return code == 301 // MovedPermanently
                || code == 302 // Found
                || code == 307 // TemporaryRedirect
                || code == 308; // PermanentRedirect (not defined in the net48 HttpStatusCode enum)
        }

        /// <summary>
        /// Runs <c>pip download</c> for a given platform configuration.
        /// </summary>
        private Task<PipProcessResult> RunPipDownloadAsync(string requirementsFilePath, string pythonVersion, IEnumerable<string> platformTags, string destinationDirectory, CancellationToken cancellationToken)
        {
            var arguments = new List<string>
            {
                "download",
                "-r",
                requirementsFilePath,
                "--only-binary=:all:",
                "--dest",
                destinationDirectory,
                "--implementation",
                "cp",
                "--python-version",
                pythonVersion,
            };

            foreach (string platformTag in platformTags)
            {
                arguments.Add("--platform");
                arguments.Add(platformTag);
            }

            return RunPipAsync(arguments, cancellationToken);
        }

        /// <summary>
        /// Executes the resolved pip command with the specified arguments.
        /// </summary>
        private async Task<PipProcessResult> RunPipAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
        {
            var startInfo = _pipCommand.CreateProcessStartInfo(arguments);

            using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

            var processExited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            process.Exited += (_, _) => processExited.TrySetResult(true);

            process.Start();

            Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();

            using (cancellationToken.Register(() => processExited.TrySetCanceled(cancellationToken)))
            {
                try
                {
                    await processExited.Task.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Kill the (possibly still running) child process tree so it doesn't keep writing into the staging
                    // directory after this method returns, then swallow the (now-orphaned) output-read tasks so they
                    // don't surface as unobserved task exceptions.
                    TryKillProcess(process);
                    await SwallowAsync(standardOutputTask).ConfigureAwait(false);
                    await SwallowAsync(standardErrorTask).ConfigureAwait(false);
                    throw;
                }
            }

            string standardOutput = await standardOutputTask.ConfigureAwait(false);
            string standardError = await standardErrorTask.ConfigureAwait(false);

            return new PipProcessResult(process.ExitCode, standardOutput, standardError);
        }

        /// <summary>
        /// Kills <paramref name="process"/> (including any child processes) if it hasn't exited yet, ignoring any
        /// errors (e.g. the process exiting on its own between the check and the kill call).
        /// </summary>
        private static void TryKillProcess(Process process)
        {
            try
            {
                if (!process.HasExited)
                {
                    // net48's Process.Kill() has no "entire tree" overload; pip itself is expected to be the only
                    // process performing the network I/O for this call, so killing just the immediate process is
                    // sufficient here.
                    process.Kill();
                }
            }
            catch (Exception)
            {
                // Best-effort: the process may have already exited, or may not support killing the entire tree on
                // this platform. Either way, there's nothing more we can do here.
            }
        }

        /// <summary>
        /// Awaits <paramref name="task"/>, discarding any exception, so it doesn't surface as an unobserved task
        /// exception once <paramref name="task"/> completes.
        /// </summary>
        private static async Task SwallowAsync(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Intentionally ignored; the operation was already cancelled.
            }
        }

        /// <summary>
        /// Deduplicates downloaded wheels and moves shared or universal wheels to the universal directory.
        /// </summary>
        internal void ProcessDownloadedWheels()
        {
            var windowsWheels = ExtractWheelsFromFolder(_windowsDirectory);
            var linuxWheels = ExtractWheelsFromFolder(_linuxDirectory);

            MoveSharedWheels(windowsWheels, linuxWheels);
            PreferUniversalWheels(windowsWheels, linuxWheels);
        }

        /// <summary>
        /// Moves wheels present on both platforms to the universal directory.
        /// </summary>
        private void MoveSharedWheels(HashSet<string> windowsWheels, HashSet<string> linuxWheels)
        {
            var shared = new HashSet<string>(windowsWheels, StringComparer.Ordinal);
            shared.IntersectWith(linuxWheels);

            foreach (string wheel in shared)
            {
                MoveFile(_windowsDirectory, wheel, _universalDirectory, wheel);
                DeleteFile(_linuxDirectory, wheel);
            }

            windowsWheels.ExceptWith(shared);
            linuxWheels.ExceptWith(shared);
        }

        /// <summary>
        /// Prefers universal wheels when one platform has a platform-specific wheel but the other has a universal wheel
        /// for the same package/version. Even though universal wheels can be slower than their platform-specific
        /// counterparts, this reduces the overall package size, which is more important for scripted connector packages.
        /// </summary>
        private void PreferUniversalWheels(HashSet<string> windowsWheels, HashSet<string> linuxWheels)
        {
            var windowsUniversalMap = BuildUniversalWheelMap(windowsWheels);
            var linuxUniversalMap = BuildUniversalWheelMap(linuxWheels);

            ReplaceWithUniversal(linuxWheels, windowsUniversalMap, _linuxDirectory, _windowsDirectory);
            ReplaceWithUniversal(windowsWheels, linuxUniversalMap, _windowsDirectory, _linuxDirectory);
        }

        /// <summary>
        /// Replaces platform-specific wheels with universal wheels when available on the other platform.
        /// </summary>
        private void ReplaceWithUniversal(HashSet<string> wheels, IReadOnlyDictionary<string, string> universalMap, string wheelsSourceDirectory, string universalSourceDirectory)
        {
            foreach (string wheel in wheels.ToList())
            {
                string prefix = GetPackagePrefix(wheel);

                if (universalMap.TryGetValue(prefix, out string universalWheel))
                {
                    MoveFile(universalSourceDirectory, universalWheel, _universalDirectory, universalWheel);
                    DeleteFile(wheelsSourceDirectory, wheel);
                }
            }
        }

        /// <summary>
        /// Lists all wheel filenames in a directory.
        /// </summary>
        private HashSet<string> ExtractWheelsFromFolder(string wheelsFolder)
        {
            return new HashSet<string>(
                _fileSystem.Directory.EnumerateFiles(wheelsFolder, "*.whl").Select(_fileSystem.Path.GetFileName),
                StringComparer.Ordinal);
        }

        /// <summary>
        /// Builds a mapping of package-version prefix to universal wheel filenames.
        /// </summary>
        private static Dictionary<string, string> BuildUniversalWheelMap(IEnumerable<string> wheels)
        {
            return wheels
                .Where(IsUniversalWheel)
                .ToDictionary(GetPackagePrefix, wheel => wheel, StringComparer.Ordinal);
        }

        /// <summary>
        /// Checks whether a wheel is platform-independent.
        /// </summary>
        private static bool IsUniversalWheel(string wheelName)
        {
            return wheelName.EndsWith("-none-any.whl", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Extracts the 'package-version' prefix from a wheel filename.
        /// </summary>
        private static string GetPackagePrefix(string wheelName)
        {
            string[] parts = wheelName.Split('-');
            return parts.Length >= 2 ? $"{parts[0]}-{parts[1]}" : wheelName;
        }

        private void MoveFile(string sourceDirectory, string sourceFileName, string destinationDirectory, string destinationFileName)
        {
            string sourcePath = _fileSystem.Path.Combine(sourceDirectory, sourceFileName);
            string destinationPath = _fileSystem.Path.Combine(destinationDirectory, destinationFileName);
            _fileSystem.File.Move(sourcePath, destinationPath);
        }

        private void DeleteFile(string directory, string fileName)
        {
            _fileSystem.File.Delete(_fileSystem.Path.Combine(directory, fileName));
        }

        /// <summary>
        /// Result of a completed pip process invocation.
        /// </summary>
        private sealed class PipProcessResult
        {
            public PipProcessResult(int exitCode, string standardOutput, string standardError)
            {
                ExitCode = exitCode;
                StandardOutput = standardOutput;
                StandardError = standardError;
            }

            public int ExitCode { get; }

            public string StandardOutput { get; }

            public string StandardError { get; }
        }
    }
}
