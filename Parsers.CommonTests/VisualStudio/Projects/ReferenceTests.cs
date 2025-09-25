namespace Parsers.CommonTests.VisualStudio.Projects
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Parsers.Common.VisualStudio.Projects;

    [TestClass]
    public class ReferenceTests
    {
        [TestMethod]
        [DataRow(null, null, null)]
        [DataRow("MyReference", null, "MyReference.dll")]
        [DataRow("MyReference", "..\\MyHintPath.dll", "MyHintPath.dll")]
        [DataRow("Newtonsoft.Json, Version=11.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed, processorArchitecture=MSIL", null, "Newtonsoft.Json.dll")]
        public void Load_GeneralValid(string name, string hintPath, string expectedResult)
        {
            // Arrange
            var reference = new Reference(name, hintPath);

            // Act
            var result = reference.GetDllName();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}