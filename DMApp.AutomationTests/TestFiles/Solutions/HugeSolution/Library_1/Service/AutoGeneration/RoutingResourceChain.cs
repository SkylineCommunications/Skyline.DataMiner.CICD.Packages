namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class RoutingResourceChain : IEquatable<RoutingResourceChain>
	{
		public RoutingResourceChain()
		{
			FirstMatrix = new Matrix();
			ConnectingMatrix = new Matrix();
			LastMatrix = new Matrix();
		}

		public bool IsValid { get; set; }

		public bool FirstMatrixRequired { get; set; }

		public Matrix FirstMatrix { get; set; }

		public bool ConnectingMatrixRequired { get; set; }

		public Matrix ConnectingMatrix { get; set; }

		public bool LastMatrixRequired { get; set; }

		public Matrix LastMatrix { get; set; }

		public int AmountOfHops => new[] { FirstMatrixRequired, ConnectingMatrixRequired, LastMatrixRequired }.Count(x => x);

		public int CombinedResourcePriority => AllResources.Any() ? AllResources.Select(r => r.GetPriority()).Sum() : Int32.MaxValue;

		public List<FunctionResource> AllResources
		{
			get
			{
				var allResources = new List<FunctionResource>();

				if (FirstMatrix != null) allResources.AddRange(FirstMatrix.GetAllResources());
				if (ConnectingMatrix != null) allResources.AddRange(ConnectingMatrix.GetAllResources());
				if (LastMatrix != null) allResources.AddRange(LastMatrix.GetAllResources());

				return allResources;
			}
		}

		public IReadOnlyCollection<Matrix> AllMatrices => new[] { FirstMatrix, ConnectingMatrix, LastMatrix };

		public int GetAmountOfChangesComparedTo(RoutingResourceChain other)
		{
			int amountOfChanges = 0;

			if (FirstMatrix?.Input?.GUID != other?.FirstMatrix?.Input?.GUID) amountOfChanges++;
			if (FirstMatrix?.Output?.GUID != other?.FirstMatrix?.Output?.GUID) amountOfChanges++;
			if (ConnectingMatrix?.Input?.GUID != other?.ConnectingMatrix?.Input?.GUID) amountOfChanges++;
			if (ConnectingMatrix?.Output?.GUID != other?.ConnectingMatrix?.Output?.GUID) amountOfChanges++;
			if (LastMatrix?.Input?.GUID != other?.LastMatrix?.Input?.GUID) amountOfChanges++;
			if (LastMatrix?.Output?.GUID != other?.LastMatrix?.Output?.GUID) amountOfChanges++;

			return amountOfChanges;
		}

		public int GetAmountOfOverlappingResourcesComparedTo(RoutingResourceChain other)
		{
			return AllResources.Intersect(other.AllResources).Count();
		}

		public int GetAmountOfOverlappingMatrixResourcePairsComparedTo(RoutingResourceChain other)
		{
			if (other is null) throw new ArgumentNullException(nameof(other));

			int amountOfOverlappingMatrixResourcePairs = 0;

			foreach (var matrix in AllMatrices)
			{
				if (!matrix.GetAllResources().Any()) continue; // empty matrices should not be considered for equality, these do not represent services

				foreach (var otherMatrix in other.AllMatrices)
				{
					if (matrix.Equals(otherMatrix)) amountOfOverlappingMatrixResourcePairs++;
				}
			}

			return amountOfOverlappingMatrixResourcePairs;
		}

		public bool UsesOneOrMoreSameMatrixResourcePairs(RoutingResourceChain other)
		{
			return GetAmountOfOverlappingMatrixResourcePairsComparedTo(other) > 0;
		}

		public bool Equals(RoutingResourceChain other)
		{
			if (other == null) return false;
			if (ReferenceEquals(this, other)) return true;

			bool equal = true;

			equal &= IsValid == other.IsValid;

			equal &= FirstMatrixRequired == other.FirstMatrixRequired;
			equal &= (FirstMatrix == null && other.FirstMatrix == null) || (FirstMatrix != null && FirstMatrix.Equals(other.FirstMatrix));

			equal &= ConnectingMatrixRequired == other.ConnectingMatrixRequired;
			equal &= (ConnectingMatrix == null && other.ConnectingMatrix == null) || (ConnectingMatrix != null && ConnectingMatrix.Equals(other.ConnectingMatrix));

			equal &= LastMatrixRequired == other.LastMatrixRequired;
			equal &= (LastMatrix == null && other.LastMatrix == null) || (LastMatrix != null && LastMatrix.Equals(other.LastMatrix));

			return equal;
		}

		public override string ToString()
		{
			var sb = new StringBuilder($"({(IsValid ? "Valid" : "Invalid")})");

			sb.Append($"[{FirstMatrix?.Input?.Name ?? Constants.None}]");
			sb.Append($"[{FirstMatrix?.Output?.Name ?? Constants.None}]");

			sb.Append(" ==> ");

			sb.Append($"[{ConnectingMatrix?.Input?.Name ?? Constants.None}]");
			sb.Append($"[{ConnectingMatrix?.Output?.Name ?? Constants.None}]");

			sb.Append(" ==> ");

			sb.Append($"[{LastMatrix?.Input?.Name ?? Constants.None}]");
			sb.Append($"[{LastMatrix?.Output?.Name ?? Constants.None}]");

			return sb.ToString();
		}
	}
}
