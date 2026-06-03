namespace Skyline.DataMiner.CICD.Packages.TestHelpers
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

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
        /// <param name="sdkVersion">The Skyline.DataMiner.Sdk version to pin in global.json. Default is "2.5.2".</param>
        /// <param name="methodName">The test method name (auto-captured via CallerMemberName).</param>
        /// <returns>The full path to the created test directory.</returns>
        public static string InitializeDirectoryForTest(string sdkVersion = "2.5.2", [CallerMemberName] string? methodName = null)
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
    }
}
