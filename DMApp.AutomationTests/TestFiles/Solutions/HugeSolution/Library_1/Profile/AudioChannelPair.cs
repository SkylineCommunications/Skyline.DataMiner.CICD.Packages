namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile
{
	using System;
    using System.Collections;
    using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

	/// <summary>
	/// This class represents an Audio Channel Pair.
	/// </summary>
	public class AudioChannelPair : IReadOnlyList<ProfileParameter>
	{
		private bool isStereo;

		/// <summary>
		/// Initializes a new instance of the <see cref="AudioChannelPair"/> class
		/// </summary>
		/// <param name="firstChannel">ProfileParameter of the first audio channel in this pair.</param>
		/// <param name="firstChannelDescription">ProfileParameter of the description for the first audio channel in this pair.</param>
		/// <param name="secondChannel">ProfileParameter of the second audio channel in this pair.</param>
		/// <param name="secondChannelDescription">ProfileParameter of the description for the second audio channel in this pair.</param>
		/// <param name="dolbyDecoding">ProfileParameter of the Dolby Decoding parameter.</param>
		/// <param name="isReception"></param>
		/// <exception cref="ArgumentNullException"/>
		public AudioChannelPair(ProfileParameter firstChannel, ProfileParameter firstChannelDescription, ProfileParameter secondChannel, ProfileParameter secondChannelDescription, ProfileParameter dolbyDecoding, bool isReception)
		{
			FirstChannelProfileParameter = firstChannel ?? throw new ArgumentNullException(nameof(firstChannel));
			FirstChannelDescriptionProfileParameter = firstChannelDescription ?? throw new ArgumentNullException(nameof(firstChannelDescription));
			SecondChannelProfileParameter = secondChannel ?? throw new ArgumentNullException(nameof(secondChannel));
			SecondChannelDescriptionProfileParameter = secondChannelDescription ?? throw new ArgumentNullException(nameof(secondChannelDescription));

			FirstChannel = AudioChannelOption.FromDiscreet(firstChannel.StringValue, firstChannelDescription.StringValue);
			SecondChannel = AudioChannelOption.FromDiscreet(secondChannel.StringValue, secondChannelDescription.StringValue);

			List<AudioChannelOption> allAudioChannelOptions = new List<AudioChannelOption> { AudioChannelOption.None() }.Concat(FirstChannelProfileParameter.Discreets.Select(d => AudioChannelOption.FromDiscreet(d.DisplayValue, String.Empty))).ToList();
			List<AudioChannelOption> dolbyOptions = allAudioChannelOptions.Where(x => x.Value.Contains("Dolby")).ToList();
            List<AudioChannelOption> dolbyDuoChannelOptions = dolbyOptions.Where(x => x.Value.Contains("&A")).ToList();

			if (isReception)
            {
				FirstChannelOptions = allAudioChannelOptions.Except(dolbyDuoChannelOptions).ToList();
				SecondChannelOptions = allAudioChannelOptions.Except(dolbyDuoChannelOptions).ToList();
			}
			else
            {
				FirstChannelOptions = allAudioChannelOptions;
				SecondChannelOptions = allAudioChannelOptions;
			}

			IsStereo = FirstChannel.Equals(SecondChannel);

			//if (FirstChannelProfileParameter.StringValue == "Other" && SecondChannelProfileParameter.StringValue == "Other")
			//{
			//	IsStereo = FirstChannelDescriptionProfileParameter.StringValue == SecondChannelDescriptionProfileParameter.StringValue;
			//}
			//else
			//{
			//	IsStereo = FirstChannelProfileParameter.StringValue == SecondChannelProfileParameter.StringValue;
			//}

			if (dolbyDecoding != null)
			{
				DolbyDecodingProfileParameter = dolbyDecoding;
			}
		}

		/// <summary>
		/// Indicates if this audio channel pair contains the provided profile parameter.
		/// </summary>
		/// <param name="parameter">The profile parameter.</param>
		/// <returns>True in case the profile parameter is part of this audio channel configuration.</returns>
		public bool Contains(ProfileParameter parameter)
		{
			return FirstChannelProfileParameter.Equals(parameter) || FirstChannelDescriptionProfileParameter.Equals(parameter) || SecondChannelProfileParameter.Equals(parameter) || SecondChannelDescriptionProfileParameter.Equals(parameter);
		}

		/// <summary>
		/// Clears the values of the Audio Channel Profile Parameters contained in the Pair.
		/// </summary>
		public void Clear()
		{
			if (DolbyDecodingProfileParameter != null)
			{
				DolbyDecodingProfileParameter.Value = "No";
			}

			//FirstChannelProfileParameter.Value = Constants.None;
			//SecondChannelProfileParameter.Value = Constants.None;

			//FirstChannelDescriptionProfileParameter.Value = string.Empty;
			//SecondChannelDescriptionProfileParameter.Value = string.Empty;

			FirstChannel = AudioChannelOption.None();
			SecondChannel = AudioChannelOption.None();

			IsStereo = true;
		}

		// UI
		public List<AudioChannelOption> SelectedOptions
		{
			get
            {
				var options = new List<AudioChannelOption>();
				if (FirstChannel != null && !FirstChannel.IsNone) options.Add(FirstChannel);
				if (SecondChannel != null && !SecondChannel.IsNone) options.Add(SecondChannel);
				return options;
            } 
		}

		public event EventHandler<AudioChannelOptionChangedEventArgs> FirstChannelChanged;

		private AudioChannelOption firstChannel;

		// UI
		public AudioChannelOption FirstChannel
        {
			get => firstChannel;
			set
            {
				AudioChannelOption oldValue = firstChannel;
				firstChannel = value;

				FirstChannelProfileParameter.Value = value.Value;
				FirstChannelDescriptionProfileParameter.Value = value.OtherDescription;

				if (value.IsStereo)
                {
					SecondChannelProfileParameter.Value = value.Value;
					SecondChannelDescriptionProfileParameter.Value = value.OtherDescription;
				}

				FirstChannelChanged?.Invoke(this, new AudioChannelOptionChangedEventArgs(value, oldValue));
            }
        }

		public event EventHandler<IReadOnlyList<AudioChannelOption>> FirstChannelOptionsChanged;

		private IReadOnlyList<AudioChannelOption> firstChannelOptions = new List<AudioChannelOption>();

		public IReadOnlyList<AudioChannelOption> FirstChannelOptions
        {
			get => firstChannelOptions;
			set
            {
				firstChannelOptions = value ?? throw new ArgumentNullException(nameof(value));
				FirstChannelOptionsChanged?.Invoke(this, firstChannelOptions);
            }
        }

		public event EventHandler<AudioChannelOptionChangedEventArgs> SecondChannelChanged;

		private AudioChannelOption secondChannel;

		// UI
		public AudioChannelOption SecondChannel
		{
			get => secondChannel;
			set
			{
				AudioChannelOption oldValue = secondChannel;
				secondChannel = value;

				SecondChannelProfileParameter.Value = value.Value;
				SecondChannelDescriptionProfileParameter.Value = value.OtherDescription;

				if (value.IsStereo)
				{
					FirstChannelProfileParameter.Value = value.Value;
					FirstChannelDescriptionProfileParameter.Value = value.OtherDescription;
				}

				SecondChannelChanged?.Invoke(this, new AudioChannelOptionChangedEventArgs(value, oldValue));
			}
		}

		public event EventHandler<IReadOnlyList<AudioChannelOption>> SecondChannelOptionsChanged;

		private IReadOnlyList<AudioChannelOption> secondChannelOptions = new List<AudioChannelOption>();

		public IReadOnlyList<AudioChannelOption> SecondChannelOptions
		{
			get => secondChannelOptions;
			set
			{
				secondChannelOptions = value ?? throw new ArgumentNullException(nameof(value));
				SecondChannelOptionsChanged?.Invoke(this, secondChannelOptions);
			}
		}

		//public event EventHandler<AudioChannelOptionsChangedEventArgs> AudioChannelOptionsChanged;

		/// <summary>
		/// Gets all options without the Dolby Decoded channels for the Audio Channel profile parameters.
		/// </summary>
		//public List<AudioChannelOption> AllAudioChannelOptionsWithoutDolbyDecoding { get; private set; }

		/// <summary>
		/// The profile parameter for the first channel.
		/// </summary>
		public ProfileParameter FirstChannelProfileParameter { get; private set; }

		/// <summary>
		/// The profile parameter for the description of the first channel.
		/// </summary>
		public ProfileParameter FirstChannelDescriptionProfileParameter { get; private set; }

		/// <summary>
		/// The profile parameter for the second channel.
		/// </summary>
		public ProfileParameter SecondChannelProfileParameter { get; private set; }

		/// <summary>
		/// The profile parameter for the description of the second channel.
		/// </summary>
		public ProfileParameter SecondChannelDescriptionProfileParameter { get; private set; }

		/// <summary>
		/// The Dolby decoding profile parameter.
		/// </summary>
		/// <remarks>Possible values: "Yes", "No".</remarks>
		public ProfileParameter DolbyDecodingProfileParameter { get; private set; }

		/// <summary>
		/// The channel of this audio channel pair.
		/// This matches the Audio Channel ID of the first Audio Channel in this Pair.
		/// </summary>
		public int Channel => Convert.ToInt32(FirstChannelProfileParameter.Name.Split(' ').Last());

		/// <summary>
		/// The description of this audio channel pair.
		/// </summary>
		public string Description => $"Audio Channel {Channel}&{Channel + 1}";

		/// <summary>
		/// Indicates if this audio channel pair is stereo or mono.
		/// </summary>
		public bool IsStereo
		{
			get => isStereo;

			set
			{
				if (isStereo != value)
				{
					isStereo = value;
					IsStereoChanged?.Invoke(this, isStereo);
				}
			}
		}

		/// <summary>
		/// This event is called when the value of the IsStereo configuration parameter is updated.
		/// </summary>
		public event EventHandler<bool> IsStereoChanged;

		/// <summary>
		/// Used in AudioChannelPairSection.
		/// Indicates whether this pair should be visible in the UI.
		/// An Audio Channel Pair should only be displayed when it's values are not none.
		/// </summary>
		public bool ShouldDisplay => FirstChannelProfileParameter.StringValue != "None" && FirstChannelProfileParameter.Value != null || SecondChannelProfileParameter.StringValue != "None" && SecondChannelProfileParameter.Value != null;

        public int Count => 2;

        public ProfileParameter this[int index]
        {
			get
            {
				switch(index)
                {
					case 0:
						return FirstChannelProfileParameter;
					case 1:
						return SecondChannelProfileParameter;
					default:
						throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        /// <summary>
        /// Generates a HashCode for this object.
        /// </summary>
        /// <returns>HashCode for this object.</returns>
        public override int GetHashCode()
		{
			return FirstChannelProfileParameter.GetHashCode() ^ FirstChannelDescriptionProfileParameter.GetHashCode() ^ SecondChannelProfileParameter.GetHashCode() ^ SecondChannelDescriptionProfileParameter.GetHashCode();
		}

		/// <summary>
		/// Checks if this object matches another one.
		/// </summary>
		/// <param name="obj">Object to check.</param>
		/// <returns>True if object matches else false.</returns>
		public override bool Equals(object obj)
		{
			AudioChannelPair other = obj as AudioChannelPair;
			if (other == null) return false;

			return FirstChannelProfileParameter.Equals(other.FirstChannelProfileParameter) && FirstChannelDescriptionProfileParameter.Equals(other.FirstChannelDescriptionProfileParameter) && SecondChannelProfileParameter.Equals(other.SecondChannelProfileParameter) && SecondChannelDescriptionProfileParameter.Equals(other.SecondChannelDescriptionProfileParameter);
		}

		public override string ToString()
		{
			return $"Channel {Channel} Profile Parameter = {FirstChannelProfileParameter.StringValue}. | Channel {Channel + 1} Profile Parameter = {SecondChannelProfileParameter.StringValue} | IsStereo = {IsStereo} | Dolby Decoding = {(DolbyDecodingProfileParameter != null ? DolbyDecodingProfileParameter.StringValue : "N/A")}|UI: First Channel {FirstChannel}, Second Channel: {SecondChannel}";
		}

        public IEnumerator<ProfileParameter> GetEnumerator()
        {
			List<ProfileParameter> list = new List<ProfileParameter>(new ProfileParameter[] { FirstChannelProfileParameter, SecondChannelProfileParameter });
			return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
			return GetEnumerator();
		}

		internal IEnumerable<AudioChannelOption> GetSourceOptions()
        {
			List<AudioChannelOption> options = new List<AudioChannelOption>();
			if (IsStereo)
            {
				options.Add(AudioChannelOption.FromSource(Channel, true, FirstChannelProfileParameter.StringValue, FirstChannelDescriptionProfileParameter.StringValue));
            }
			else
            {
				options.Add(AudioChannelOption.FromSource(Channel, false, FirstChannelProfileParameter.StringValue, FirstChannelDescriptionProfileParameter.StringValue));
				options.Add(AudioChannelOption.FromSource(Channel + 1, false, SecondChannelProfileParameter.StringValue, SecondChannelDescriptionProfileParameter.StringValue));
			}

			return options;
		}

        public class AudioChannelOptionChangedEventArgs : EventArgs
		{
			internal AudioChannelOptionChangedEventArgs(AudioChannelOption newValue, AudioChannelOption previousValue)
			{
				NewValue = newValue;
				PreviousValue = previousValue;
			}

			public AudioChannelOption NewValue { get; private set; }

			public AudioChannelOption PreviousValue { get; private set; }
		}
	}
}