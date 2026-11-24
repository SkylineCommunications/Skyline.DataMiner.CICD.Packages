namespace Parsers.CommonTests.VisualStudio.Projects
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Assemblers.Common.VisualStudio.Projects;

    [TestClass]
    public class DataMinerProjectTypeConverterTests
    {
        [TestMethod]
        public void DataMinerProjectTypeConverter_ToEnum_WebApplication_ReturnsWebApplication()
        {
            // Act
            var result = DataMinerProjectTypeConverter.ToEnum("WebApplication");

            // Assert
            result.Should().Be(DataMinerProjectType.WebApplication);
        }

        [TestMethod]
        public void DataMinerProjectTypeConverter_ToString_WebApplication_ReturnsWebApplication()
        {
            // Act
            var result = DataMinerProjectTypeConverter.ToString(DataMinerProjectType.WebApplication);

            // Assert
            result.Should().Be("WebApplication");
        }

        [TestMethod]
        public void DataMinerProjectTypeConverter_ToEnum_UnknownValue_ReturnsUnknown()
        {
            // Act
            var result = DataMinerProjectTypeConverter.ToEnum("SomethingElse");

            // Assert
            result.Should().Be(DataMinerProjectType.Unknown);
        }

        [TestMethod]
        public void DataMinerProjectTypeConverter_ToEnum_Null_ReturnsNull()
        {
            // Act
            var result = DataMinerProjectTypeConverter.ToEnum(null);

            // Assert
            result.Should().BeNull();
        }
    }
}
