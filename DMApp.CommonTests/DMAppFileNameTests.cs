#pragma warning disable SA1201
#pragma warning disable SA1203
#pragma warning disable CA1806

namespace DMApp.CommonTests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.DMApp.Common;

    [TestClass]
    public class DMAppFileNameTests
    {
        private const string PackageName = "MyPackage";

        #region Constructor

        [TestMethod]
        public void Constructor_Valid()
        {
            // Arrange
            const string fileName = "RandomFile.dmapp";

            // Act
            var result = new DMAppFileName(fileName);

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().BeEquivalentTo(fileName);
        }

        [TestMethod]
        public void Constructor_ContainsFolder_ExpectedArgumentException()
        {
            // Arrange
            const string fileName = "RandomFolder\\RandomFile.dmapp";

            // Act
            Action act = () => new DMAppFileName(fileName);

            // Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("The specified file name can not have a directory structure.*fileName*");
        }

        [TestMethod]
        public void Constructor_NoExtension_ExpectedArgumentException()
        {
            // Arrange
            const string fileName = "RandomFile";

            // Act
            Action act = () => new DMAppFileName(fileName);

            // Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("The specified file name must end with '.dmapp'.*fileName*");
        }

        [TestMethod]
        public void Constructor_Null_ExpectedArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => new DMAppFileName(null);

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void Constructor_Invalid_ExpectedArgumentException(string fileName)
        {
            // Arrange

            // Act
            Action act = () => new DMAppFileName(fileName);

            // Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Value can not be empty or whitespace.*fileName*");
        }

        #endregion

        #region FromTag

        private const string Tag = "1.0.0.1";

        [TestMethod]
        public void FromTag_Valid()
        {
            // Arrange
            const string expectedResult = "MyPackage 1.0.0-CU1.dmapp";

            // Act
            var result = DMAppFileName.FromTag(PackageName, Tag);

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void FromTag_NullPackageName_ExpectedArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => DMAppFileName.FromTag(null, Tag);

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void FromTag_InvalidPackageName_ExpectedArgumentException(string packageNameToTest)
        {
            // Arrange

            // Act
            Action act = () => DMAppFileName.FromTag(packageNameToTest, Tag);

            // Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Value can not be empty or whitespace.*packageName*");
        }

        [TestMethod]
        public void FromTag_NullTag_ExpectedArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => DMAppFileName.FromTag(PackageName, null);

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void FromTag_InvalidTag_ExpectedArgumentException(string tagToTest)
        {
            // Arrange

            // Act
            Action act = () => DMAppFileName.FromTag(PackageName, tagToTest);

            // Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Value can not be empty or whitespace.*tag*");
        }

        #endregion

        #region FromBuildNumber

        private const string Branch = "MyBranch";
        private const int BuildNumber = 1;

        [TestMethod]
        public void FromBuildNumber_Valid()
        {
            // Arrange
            const string expectedResult = "MyPackage MyBranch_B1.dmapp";

            // Act
            var result = DMAppFileName.FromBuildNumber(PackageName, Branch, BuildNumber);

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void FromBuildNumber_NullPackageName_ExpectedArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => DMAppFileName.FromBuildNumber(null, Branch, BuildNumber);

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void FromBuildNumber_InvalidPackageName_ExpectedArgumentException(string packageNameToTest)
        {
            // Arrange

            // Act
            Action act = () => DMAppFileName.FromBuildNumber(packageNameToTest, Branch, BuildNumber);

            // Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Value can not be empty or whitespace.*packageName*");
        }

        [TestMethod]
        public void FromBuildNumber_NullBranch_ExpectedArgumentNullException()
        {
            // Arrange

            // Act
            Action act = () => DMAppFileName.FromBuildNumber(PackageName, null, BuildNumber);

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void FromBuildNumber_InvalidBranch_ExpectedArgumentException(string branchToTest)
        {
            // Arrange

            // Act
            Action act = () => DMAppFileName.FromBuildNumber(PackageName, branchToTest, BuildNumber);

            // Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Value can not be empty or whitespace.*branch*");
        }

        [TestMethod]
        [DataRow(0, "MyPackage MyBranch_B0.dmapp")]
        [DataRow(1, "MyPackage MyBranch_B1.dmapp")]
        [DataRow(UInt16.MaxValue, "MyPackage MyBranch_B65535.dmapp")]
        [DataRow(123456789, "MyPackage MyBranch_B123456789.dmapp")]
        [DataRow(Int32.MaxValue, "MyPackage MyBranch_B2147483647.dmapp")]
        public void FromBuildNumber_BuildNumber_IntValid(int buildNumberToTest, string expectedResult)
        {
            // Arrange

            // Act
            var result = DMAppFileName.FromBuildNumber(PackageName, Branch, buildNumberToTest);

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void FromBuildNumber_BuildNumber_Invalid_ExpectedArgumentException()
        {
            // Arrange

            // Act
            Action act = () => DMAppFileName.FromBuildNumber(PackageName, Branch, -1);

            // Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Value can not be lower than zero.*buildNumber*");
        }

        [TestMethod]
        [DataRow(0U, "MyPackage MyBranch_B0.dmapp")]
        [DataRow(1U, "MyPackage MyBranch_B1.dmapp")]
        [DataRow(UInt16.MaxValue, "MyPackage MyBranch_B65535.dmapp")]
        [DataRow(123456789U, "MyPackage MyBranch_B123456789.dmapp")]
        [DataRow(UInt32.MaxValue, "MyPackage MyBranch_B4294967295.dmapp")]
        public void FromBuildNumber_BuildNumber_UIntValid(uint buildNumberToTest, string expectedResult)
        {
            // Arrange

            // Act
            var result = DMAppFileName.FromBuildNumber(PackageName, Branch, buildNumberToTest);

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().BeEquivalentTo(expectedResult);
        }

        #endregion
    }
}