namespace DMProtocolTests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;
    using Skyline.DataMiner.CICD.DMProtocol;

    [TestClass]
    public class PythonVersionConstraintTests
    {
        [TestMethod]
        [DataRow(">=3.14;<3.15", "3.14")]
        [DataRow(">=3.10", "3.10")]
        [DataRow(">=3.9.2;<3.10", "3.9.2")]
        [DataRow("<3.15;>=3.14", "3.14")]
        public void ExtractMinimumVersion_ValidConstraint_ReturnsMinimumVersion(string constraint, string expectedVersion)
        {
            // Act
            string version = PythonVersionConstraint.ExtractMinimumVersion(constraint);

            // Assert
            version.Should().Be(expectedVersion);
        }

        [TestMethod]
        public void ExtractMinimumVersion_NullOrWhitespace_Throws()
        {
            // Act
            Action act = () => PythonVersionConstraint.ExtractMinimumVersion("   ");

            // Assert
            act.Should().Throw<InvalidManifestException>();
        }

        [TestMethod]
        public void ExtractMinimumVersion_NoMinimumConstraint_Throws()
        {
            // Act
            Action act = () => PythonVersionConstraint.ExtractMinimumVersion("<3.15");

            // Assert
            act.Should().Throw<InvalidManifestException>();
        }
    }
}
