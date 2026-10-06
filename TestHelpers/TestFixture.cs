namespace Skyline.DataMiner.CICD.Packages.TestHelpers
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Build.Locator;

    using Skyline.DataMiner.CICD.FileSystem;

    /// <summary>
    /// Base fixture utilities for test project/solution creation.
    /// </summary>
    public static class TestFixture
    {
        private static readonly IFileSystem FileSystem = CICD.FileSystem.FileSystem.Instance;

        /// <summary>
        /// Gets the root directory where all test fixtures are created.
        /// </summary>
        public static string TestFixtureRoot => FileSystem.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GeneratedTestFiles");

        /// <summary>
        /// Ensures MSBuild is registered for project evaluation (Windows only).
        /// </summary>
        public static void EnsureMsBuildRegistered()
        {
            if (!MSBuildLocator.IsRegistered && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                MSBuildLocator.RegisterDefaults();
            }
        }

        /// <summary>
        /// Initializes a new test directory under <see cref="TestFixtureRoot"/> with a global.json for the DataMiner SDK.
        /// </summary>
        /// <param name="sdkVersion">The Skyline.DataMiner.Sdk version to pin in global.json. Default is "2.5.9-sdmfix20261005.1".</param>
        /// <param name="methodName">The test method name (auto-captured via CallerMemberName).</param>
        /// <returns>The full path to the created test directory.</returns>
        public static string InitializeDirectoryForTest(string sdkVersion = "2.5.9-sdmfix20261005.1", [CallerMemberName] string? methodName = null)
        {
            if (String.IsNullOrWhiteSpace(methodName))
            {
                methodName = Guid.NewGuid().ToString();
            }

#if NETFRAMEWORK
            string suffix = "NETFRAMEWORK";
#else
            string suffix = "NET";
#endif

            string testDir = FileSystem.Path.Combine(TestFixtureRoot, methodName, suffix);
            FileSystem.Directory.CreateDirectory(testDir);

            string globalJsonContent = $$"""
                                        {
                                          "msbuild-sdks": {
                                            "Skyline.DataMiner.Sdk": "{{sdkVersion}}"
                                          }
                                        }
                                        """;
            WriteFile(FileSystem.Path.Combine(testDir, "global.json"), globalJsonContent);

            return testDir;
        }

        /// <summary>
        /// Deletes the entire <see cref="TestFixtureRoot"/> directory (ignores errors).
        /// </summary>
        public static void CleanupRoot()
        {
            if (FileSystem.Directory.Exists(TestFixtureRoot))
            {
                try
                {
                    FileSystem.Directory.Delete(TestFixtureRoot, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        /// <summary>
        /// Writes a file to disk, creating parent directories if needed.
        /// </summary>
        /// <param name="fullPath">The absolute path to the file.</param>
        /// <param name="content">The content to write.</param>
        public static void WriteFile(string fullPath, string content)
        {
            string? dir = FileSystem.Path.GetDirectoryName(fullPath);
            if (!String.IsNullOrEmpty(dir))
            {
                FileSystem.Directory.CreateDirectory(dir);
            }

            FileSystem.File.WriteAllText(fullPath, content);
        }

        /// <summary>
        /// Writes a binary file to disk, creating parent directories if needed.
        /// </summary>
        /// <param name="fullPath">The absolute path to the file.</param>
        /// <param name="content">The binary content to write.</param>
        public static void WriteBinaryFile(string fullPath, byte[] content)
        {
            string? dir = FileSystem.Path.GetDirectoryName(fullPath);
            if (!String.IsNullOrEmpty(dir))
            {
                FileSystem.Directory.CreateDirectory(dir);
            }

            FileSystem.File.WriteAllBytes(fullPath, content);
        }

        /// <summary>
        /// Builds a real fixture project with bounded process and redirected-output waits.
        /// </summary>
        /// <param name="projectPath">The fixture project file.</param>
        public static async Task BuildProjectAsync(string projectPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectPath}\" -c Debug --nologo --disable-build-servers --verbosity quiet -p:ImportDirectoryBuildProps=false -p:ManagePackageVersionsCentrally=false -p:GeneratePackageOnBuild=false -p:RestoreSources=https://api.nuget.org/v3/index.json",
                WorkingDirectory = FileSystem.Path.GetDirectoryName(projectPath),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            // MSBuildLocator's desktop host must not override the child dotnet SDK.
            startInfo.EnvironmentVariables.Remove("MSBUILD_EXE_PATH");
            startInfo.EnvironmentVariables.Remove("MSBuildExtensionsPath");
            startInfo.EnvironmentVariables.Remove("MSBuildSDKsPath");
            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException($"Could not start the fixture build for '{projectPath}'.");
            Task<string> output = process.StandardOutput.ReadToEndAsync();
            Task<string> errors = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(180000))
            {
                try
                {
#if NETFRAMEWORK
                using var termination = Process.Start(new ProcessStartInfo
                {
                    FileName = FileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "taskkill.exe"),
                    Arguments = $"/PID {process.Id} /T /F",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }) ?? throw new InvalidOperationException($"Could not terminate fixture process {process.Id}.");
                if (!termination.WaitForExit(10000))
                {
                    termination.Kill();
                    throw new TimeoutException($"Termination of fixture process {process.Id} timed out.");
                }
                if (termination.ExitCode != 0 && !process.HasExited)
                {
                    throw new InvalidOperationException($"Could not terminate fixture process tree {process.Id}.");
                }
#else
                process.Kill(entireProcessTree: true);
#endif
                if (!process.WaitForExit(10000))
                {
                    throw new TimeoutException($"Fixture process {process.Id} did not exit after termination.");
                }
                throw new TimeoutException($"Fixture build exceeded three minutes: '{projectPath}'.");
                }
                finally
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        if (!process.WaitForExit(10000))
                        {
                            throw new TimeoutException($"Fixture process {process.Id} could not be reaped after failed tree termination.");
                        }
                    }
                }
            }

            var reads = Task.WhenAll(output, errors);
            using var readTimeout = new CancellationTokenSource();
            if (await Task.WhenAny(reads, Task.Delay(TimeSpan.FromSeconds(30), readTimeout.Token)).ConfigureAwait(false) != reads)
            {
                throw new TimeoutException($"Fixture output pipes did not close: '{projectPath}'.");
            }

            readTimeout.Cancel();
            await reads.ConfigureAwait(false);
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Fixture build failed for '{projectPath}'.{Environment.NewLine}{output.Result}{Environment.NewLine}{errors.Result}");
            }
        }
    }
}
