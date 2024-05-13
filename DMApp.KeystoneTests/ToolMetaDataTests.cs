using Microsoft.VisualStudio.TestTools.UnitTesting;

using Skyline.DataMiner.CICD.DMApp.Keystone;

namespace Skyline.DataMiner.CICD.DMApp.Keystone.Tests
{
    [TestClass()]
    public class ToolMetaDataTests
    {
        [TestMethod()]
        public void ToolMetaDataTest_ToolNameCommandCleanup_goodInput()
        {
            // Arrange

            // Act
            var result = new ToolMetaData("my-good-command", "Company.Testing.MyGoodCommand", "1.0.1", "JST", "Company", "SomeDir");

            // Assert
            Assert.AreEqual("Company.Testing.MyGoodCommand", result.ToolName);
            Assert.AreEqual("my-good-command", result.ToolCommand);
        }

        [TestMethod()]
        public void ToolMetaDataTest_ToolNameCommandCleanup_BadCommandInput_Spaces()
        {
            // Arrange

            // Act
            var result = new ToolMetaData("my good command", "Company.Testing.MyGoodCommand", "1.0.1", "JST", "Company", "SomeDir");

            // Assert
            Assert.AreEqual("Company.Testing.MyGoodCommand", result.ToolName);
            Assert.AreEqual("my-good-command", result.ToolCommand);
        }

        [TestMethod()]
        public void ToolMetaDataTest_ToolNameCommandCleanup_BadCommandInput_InvalidCharacters()
        {
            // Arrange

            // Act
            var result = new ToolMetaData("my** good''$ com//mand", "Company.Testing.MyGoodCommand", "1.0.1", "JST", "Company", "SomeDir");

            // Assert
            Assert.AreEqual("Company.Testing.MyGoodCommand", result.ToolName);
            Assert.AreEqual("my-good-command", result.ToolCommand);
        }

        [TestMethod()]
        public void ToolMetaDataTest_ToolNameCommandCleanup_BadCommandInput_UpperCasing()
        {
            // Arrange

            // Act
            var result = new ToolMetaData("MY good CoMManD", "Company.Testing.MyGoodCommand", "1.0.1", "JST", "Company", "SomeDir");

            // Assert
            Assert.AreEqual("Company.Testing.MyGoodCommand", result.ToolName);
            Assert.AreEqual("my-good-command", result.ToolCommand);
        }



        [TestMethod()]
        public void ToolMetaDataTest_ToolNameCommandCleanup_BadNameInput_Spaces()
        {
            // Arrange

            // Act
            var result = new ToolMetaData("my-good-command", "Company.Testing.My Good Command", "1.0.1", "JST", "Company", "SomeDir");

            // Assert
            Assert.AreEqual("Company.Testing.MyGoodCommand", result.ToolName);
            Assert.AreEqual("my-good-command", result.ToolCommand);
        }

        [TestMethod()]
        public void ToolMetaDataTest_ToolNameCommandCleanup_BadNameInput_InvalidCharacters()
        {
            // Arrange

            // Act
            var result = new ToolMetaData("my-good-command", "Company.Testing.My**Good\\@Com%%$mand", "1.0.1", "JST", "Company", "SomeDir");

            // Assert
            Assert.AreEqual("Company.Testing.MyGoodCommand", result.ToolName);
            Assert.AreEqual("my-good-command", result.ToolCommand);
        }
    }
}