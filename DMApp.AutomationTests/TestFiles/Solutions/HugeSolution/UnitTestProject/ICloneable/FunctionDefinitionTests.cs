using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Function;

namespace UnitTestProject.ICloneable
{
	[TestClass]
	public class FunctionDefinitionTests
	{
		[TestMethod]
		public void Children_NotChanged()
		{
			var functionDefinition = new FunctionDefinition
			{
				Children = new List<Guid> { Guid.Empty }
			};

			var clone = functionDefinition.Clone() as FunctionDefinition;

			Assert.IsTrue(functionDefinition.Children.SequenceEqual(clone.Children));
		}

		[TestMethod]
		public void Children_Changed()
		{
			var functionDefinition = new FunctionDefinition
			{
				Children = new List<Guid>{ Guid.Empty }
			};

			var clone = functionDefinition.Clone() as FunctionDefinition;

			functionDefinition.Children.Add(Guid.NewGuid());

			Assert.IsFalse(functionDefinition.Children.SequenceEqual(clone.Children));
		}
	}
}
