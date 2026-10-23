namespace Assemblers.ProtocolTests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Protocol;
    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;

    [TestClass]
    public class PythonVersionConstraintTests
    {
        [TestMethod]
        [DataRow(">=3.14;<3.15", "3.14")]
        [DataRow(">=3.9.2;<3.10", "3.9.2")]
        [DataRow("<3.15;>=3.14", "3.14")]
        [DataRow(">=3.10;<=3.10.9", "3.10")]
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

        [TestMethod]
        public void ExtractMinimumVersion_NoUpperBound_Throws()
        {
            // Act
            Action act = () => PythonVersionConstraint.ExtractMinimumVersion(">=3.10");

            // Assert
            act.Should().Throw<InvalidManifestException>();
        }

        [TestMethod]
        public void ExtractMinimumVersion_UpperBoundSpansMultipleMinorVersions_Throws()
        {
            // Act
            Action act = () => PythonVersionConstraint.ExtractMinimumVersion(">=3.10;<=3.12");

            // Assert
            act.Should().Throw<InvalidManifestException>();
        }

        [TestMethod]
        [DataRow(">=3.14;<3.14")]
        [DataRow(">=3.14.5;<=3.14.2")]
        [DataRow(">=3.14;<3.15;<3.14")]
        public void ExtractMinimumVersion_ContradictoryUpperBound_Throws(string constraint)
        {
            // Act
            Action act = () => PythonVersionConstraint.ExtractMinimumVersion(constraint);

            // Assert
            act.Should().Throw<InvalidManifestException>();
        }

        [TestMethod]
        [DataRow(">=3.14;<3.15;!=3.14.*")]
        [DataRow(">=3.14;>3.14.5;<3.15")]
        public void ExtractMinimumVersion_UnsupportedClause_Throws(string constraint)
        {
            // Act
            Action act = () => PythonVersionConstraint.ExtractMinimumVersion(constraint);

            // Assert
            act.Should().Throw<InvalidManifestException>();
        }
    }
}
