namespace Assemblers.ProtocolTests.ScriptedConnectorManifest
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;

    [TestClass]
    public class ManifestTests
    {
        [TestMethod]
        public void Parse_ValidManifest_ReturnsExpectedValues()
        {
            // Arrange
            const string json = """
                {
                  "schema_version": "1.0",
                  "project": {
                    "id": "11111111-1111-1111-1111-111111111111",
                    "name": "MyConnector",
                    "version": "1.2.3",
                    "description": "Some description",
                    "author": "Skyline Communications"
                  },
                  "runtime": {
                    "language": "python",
                    "supported_platforms": ["x86_64-windows-msvc", "x86_64-linux-gnu"],
                    "python": {
                      "version": ">=3.10;<3.11",
                      "entry_point": "run/main.py"
                    }
                  }
                }
                """;

            // Act
            Manifest manifest = Manifest.Parse(json);

            // Assert
            manifest.SchemaVersion.Should().Be("1.0");
            manifest.Project.Id.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
            manifest.Project.Name.Should().Be("MyConnector");
            manifest.Project.Version.Should().Be("1.2.3");
            manifest.Runtime.Language.Should().Be(RuntimeLanguage.Python);
            manifest.Runtime.SupportedPlatforms.Should().BeEquivalentTo(new[] { SupportedPlatform.Windows, SupportedPlatform.Linux });
            manifest.Runtime.Python.Version.Should().Be(">=3.10;<3.11");
            manifest.Runtime.Python.EntryPoint.Should().Be("run/main.py");
        }

        [TestMethod]
        public void Parse_UnsupportedRuntimeLanguage_Throws()
        {
            // Arrange
            const string json = """
                {
                  "schema_version": "1.0",
                  "project": { "id": "11111111-1111-1111-1111-111111111111", "name": "MyConnector", "version": "1.0.0" },
                  "runtime": { "language": "powershell", "python": { "version": ">=3.10", "entry_point": "run/main.py" } }
                }
                """;

            // Act
            Action act = () => Manifest.Parse(json);

            // Assert
            act.Should().Throw<System.Text.Json.JsonException>();
        }

        [TestMethod]
        public void ToJson_RoundTrip_PreservesValues()
        {
            // Arrange
            var manifest = new Manifest
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
                    SupportedPlatforms = new[] { SupportedPlatform.Linux },
                    Python = new ManifestPython { Version = ">=3.10", EntryPoint = "run/main.py" },
                },
            };

            // Act
            string json = manifest.ToJson();
            Manifest roundTripped = Manifest.Parse(json);

            // Assert
            roundTripped.Should().BeEquivalentTo(manifest);
            json.Should().Contain("\"x86_64-linux-gnu\"");
            json.Should().Contain("\"python\"");
        }
    }
}
