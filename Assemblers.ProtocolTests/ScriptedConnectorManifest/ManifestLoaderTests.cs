namespace Assemblers.ProtocolTests.ScriptedConnectorManifest
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;

    [TestClass]
    public class ManifestLoaderTests
    {
        private static Manifest CreateValidManifest()
        {
            return new Manifest
            {
                SchemaVersion = "1.0",
                Project = new ManifestProject
                {
                    Id = Guid.NewGuid(),
                    Name = "MyConnector",
                    Version = "1.0.0",
                },
                Runtime = new ManifestRuntime
                {
                    Language = RuntimeLanguage.Python,
                    SupportedPlatforms = new[] { SupportedPlatform.Windows, SupportedPlatform.Linux },
                    Python = new ManifestPython { Version = ">=3.14", EntryPoint = "run/main.py" },
                },
            };
        }

        [TestMethod]
        public void Validate_ValidManifest_DoesNotThrow()
        {
            // Arrange
            Manifest manifest = CreateValidManifest();

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Validate_NullManifest_Throws()
        {
            // Act
            Action act = () => ManifestLoader.Validate(null);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*empty*");
        }

        [TestMethod]
        public void Validate_MissingSchemaVersion_Throws()
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.SchemaVersion = null;

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*schema_version*");
        }

        [TestMethod]
        public void Validate_MissingProject_Throws()
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Project = null;

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*project.name*");
        }

        [TestMethod]
        public void Validate_MissingProjectName_Throws()
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Project.Name = " ";

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*project.name*");
        }

        [TestMethod]
        public void Validate_EmptyProjectId_Throws()
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Project.Id = Guid.Empty;

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*project.id*");
        }

        [TestMethod]
        public void Validate_MissingProjectVersion_Throws()
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Project.Version = null;

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*project.version*");
        }

        [TestMethod]
        public void Validate_MissingRuntime_Throws()
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Runtime = null;

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*runtime.language*");
        }

        [TestMethod]
        public void Validate_NoSupportedPlatforms_Throws()
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Runtime.SupportedPlatforms = Array.Empty<SupportedPlatform>();

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*supported_platforms*");
        }

        [TestMethod]
        public void Validate_NullSupportedPlatforms_Throws()
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Runtime.SupportedPlatforms = null;

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*supported_platforms*");
        }

        [TestMethod]
        public void Validate_MissingPythonSection_Throws()
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Runtime.Python = null;

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*entry_point*");
        }

        [TestMethod]
        public void Validate_MissingEntryPoint_Throws()
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Runtime.Python.EntryPoint = " ";

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*entry_point*");
        }

        [TestMethod]
        public void Validate_MissingPythonVersion_Throws()
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Runtime.Python.Version = null;

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*runtime.python.version*");
        }

        [TestMethod]
        [DataRow("../secrets.txt")]
        [DataRow("run/../../secrets.txt")]
        [DataRow("run/./main.py")]
        [DataRow("..\\secrets.txt")]
        public void Validate_EntryPointWithTraversalSegments_Throws(string entryPoint)
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Runtime.Python.EntryPoint = entryPoint;

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*entry_point*");
        }

        [TestMethod]
        [DataRow("C:\\secrets.txt")]
        [DataRow("/etc/passwd")]
        public void Validate_RootedEntryPoint_Throws(string entryPoint)
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Runtime.Python.EntryPoint = entryPoint;

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().Throw<InvalidManifestException>().WithMessage("*entry_point*");
        }

        [TestMethod]
        [DataRow("run/main.py")]
        [DataRow("main.py")]
        [DataRow("nested/deeper/main.py")]
        public void Validate_WellFormedRelativeEntryPoint_DoesNotThrow(string entryPoint)
        {
            // Arrange
            Manifest manifest = CreateValidManifest();
            manifest.Runtime.Python.EntryPoint = entryPoint;

            // Act
            Action act = () => ManifestLoader.Validate(manifest);

            // Assert
            act.Should().NotThrow();
        }
    }
}
