namespace UnitTestProject.ChangeTracking.NewsInformation
{
	using System.Linq;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;

	[TestClass]
	public class NewsInformationTests
	{
		[TestMethod]
		public void TestMethod1()
		{
			var newsInformation = new NewsInformation
			{
				NewsCameraOperator = "test",
				Journalist = "test",
				VirveCommandGroupOne = "test",
				VirveCommandGroupTwo = "test",
				AdditionalInformation = "test"
			};

			newsInformation.AcceptChanges(null);

			var change = newsInformation.Change;

			Assert.IsFalse(change.Summary.IsChanged);
		}

		[TestMethod]
		public void TestMethod4()
		{
			var newsInformation = new NewsInformation
			{
				NewsCameraOperator = "test",
				Journalist = "test",
				VirveCommandGroupOne = "test",
				VirveCommandGroupTwo = "test",
				AdditionalInformation = "test"
			};

			newsInformation.AcceptChanges(null);

			var change = newsInformation.Change as ClassChange;

			Assert.IsFalse(change.PropertyChanges.Any());
		}

		[TestMethod]
		public void TestMethod2()
		{
			var newsInformation = new NewsInformation
			{
				NewsCameraOperator = "test",
				Journalist = "test",
				VirveCommandGroupOne = "test",
				VirveCommandGroupTwo = "test",
				AdditionalInformation = "test"
			};

			newsInformation.AcceptChanges(null);

			newsInformation.AdditionalInformation = "new value";

			var change = newsInformation.Change;

			Assert.IsTrue(change.Summary.IsChanged);
		}

		[TestMethod]
		public void TestMethod3()
		{
			var newsInformation = new NewsInformation
			{
				NewsCameraOperator = "test",
				Journalist = "test",
				VirveCommandGroupOne = "test",
				VirveCommandGroupTwo = "test",
				AdditionalInformation = "test"
			};

			newsInformation.AcceptChanges(null);

			newsInformation.AdditionalInformation = "new value";

			var change = newsInformation.Change as ClassChange;

			Assert.IsTrue(change.PropertyChanges.Any(pc => pc.PropertyName == nameof(NewsInformation.AdditionalInformation) && pc.Change.OldValue == "test" && pc.Change.NewValue == "new value"));
		}

		[TestMethod]
		public void TestMethod5()
		{
			var newsInformation = new NewsInformation
			{
				NewsCameraOperator = "value1",
				Journalist = "value1",
				VirveCommandGroupOne = "value1",
				VirveCommandGroupTwo = "value1",
				AdditionalInformation = "value1"
			};

			var other = new NewsInformation
			{
				NewsCameraOperator = "value1",
				Journalist = "value1",
				VirveCommandGroupOne = "value1",
				VirveCommandGroupTwo = "value1",
				AdditionalInformation = "value1"
			};

			var change = newsInformation.GetChangeComparedTo(null, other);

			Assert.IsFalse(change.Summary.IsChanged);
		}

		[TestMethod]
		public void TestMethod6()
		{
			var newsInformation = new NewsInformation
			{
				NewsCameraOperator = "value1",
				Journalist = "value1",
				VirveCommandGroupOne = "value1",
				VirveCommandGroupTwo = "value1",
				AdditionalInformation = "value1"
			};

			var other = new NewsInformation
			{
				NewsCameraOperator = "value2",
				Journalist = "value2",
				VirveCommandGroupOne = "value2",
				VirveCommandGroupTwo = "value2",
				AdditionalInformation = "value2"
			};

			var change = newsInformation.GetChangeComparedTo(null, other);

			Assert.IsTrue(change.Summary.IsChanged);
		}
	}
}
