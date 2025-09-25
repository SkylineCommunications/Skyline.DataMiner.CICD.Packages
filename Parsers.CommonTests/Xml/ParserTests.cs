namespace Parsers.CommonTests.Xml
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    [TestClass]
    public class XmlParser
    {
        [TestMethod]
        public void XmlParser_Simple()
        {
            // Arrange
            string xml = "<A b=\"c\">x</A>";

            // Act
            var parser = new Parser(xml);

            // Check
            var document = parser.Document;
            var a = document.Element["a"];
            Assert.IsNotNull(a);
            Assert.AreEqual("x", a.InnerText);
            Assert.AreEqual(14, a.TotalLength);

            var b = a.Attribute["b"];
            Assert.IsNotNull(b);
            Assert.AreEqual("c", b.Value);
            Assert.AreEqual(1, b.ValueLength);
            Assert.AreEqual(5, b.TotalLength);
        }

        [TestMethod]
        public void XmlParser_SpecialCharactersInElementValue()
        {
            // Arrange
            string xml = "<A>x &amp; y</A>";

            // Act
            var parser = new Parser(xml);

            // Check
            var document = parser.Document;
            var a = document.Element["a"];
            Assert.IsNotNull(a);
            Assert.AreEqual("x & y", a.InnerText);
            Assert.AreEqual(16, a.TotalLength);
        }

        [TestMethod]
        public void XmlParser_SpecialCharactersInAttributeValue()
        {
            // Arrange
            string xml = "<A b=\"x &amp; y\" />";

            // Act
            var parser = new Parser(xml);

            // Check
            var document = parser.Document;
            var a = document.Element["a"];
            Assert.IsNotNull(a);
            Assert.AreEqual(String.Empty, a.InnerText);
            Assert.AreEqual(19, a.TotalLength);

            var b = a.Attribute["b"];
            Assert.IsNotNull(b);
            Assert.AreEqual("x & y", b.Value);
            Assert.AreEqual(9, b.ValueLength);
            Assert.AreEqual(13, b.TotalLength);
        }

        [TestMethod]
        public void XmlParser_CDataShouldNotBeDecoded()
        {
            // Arrange
            string xml = "<A><![CDATA[x &amp; y]]></A>";

            // Act
            var parser = new Parser(xml);

            // Check
            var document = parser.Document;
            var a = document.Element["a"];
            Assert.IsNotNull(a);
            Assert.AreEqual("x &amp; y", a.InnerText);
            Assert.AreEqual(28, a.TotalLength);

            var cdata = a.Children.OfType<XmlCDATA>().FirstOrDefault();
            Assert.IsNotNull(cdata);
            Assert.AreEqual("x &amp; y", cdata.InnerText);
            Assert.AreEqual(21, cdata.TotalLength);
        }

        [TestMethod]
        public void XmlParser_CommentShouldNotBeDecoded()
        {
            // Arrange
            string xml = "<A><!-- x &amp; y --></A>";

            // Act
            var parser = new Parser(xml);

            // Check
            var document = parser.Document;
            var a = document.Element["a"];
            Assert.IsNotNull(a);
            Assert.AreEqual(String.Empty, a.InnerText);
            Assert.AreEqual(25, a.TotalLength);

            var comment = a.Children.OfType<XmlComment>().FirstOrDefault();
            Assert.IsNotNull(comment);
            Assert.AreEqual(" x &amp; y ", comment.InnerText);
            Assert.AreEqual(18, comment.TotalLength);
        }

        [TestMethod]
        public void XmlParser_IgnoreCommentInValue()
        {
            // Arrange
            string xml = "<A>x<!--comment--></A>";

            // Act
            var parser = new Parser(xml);

            // Check
            var document = parser.Document;
            var a = document.Element["a"];
            Assert.IsNotNull(a);
            Assert.AreEqual(22, a.TotalLength);
            Assert.AreEqual("x", a.InnerText);
        }

        [TestMethod]
        public void XmlParser_Parse_Insert()
        {
            // Arrange
            const string xml = @"<a><b id=""1"" /><b id=""3"" /></a>";
            var parser = new Parser(xml);

            // Act
            parser.CharsInserted(15, @"<b id=""2"" />");

            // Check
            var document = parser.Document;
            var a = document.Element["a"];
            Assert.AreEqual(3, a.Children.Count);
            Assert.AreEqual(null, a.InnerText);

            var b = a.Elements["b"].ToList();

            Assert.AreEqual(String.Empty, b[0].InnerText);
            Assert.AreEqual("1", b[0].Attribute["id"].Value);

            Assert.AreEqual(String.Empty, b[1].InnerText);
            Assert.AreEqual("2", b[1].Attribute["id"].Value);

            Assert.AreEqual(String.Empty, b[2].InnerText);
            Assert.AreEqual("3", b[2].Attribute["id"].Value);
        }

        [TestMethod]
        public void XmlParser_Parse_Remove()
        {
            // Arrange
            const string xml = @"<a><b id=""1"" /><b id=""2"" /><b id=""3"" /></a>";
            var parser = new Parser(xml);

            // Act
            parser.CharsRemoved(15, 12);

            // Check
            var document = parser.Document;
            var a = document.Element["a"];
            Assert.AreEqual(2, a.Children.Count);
            Assert.AreEqual(null, a.InnerText);

            var b = a.Elements["b"].ToList();

            Assert.AreEqual(String.Empty, b[0].InnerText);
            Assert.AreEqual("1", b[0].Attribute["id"].Value);

            Assert.AreEqual(String.Empty, b[1].InnerText);
            Assert.AreEqual("3", b[1].Attribute["id"].Value);
        }
    }
}
