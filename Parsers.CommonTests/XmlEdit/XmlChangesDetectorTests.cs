namespace Parsers.CommonTests.XmlEdit
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Skyline.DataMiner.CICD.Parsers.Common.Xml;
    using EditXml = Skyline.DataMiner.CICD.Parsers.Common.XmlEdit;

    [TestClass]
    public class XmlChangesDetector
    {
        #region Elements

        [TestMethod]
        public void XmlChangesDetector_AddElement_ElementIsAdded()
        {
            // Arrange.
            string xml = @"<A></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.Children.Add(new EditXml.XmlElement("B"));

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(3, single.NewPosition);
            Assert.AreEqual("<B />", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_AddElementSelfContained_ElementIsAdded()
        {
            // Arrange.
            string xml = @"<A />";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.Children.Add(new EditXml.XmlElement("B"));

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            Assert.HasCount(3, changes);

            var change1 = changes[0];
            Assert.AreEqual(EditXml.ChangeType.Deletion, change1.Type);
            Assert.AreEqual(2, change1.OldPosition);
            Assert.AreEqual(2, change1.OldLength);

            var change2 = changes[1];
            Assert.AreEqual(EditXml.ChangeType.Insertion, change2.Type);
            Assert.AreEqual(5, change2.NewPosition);
            Assert.AreEqual("<B />", change2.NewText);

            var change3 = changes[2];
            Assert.AreEqual(EditXml.ChangeType.Insertion, change3.Type);
            Assert.AreEqual(5, change3.NewPosition);
            Assert.AreEqual("</A>", change3.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_InsertElement_ElementIsAdded()
        {
            // Arrange.
            string xml = @"<A><B /><D /></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.Children.Insert(1, new EditXml.XmlElement("C"));

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(8, single.NewPosition);
            Assert.AreEqual("<C />", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_InsertBeforeElement_ElementIsAdded()
        {
            // Arrange.
            string xml = @"<A><B /><D /></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var d = a.Element["D"];
            a.Children.InsertBefore(d, new EditXml.XmlElement("C"));

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(8, single.NewPosition);
            Assert.AreEqual("<C />", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_InsertAfterElement_ElementIsAdded()
        {
            // Arrange.
            string xml = @"<A><B /><D /></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var b = a.Element["B"];
            a.Children.InsertAfter(b, new EditXml.XmlElement("C"));

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(8, single.NewPosition);
            Assert.AreEqual("<C />", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_RemoveElement_ElementIsRemoved()
        {
            // Arrange.
            string xml = @"<A><B></B></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var b = a.Element["B"];
            a.Children.Remove(b);

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Deletion, single.Type);
            Assert.AreEqual(3, single.OldPosition);
            Assert.AreEqual(7, single.OldLength);
        }

        [TestMethod]
        public void XmlChangesDetector_RenameElement_ElementIsRenamed()
        {
            // Arrange.
            string xml = @"<A><B></B></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var b = a.Element["B"];
            b.Name = "C";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            Assert.HasCount(2, changes);

            var change1 = changes[0];
            Assert.AreEqual(EditXml.ChangeType.Replace, change1.Type);
            Assert.AreEqual(4, change1.OldPosition);
            Assert.AreEqual(1, change1.OldLength);
            Assert.AreEqual(4, change1.NewPosition);
            Assert.AreEqual("C", change1.NewText);

            var change2 = changes[1];
            Assert.AreEqual(EditXml.ChangeType.Replace, change2.Type);
            Assert.AreEqual(8, change2.OldPosition);
            Assert.AreEqual(1, change2.OldLength);
            Assert.AreEqual(8, change2.NewPosition);
            Assert.AreEqual("C", change2.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_RenameElementSelfContained_ElementIsRenamed()
        {
            // Arrange.
            string xml = @"<A><B /></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var b = a.Element["B"];
            b.Name = "C";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Replace, single.Type);
            Assert.AreEqual(4, single.OldPosition);
            Assert.AreEqual(1, single.OldLength);
            Assert.AreEqual(4, single.NewPosition);
            Assert.AreEqual("C", single.NewText);
        }

        #endregion

        #region Attributes

        [TestMethod]
        public void XmlChangesDetector_AddAttribute_AttributeIsAdded()
        {
            // Arrange.
            string xml = @"<A></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.Attributes.Add("b", "c");

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(2, single.NewPosition);
            Assert.AreEqual(" b=\"c\"", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_AddAttributeWithSpecialCharacters_AttributeIsAdded()
        {
            // Arrange.
            string xml = @"<A></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.Attributes.Add("b", "c & d");

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(2, single.NewPosition);
            Assert.AreEqual(" b=\"c &amp; d\"", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_InsertAttribute_AttributeIsAdded()
        {
            // Arrange.
            string xml = "<A b=\"b\" d=\"d\"></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.Attributes.Insert(1, new EditXml.XmlAttribute("c", "c"));

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(8, single.NewPosition);
            Assert.AreEqual(" c=\"c\"", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_RemoveAttribute_AttributeIsRemoved()
        {
            // Arrange.
            string xml = @"<A b='c'></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var b = a.Attribute["b"];
            a.Attributes.Remove(b);

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Deletion, single.Type);
            Assert.AreEqual(2, single.OldPosition);
            Assert.AreEqual(6, single.OldLength);
        }

        [TestMethod]
        public void XmlChangesDetector_UpdateAttributeName_AttributeIsUpdated()
        {
            // Arrange.
            string xml = @"<A b='c'></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var b = a.Attribute["b"];
            b.Name = "d";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Replace, single.Type);
            Assert.AreEqual(3, single.OldPosition);
            Assert.AreEqual(1, single.OldLength);
            Assert.AreEqual(3, single.NewPosition);
            Assert.AreEqual("d", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_UpdateAttributeValue_AttributeIsUpdated()
        {
            // Arrange.
            string xml = @"<A b='c'></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var b = a.Attribute["b"];
            b.Value = "d";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Replace, single.Type);
            Assert.AreEqual(6, single.OldPosition);
            Assert.AreEqual(1, single.OldLength);
            Assert.AreEqual(6, single.NewPosition);
            Assert.AreEqual("d", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_UpdateAttributeValueWithSpecialCharacters_AttributeIsUpdated()
        {
            // Arrange.
            string xml = @"<A b='c'></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var b = a.Attribute["b"];
            b.Value = "d & e";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Replace, single.Type);
            Assert.AreEqual(6, single.OldPosition);
            Assert.AreEqual(1, single.OldLength);
            Assert.AreEqual(6, single.NewPosition);
            Assert.AreEqual("d &amp; e", single.NewText);
        }

        #endregion

        #region Content

        [TestMethod]
        public void XmlChangesDetector_AddContent_ContentIsAdded()
        {
            // Arrange.
            string xml = @"<A></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.InnerText = "b";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(3, single.NewPosition);
            Assert.AreEqual("b", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_AddContentWithSpecialCharacters_ContentIsAdded()
        {
            // Arrange.
            string xml = @"<A></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.InnerText = "b & c";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(3, single.NewPosition);
            Assert.AreEqual("b &amp; c", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_UpdateContent_ContentIsUpdated()
        {
            // Arrange.
            string xml = @"<A>b</A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.InnerText = "c";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Replace, single.Type);
            Assert.AreEqual(3, single.OldPosition);
            Assert.AreEqual(1, single.OldLength);
            Assert.AreEqual(3, single.NewPosition);
            Assert.AreEqual("c", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_UpdateContentWithSpecialCharacters_ContentIsUpdated()
        {
            // Arrange.
            string xml = @"<A>b</A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.InnerText = "c & d";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Replace, single.Type);
            Assert.AreEqual(3, single.OldPosition);
            Assert.AreEqual(1, single.OldLength);
            Assert.AreEqual(3, single.NewPosition);
            Assert.AreEqual("c &amp; d", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_UpdateContentWithEmptyString_ContentIsUpdated()
        {
            // Arrange.
            string xml = @"<A>b</A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.InnerText = String.Empty;

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Replace, single.Type);
            Assert.AreEqual(3, single.OldPosition);
            Assert.AreEqual(1, single.OldLength);
            Assert.AreEqual(3, single.NewPosition);
            Assert.AreEqual(String.Empty, single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_RemoveContent_ContentIsRemoved()
        {
            // Arrange.
            string xml = @"<A>b</A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.InnerText = null;

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Deletion, single.Type);
            Assert.AreEqual(3, single.OldPosition);
            Assert.AreEqual(1, single.OldLength);
        }

        #endregion

        #region CData

        [TestMethod]
        public void XmlChangesDetector_AddCData_CDataIsAdded()
        {
            // Arrange.
            string xml = @"<A></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.Children.Add(new EditXml.XmlCDATA("b"));

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(3, single.NewPosition);
            Assert.AreEqual("<![CDATA[b]]>", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_AddCDataWithSpecialCharacters_CDataIsAdded()
        {
            // Arrange.
            string xml = @"<A></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.Children.Add(new EditXml.XmlCDATA("b & c"));

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(3, single.NewPosition);
            Assert.AreEqual("<![CDATA[b & c]]>", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_UpdateCData_CDataIsUpdated()
        {
            // Arrange.
            string xml = @"<A><![CDATA[b]]></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var cdata = a.Children.OfType<EditXml.XmlCDATA>().First();
            cdata.InnerText = "c";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Replace, single.Type);
            Assert.AreEqual(12, single.OldPosition);
            Assert.AreEqual(1, single.OldLength);
            Assert.AreEqual(12, single.NewPosition);
            Assert.AreEqual("c", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_UpdateCDataWithSpecialCharacters_CDataIsUpdated()
        {
            // Arrange.
            string xml = @"<A><![CDATA[b]]></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var cdata = a.Children.OfType<EditXml.XmlCDATA>().First();
            cdata.InnerText = "c & d";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Replace, single.Type);
            Assert.AreEqual(12, single.OldPosition);
            Assert.AreEqual(1, single.OldLength);
            Assert.AreEqual(12, single.NewPosition);
            Assert.AreEqual("c & d", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_RemoveCData_CDataIsRemoved()
        {
            // Arrange.
            string xml = @"<A><![CDATA[b]]></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var cdata = a.Children.OfType<EditXml.XmlCDATA>().First();
            a.Children.Remove(cdata);

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Deletion, single.Type);
            Assert.AreEqual(3, single.OldPosition);
            Assert.AreEqual(13, single.OldLength);
        }

        #endregion

        #region Comments

        [TestMethod]
        public void XmlChangesDetector_AddComment_CommentIsAdded()
        {
            // Arrange.
            string xml = @"<A></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.Children.Add(new EditXml.XmlComment("b"));

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(3, single.NewPosition);
            Assert.AreEqual("<!--b-->", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_AddCommentWithSpecialCharacters_CommentIsAdded()
        {
            // Arrange.
            string xml = @"<A></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            a.Children.Add(new EditXml.XmlComment("b & c"));

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(3, single.NewPosition);
            Assert.AreEqual("<!--b & c-->", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_UpdateComment_CommentIsUpdated()
        {
            // Arrange.
            string xml = @"<A><!--b--></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var comment = a.Children.OfType<EditXml.XmlComment>().First();
            comment.InnerText = "c";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Replace, single.Type);
            Assert.AreEqual(7, single.OldPosition);
            Assert.AreEqual(1, single.OldLength);
            Assert.AreEqual(7, single.NewPosition);
            Assert.AreEqual("c", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_UpdateCommentWithSpecialCharacters_CommentIsUpdated()
        {
            // Arrange.
            string xml = @"<A><!--b--></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var comment = a.Children.OfType<EditXml.XmlComment>().First();
            comment.InnerText = "c & d";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Replace, single.Type);
            Assert.AreEqual(7, single.OldPosition);
            Assert.AreEqual(1, single.OldLength);
            Assert.AreEqual(7, single.NewPosition);
            Assert.AreEqual("c & d", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_RemoveComment_CommentIsRemoved()
        {
            // Arrange.
            string xml = @"<A><!--b--></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var a = xmlEdit.Element["A"];
            var comment = a.Children.OfType<EditXml.XmlComment>().First();
            a.Children.Remove(comment);

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Deletion, single.Type);
            Assert.AreEqual(3, single.OldPosition);
            Assert.AreEqual(8, single.OldLength);
        }

        #endregion

        #region Declaration

        [TestMethod]
        public void XmlChangesDetector_AddDeclaration_DeclarationIsAdded()
        {
            // Arrange.
            string xml = @"<A></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            xmlEdit.Declaration = new EditXml.XmlDeclaration("1.0", "UTF-8", "no");

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Insertion, single.Type);
            Assert.AreEqual(0, single.NewPosition);
            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_UpdateDeclaration_DeclarationIsUpdated()
        {
            // Arrange.
            string xml = "<?xml version=\"1.0\" ?><A></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var declaration = xmlEdit.Declaration;
            declaration.Version = "1.1";

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Replace, single.Type);
            Assert.AreEqual(15, single.OldPosition);
            Assert.AreEqual(3, single.OldLength);
            Assert.AreEqual(15, single.NewPosition);
            Assert.AreEqual("1.1", single.NewText);
        }

        [TestMethod]
        public void XmlChangesDetector_RemoveDeclaration_DeclarationIsRemoved()
        {
            // Arrange.
            string xml = "<?xml version=\"1.0\" ?><A></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);
            var declaration = xmlEdit.Declaration;
            xmlEdit.Declaration = null;

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            var single = changes.Single();

            Assert.AreEqual(EditXml.ChangeType.Deletion, single.Type);
            Assert.AreEqual(0, single.OldPosition);
            Assert.AreEqual(22, single.OldLength);
        }

        #endregion

        #region Other

        [TestMethod]
        public void XmlChangesDetector_NoChanges_NoChangesDetected()
        {
            // Arrange.
            string xml = @"<A b='c'></A>";
            Parser parser = new Parser(xml);

            // Act.
            var xmlEdit = new EditXml.XmlDocument(parser.Document);

            // Assert
            var changes = EditXml.XmlChangesDetector.GetChanges(xml, xmlEdit);
            Assert.IsEmpty(changes);
        }

        #endregion
    }
}
