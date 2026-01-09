namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History
{
	using System;

	public enum CollectionChangeType
	{
		None,
		Add,
		Remove,
	}

	public sealed class CollectionChange : IEquatable<CollectionChange>
	{
		public CollectionChangeType Type { get; set; }

		public string ItemIdentifier { get; set; }

		public string DisplayName { get; set; }

		public bool Equals(CollectionChange other)
		{
			return Type == other.Type && ItemIdentifier == other.ItemIdentifier && DisplayName == other.DisplayName;
		}
	}
}
