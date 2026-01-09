namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision
{
	using Newtonsoft.Json;
    using System;

    public class AudioChannel
	{
		public static readonly string DefaultCode = "-";

		private string code = DefaultCode;
		private string otherText = String.Empty;

		public event EventHandler<string> CodeChanged;
		public event EventHandler<string> OtherTextChanged;

		public string AudioChannelCode
        {
			get => code;
			set
            {
				code = value ?? throw new ArgumentNullException(nameof(value));
				CodeChanged?.Invoke(this, value);
            }
        }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string AudioChannelOtherText
        {
			get => otherText;
			set
            {
				otherText = value ?? throw new ArgumentNullException(nameof(value));
				OtherTextChanged?.Invoke(this, value);
            }
        }
	}
}