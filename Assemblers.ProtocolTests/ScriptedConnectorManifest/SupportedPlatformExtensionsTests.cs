namespace Assemblers.ProtocolTests.ScriptedConnectorManifest
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Protocol.ScriptedConnectorManifest;

    [TestClass]
    public class SupportedPlatformExtensionsTests
    {
        [DataTestMethod]
        [DataRow(SupportedPlatform.Windows, "x86_64-windows-msvc")]
        [DataRow(SupportedPlatform.Linux, "x86_64-linux-gnu")]
        public void ToTargetTriple_KnownPlatform_ReturnsExpectedTriple(SupportedPlatform platform, string expectedTriple)
        {
            // Act
            string triple = platform.ToTargetTriple();

            // Assert
            triple.Should().Be(expectedTriple);
        }

        [DataTestMethod]
        [DataRow("x86_64-windows-msvc", SupportedPlatform.Windows)]
        [DataRow("x86_64-linux-gnu", SupportedPlatform.Linux)]
        public void FromTargetTriple_KnownTriple_ReturnsExpectedPlatform(string triple, SupportedPlatform expectedPlatform)
        {
            // Act
            SupportedPlatform platform = SupportedPlatformExtensions.FromTargetTriple(triple);

            // Assert
            platform.Should().Be(expectedPlatform);
        }

        [TestMethod]
        public void FromTargetTriple_UnknownTriple_Throws()
        {
            // Act
            Action act = () => SupportedPlatformExtensions.FromTargetTriple("aarch64-linux-gnu");

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}
