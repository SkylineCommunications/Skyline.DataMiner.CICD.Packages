namespace DMProtocolTests.DependencyResolution
{
    using System;
    using System.IO;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.DMProtocol.DependencyResolution;
    using Skyline.DataMiner.CICD.FileSystem;
    using Skyline.DataMiner.CICD.Loggers;

    [TestClass]
    public class WheelsResolverTests
    {
        // No-op pip command; the tests in this class only exercise the file-system-only wheel deduplication logic
        // (ProcessDownloadedWheels), so pip is never actually invoked.
        private static readonly PipCommand NoOpPipCommand = new PipCommand("echo", System.Array.Empty<string>());

        private string _outputDirectory;

        [TestInitialize]
        public void TestInitialize()
        {
            _outputDirectory = FileSystem.Instance.Directory.CreateTemporaryDirectory();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (FileSystem.Instance.Directory.Exists(_outputDirectory))
            {
                FileSystem.Instance.Directory.DeleteDirectory(_outputDirectory);
            }
        }

        [TestMethod]
        public void ProcessDownloadedWheels_WheelPresentOnBothPlatforms_MovedToUniversal()
        {
            // Arrange
            var resolver = new WheelsResolver(new LogCollector(), FileSystem.Instance, _outputDirectory, NoOpPipCommand);
            const string sharedWheel = "certifi-2026.7.22-py3-none-any.whl";
            CreateWheel(resolver.WindowsDirectory, sharedWheel);
            CreateWheel(resolver.LinuxDirectory, sharedWheel);

            // Act
            resolver.ProcessDownloadedWheels();

            // Assert
            File.Exists(Path.Combine(resolver.UniversalDirectory, sharedWheel)).Should().BeTrue();
            File.Exists(Path.Combine(resolver.WindowsDirectory, sharedWheel)).Should().BeFalse();
            File.Exists(Path.Combine(resolver.LinuxDirectory, sharedWheel)).Should().BeFalse();
        }

        [TestMethod]
        public void ProcessDownloadedWheels_UniversalWheelAvailableOnOnePlatform_ReplacesPlatformSpecificWheel()
        {
            // Arrange
            var resolver = new WheelsResolver(new LogCollector(), FileSystem.Instance, _outputDirectory, NoOpPipCommand);
            const string linuxSpecificWheel = "charset_normalizer-3.5.1-cp314-cp314-manylinux2014_x86_64.whl";
            const string windowsUniversalWheel = "charset_normalizer-3.5.1-py3-none-any.whl";
            CreateWheel(resolver.LinuxDirectory, linuxSpecificWheel);
            CreateWheel(resolver.WindowsDirectory, windowsUniversalWheel);

            // Act
            resolver.ProcessDownloadedWheels();

            // Assert: the Linux-specific wheel is dropped in favor of the (now shared) universal wheel.
            File.Exists(Path.Combine(resolver.UniversalDirectory, windowsUniversalWheel)).Should().BeTrue();
            File.Exists(Path.Combine(resolver.LinuxDirectory, linuxSpecificWheel)).Should().BeFalse();
            File.Exists(Path.Combine(resolver.WindowsDirectory, windowsUniversalWheel)).Should().BeFalse();
        }

        [TestMethod]
        public void ProcessDownloadedWheels_PlatformSpecificWheelsOnly_RemainOnRespectivePlatform()
        {
            // Arrange
            var resolver = new WheelsResolver(new LogCollector(), FileSystem.Instance, _outputDirectory, NoOpPipCommand);
            const string windowsWheel = "numpy-2.5.3-cp314-cp314-win_amd64.whl";
            const string linuxWheel = "numpy-2.5.3-cp314-cp314-manylinux_2_27_x86_64.whl";
            CreateWheel(resolver.WindowsDirectory, windowsWheel);
            CreateWheel(resolver.LinuxDirectory, linuxWheel);

            // Act
            resolver.ProcessDownloadedWheels();

            // Assert: different package-version prefixes ("numpy-2.5.3" matches for both here, but the file names
            // differ and neither is a universal ("-none-any") wheel) so nothing should be moved to universal.
            Directory.GetFiles(resolver.UniversalDirectory, "*.whl").Should().BeEmpty();
            File.Exists(Path.Combine(resolver.WindowsDirectory, windowsWheel)).Should().BeTrue();
            File.Exists(Path.Combine(resolver.LinuxDirectory, linuxWheel)).Should().BeTrue();
        }

        [TestMethod]
        public void ProcessDownloadedWheels_BothPlatformsHaveOwnUniversalWheelForSamePackage_DoesNotThrow()
        {
            // Arrange: Windows and Linux each independently resolved their own universal ("-none-any") wheel for the
            // same package/version, but under different filenames (e.g. a 'py2.py3' tag vs. a 'py3' tag). This used to
            // crash with an IOException because the second PreferUniversalWheels pass re-processed a file already
            // moved/deleted by the first pass.
            var resolver = new WheelsResolver(new LogCollector(), FileSystem.Instance, _outputDirectory, NoOpPipCommand);
            const string windowsUniversalWheel = "six-1.17.0-py2.py3-none-any.whl";
            const string linuxUniversalWheel = "six-1.17.0-py3-none-any.whl";
            CreateWheel(resolver.WindowsDirectory, windowsUniversalWheel);
            CreateWheel(resolver.LinuxDirectory, linuxUniversalWheel);

            // Act
            Action act = () => resolver.ProcessDownloadedWheels();

            // Assert: no exception, and exactly one of the two universal wheels ends up in the universal directory.
            act.Should().NotThrow();
            File.Exists(Path.Combine(resolver.WindowsDirectory, windowsUniversalWheel)).Should().BeFalse();
            File.Exists(Path.Combine(resolver.LinuxDirectory, linuxUniversalWheel)).Should().BeFalse();
            Directory.GetFiles(resolver.UniversalDirectory, "*.whl").Should().HaveCount(1);
        }

        private static void CreateWheel(string directory, string fileName)
        {
            FileSystem.Instance.File.WriteAllText(FileSystem.Instance.Path.Combine(directory, fileName), String.Empty);
        }
    }
}
