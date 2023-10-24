namespace DMApp.CommonTests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.CICD.DMApp.Common;

    [TestClass]
    public class DMAppVersionTests
    {
        #region FromDataMinerVersion

        [TestMethod]
        public void FromDataMinerVersion_HappyPathNoCu()
        {
            // Act
            var result = DMAppVersion.FromDataMinerVersion("1.0.1");

            // Assert
            Assert.AreEqual("1.0.1", result.ToString());
        }

        [TestMethod]
        public void FromDataMinerVersion_HappyPathCU()
        {
            // Act
            var result = DMAppVersion.FromDataMinerVersion("1.0.1-CU10");

            // Assert
            Assert.AreEqual("1.0.1-CU10", result.ToString());
        }

        [TestMethod]
        public void FromDataMinerVersion_BadVersion()
        {
            // Act
            Action act = () => DMAppVersion.FromDataMinerVersion("1.0.1.1");

            // Assert
            act.Should().ThrowExactly<FormatException>().WithMessage("The specified version has an invalid format.");
        }

        [TestMethod]
        public void FromDataMinerVersion_BadCU_Spaces()
        {
            // Act
            Action act = () => DMAppVersion.FromDataMinerVersion("1.0.1 - CU1");

            // Assert
            act.Should().ThrowExactly<FormatException>().WithMessage("The specified version has an invalid CU Format. Expected \"-CU[0-9]+\" ");
        }

        [TestMethod]
        public void FromDataMinerVersion_BadCU_NaN()
        {
            // Act
            Action act = () => DMAppVersion.FromDataMinerVersion("1.0.1-10Abc");

            // Assert
            act.Should().ThrowExactly<FormatException>().WithMessage("The specified version has an invalid CU Format. Expected \"-CU[0-9]+\" ");
        }

        [TestMethod]
        public void FromDataMinerVersion_Null_ExpectedArgumentNullException()
        {
            // Act
            Action act = () => DMAppVersion.FromDataMinerVersion(null);

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void FromDataMinerVersion_Empty_ExpectedArgumentException(string input)
        {
            // Act
            Action act = () => DMAppVersion.FromDataMinerVersion(input);

            // Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("The specified version can not be empty or whitespace*dmaVersion*");
        }

        #endregion

        #region FromBuildNumber

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(UInt16.MaxValue)]
        [DataRow(123456789)]
        [DataRow(Int32.MaxValue)]
        public void FromBuildNumber_Valid_Int(int buildNumber)
        {
            // Arrange
            string expectedResult = "0.0.0-CU" + buildNumber;

            // Act
            var result = DMAppVersion.FromBuildNumber(buildNumber);

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        [DataRow(0U)]
        [DataRow(1U)]
        [DataRow(UInt16.MaxValue)]
        [DataRow(123456789U)]
        [DataRow(UInt32.MaxValue)]
        public void FromBuildNumber_Valid_UInt(uint buildNumber)
        {
            // Arrange
            string expectedResult = "0.0.0-CU" + buildNumber;

            // Act
            var result = DMAppVersion.FromBuildNumber(buildNumber);

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void FromBuildNumber_Invalid_ExpectedArgumentException()
        {
            // Arrange

            // Act
            Action act = () => DMAppVersion.FromBuildNumber(-1);

            // Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("Value can not be lower than zero.*buildNumber*");
        }

        #endregion

        #region FromProtocolVersion

        [TestMethod]
        public void FromProtocolVersion_Valid()
        {
            // Arrange
            const string expectedResult = "1.0.0-CU1";

            // Act
            var result = DMAppVersion.FromProtocolVersion("1.0.0.1");

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void FromProtocolVersion_Invalid_BuildProtocol_ExpectedFormatException()
        {
            // Act
            Action act = () => DMAppVersion.FromProtocolVersion("1.0.0.1_B1");

            // Assert
            act.Should().ThrowExactly<FormatException>().WithMessage("The specified version has an invalid format.");
        }

        [TestMethod]
        public void FromProtocolVersion_Invalid_Random_ExpectedFormatException()
        {
            // Act
            Action act = () => DMAppVersion.FromProtocolVersion("ABC");

            // Assert
            act.Should().ThrowExactly<FormatException>().WithMessage("The specified version has an invalid format.");
        }

        [TestMethod]
        public void FromProtocolVersion_Null_ExpectedArgumentNullException()
        {
            // Act
            Action act = () => DMAppVersion.FromProtocolVersion(null);

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void FromProtocolVersion_Empty_ExpectedArgumentException(string input)
        {
            // Act
            Action act = () => DMAppVersion.FromProtocolVersion(input);

            // Assert
            act.Should().ThrowExactly<ArgumentException>().WithMessage("The specified version can not be empty or whitespace*protocolVersion*");
        }

        #endregion
    }
}