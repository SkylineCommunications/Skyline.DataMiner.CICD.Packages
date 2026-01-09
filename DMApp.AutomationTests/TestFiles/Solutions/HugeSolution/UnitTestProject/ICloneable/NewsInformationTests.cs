using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;

namespace UnitTestProject.ICloneable
{
	[TestClass]
	public class NewsInformationTests
	{
		[TestMethod]
		public void NewsInformation_Clone()
		{
			var newsInformation = new NewsInformation
			{
				AdditionalInformation = "test",
				Journalist = "test",
				NewsCameraOperator = "test",
				VirveCommandGroupOne = "test",
				VirveCommandGroupTwo = "test",
			};

			var clone = newsInformation.Clone() as NewsInformation;

			Assert.AreEqual(newsInformation.AdditionalInformation, clone.AdditionalInformation);
		}
	}
}
