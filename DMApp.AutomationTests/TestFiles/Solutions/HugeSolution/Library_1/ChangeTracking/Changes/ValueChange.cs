namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.History
{
	using System;
	using Newtonsoft.Json;

	public sealed class ValueChange : IEquatable<ValueChange>
	{
		public ValueChange()
		{

		}

		public ValueChange(string oldValue, string newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}

		public string OldValue { get; set; }

		public string NewValue { get; set; }

		public bool Equals(ValueChange other)
		{
			return OldValue == other.OldValue && NewValue == other.NewValue;
		}
	}
}
