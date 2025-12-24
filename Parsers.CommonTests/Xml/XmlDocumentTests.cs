namespace Parsers.CommonTests.Xml
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    [TestClass]
    public class XmlDocumentTests
    {
        [TestMethod]
        public void XmlDocument_GetXml()
        {
            // Arrange
            const string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?><abc></abc>";
            const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?><abc />";

            XmlDocument document = XmlDocument.Parse(xml);

            // Act
            var result = document.GetXml();

            // Check
            result.Should().BeEquivalentTo(expectedXml);
        }
    }
}