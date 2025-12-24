namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public sealed class Matrix : IEquatable<Matrix>
	{
		public FunctionResource Input { get; set; }

		public FunctionResource Output { get; set; }

		public IReadOnlyCollection<FunctionResource> GetAllResources()
		{
			return new List<FunctionResource> { Input, Output }.Where(r => r != null).ToList();
		}

		public bool Equals(Matrix other)
		{
			if (other == null) return false;
			if (ReferenceEquals(this, other)) return true;

			bool inputsAreEqual = (Input is null && other.Input is null) || (Input != null && Input.Equals(other.Input));
			
			bool outputsAreEqual = (Output is null && other.Output is null) || (Output != null && Output.Equals(other.Output));

			return inputsAreEqual && outputsAreEqual;
		}
	}
}
