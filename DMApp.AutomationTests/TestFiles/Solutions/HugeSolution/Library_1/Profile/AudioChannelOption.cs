namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
    using System;
	using System.Collections.Generic;
	using System.Text;

	public class AudioChannelOption
	{
		private AudioChannelOption()
		{
		}

		public int SourceChannelId { get; private set; }

		public string DisplayValue { get; private set; }

		public bool IsStereo { get; private set; }

		public string Value { get; private set; }

		public string OtherDescription { get; private set; }

		public bool OriginatesFromSource { get; private set; }

		public bool IsOther => String.Equals(Value, "Other", StringComparison.InvariantCultureIgnoreCase);

		public bool IsDolby => Value.StartsWith("Dolby", StringComparison.InvariantCultureIgnoreCase);

		public bool IsNone => String.Equals(Value, "None", StringComparison.InvariantCultureIgnoreCase);

		public static AudioChannelOption FromSource(int channelId, bool isStereo, string value, string otherDescription)
		{
			string displayValue = GenerateDisplayValue(channelId, isStereo, value, otherDescription);
			return new AudioChannelOption()
            {
				DisplayValue = displayValue,
				Value = value,
				IsStereo = isStereo,
				OtherDescription = otherDescription,
				OriginatesFromSource = true,
				SourceChannelId = channelId
			};
		}

		public static AudioChannelOption None()
		{
			return FromDiscreet(Constants.None, String.Empty);
		}

		public static AudioChannelOption FromDiscreet(string value, string description)
		{
			return new AudioChannelOption()
            {
				DisplayValue = value,
				Value = value,
				IsStereo = false,
				OtherDescription = description,
				OriginatesFromSource = false,
				SourceChannelId = -1
			};
		}

		public override bool Equals(object obj)
		{
			if (!(obj is AudioChannelOption otherOption)) return false;
			if (!OriginatesFromSource)
			{
				return String.Equals(Value, otherOption.Value);
			}
			else
            {
				bool stereoMatches = IsStereo == otherOption.IsStereo;
				bool valueMatches = String.Equals(Value, otherOption.Value);
				bool channelIdMatches = SourceChannelId == otherOption.SourceChannelId;
				bool descriptionMatches = String.Equals(OtherDescription, otherOption.OtherDescription);

				return stereoMatches && valueMatches && channelIdMatches && descriptionMatches;
            }
		}

        public override int GetHashCode()
        {
			if (!OriginatesFromSource)
			{
				return Value.GetHashCode();
			}
			else
			{
				int steroHash = IsStereo.GetHashCode() * 13;
				int valueHash = Value.GetHashCode() * 17;
				int channelIdHash = SourceChannelId.GetHashCode() * 19;

				return steroHash + valueHash + channelIdHash;
			}
		}

        public override string ToString()
        {
			return $"AudioChannelOption|DisplayValue: {DisplayValue}, Internal Value: {Value}, Other Description: {OtherDescription}, IsStereo: {IsStereo}, IsSourceOption: {OriginatesFromSource}";
        }

        private static string GenerateDisplayValue(int channelId, bool isStereo, string value, string otherDescription)
		{
			if (String.Equals(value, Constants.None)) return Constants.None;

			if (isStereo)
			{
				if (String.Equals(value, "Other", StringComparison.InvariantCultureIgnoreCase))
				{
					return $"Source Audio Channel {channelId}&{(channelId + 1)}: {value} - {otherDescription}";
				}
				else
				{
					return $"Source Audio Channel {channelId}&{(channelId + 1)}: {value}";
				}
			}
			else
			{
				if (String.Equals(value, "Other", StringComparison.InvariantCultureIgnoreCase))
				{
					return $"Source Audio Channel {channelId}: {value} - {otherDescription}";
				}
				else
				{
					return $"Source Audio Channel {channelId}: {value}";
				}
			}
		}
	}
}
