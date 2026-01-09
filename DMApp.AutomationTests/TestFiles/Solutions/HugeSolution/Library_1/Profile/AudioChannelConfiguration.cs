namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM.Logging.Orchestration;

	/// <summary>
	/// This class represents a group of unique Audio Channel Pairs.
	/// </summary>
	public class AudioChannelConfiguration
	{
		private readonly List<AudioChannelPair> audioChannelPairs = new List<AudioChannelPair>();

		private int lastDisplayedPair = -1;

		private int maxDisplayedPair = -1;

		private readonly bool isReception;

		private bool isCopyFromSource;

		public AudioChannelConfiguration(bool isReception) : this(isReception, new List<ProfileParameter>())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AudioChannelConfiguration" /> class
		/// </summary>
		/// <param name="isReception"></param>
		/// <param name="profileParameters">Collection containing the audio channel profile parameters. This can list can also contain other profile parameters. Only the audio channel profile parameters are used.</param>
		public AudioChannelConfiguration(bool isReception, IEnumerable<ProfileParameter> profileParameters)
		{
			this.isReception = isReception;
			IEnumerable<ProfileParameter> audioProfileParameters = profileParameters.Where(p => p != null && ProfileParameterGuids.AllAudioChannelConfigurationGuids.Contains(p.Id));

			Initialize(audioProfileParameters);
		}

		/// <summary>
		/// This event is called the IsCopyFromSource property changes.
		/// </summary>
		public event EventHandler<bool> IsCopyFromSourceChanged;

		/// <summary>
		/// A boolean indicating if this Audio Channel Configuration is a copy from the source, taking into account some special copy rules.
		/// </summary>
		public bool IsCopyFromSource
		{
			get => isCopyFromSource;
			set
			{
				isCopyFromSource = value;
				IsCopyFromSourceChanged?.Invoke(this, isCopyFromSource);
			}
		}

		/// <summary>
		/// The parameter indicating if dolby-e decoding is required.
		/// Only applicable in case dolby-e is selected in the source.
		/// </summary>
		public ProfileParameter AudioDolbyDecodingRequiredProfileParameter { get; set; }

		/// <summary>
		/// The parameter indicating if embedding is required.
		/// Not applicable in the source.
		/// </summary>
		public ProfileParameter AudioEmbeddingRequiredProfileParameter { get; set; }

		/// <summary>
		/// The parameter indicating if deembedding is required.
		/// Not applicable in the source.
		/// </summary>
		public ProfileParameter AudioDeembeddingRequiredProfileParameter { get; set; }

		/// <summary>
		/// The parameter indicating if shuffling is required.
		/// Not applicable in the source.
		/// </summary>
		public ProfileParameter AudioShufflingRequiredProfileParameter { get; set; }

		/// <summary>
		/// Gets the list of Audio Channel Pairs managed by this AudioChannelConfiguration.
		/// </summary>
		public IList<AudioChannelPair> AudioChannelPairs
		{
			get
			{
				return audioChannelPairs;
			}
		}

		/// <summary>
		/// Indicates if an additional Audio Channel Pair can be displayed in the UI.
		/// </summary>
		public bool CanAddAudioPair
		{
			get
			{
				return lastDisplayedPair < maxDisplayedPair;
			}
		}

		/// <summary>
		/// Indicates if an Audio Channel Pair can be removed in the UI.
		/// </summary>
		public bool CanRemoveAudioPair
		{
			get
			{
				return lastDisplayedPair > 0;
			}
		}

		/// <summary>
		/// Gets a value matching the Channel of the last Audio Channel Pair that is displayed in the UI.
		/// Returns -1 if no Audio Channel Pairs are displayed.
		/// </summary>
		public int LastDisplayedAudioPairchannel
		{
			get
			{
				return lastDisplayedPair;
			}
		}

		/// <summary>
		/// Used to indicate that the next Audio Channel Pair should be visible in the UI.
		/// </summary>
		/// <returns>-1 if the last Audio Channel Pair was already displayed, else the Channel ID of the newly displayed Audio Channel Pair.</returns>
		public int AddAudioChannelPair()
		{
			if (!CanAddAudioPair) return -1;
			lastDisplayedPair += 2;
			AudioChannelPairAdded?.Invoke(this, lastDisplayedPair);
			return lastDisplayedPair;
		}

		/// <summary>
		/// Used to indicate that the last Audio Channel Pair should not be displayed in the UI.
		/// </summary>
		/// <returns>-1 if no Audio Channel Pairs can be removed, else the Channel ID of the removed Audio Channel Pair.</returns>
		public int RemoveAudioChannelPair()
		{
			if (!CanRemoveAudioPair) return -1;
			lastDisplayedPair -= 2;
			AudioChannelPairRemoved?.Invoke(this, lastDisplayedPair);
			return (lastDisplayedPair + 2);
		}

		/// <summary>
		/// This event is called when an additional Audio Channel Pair should be displayed in the UI.
		/// </summary>
		public event EventHandler<int> AudioChannelPairAdded;

		/// <summary>
		/// This event is called when the last Audio Channel Pair should be removed from the UI.
		/// </summary>
		public event EventHandler<int> AudioChannelPairRemoved;

		/// <summary>
		/// Checks if the audio channel configuration of this object is a copy from the given source audio channel configuration, and sets the IsCopyFromSource property accordingly.
		/// </summary>
		/// <param name="sourceAudioChannelConfiguration">The audio channel configuration of the source service.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="AudioChannelPairNotFoundException"/>
		public void SetIsCopyFromSourceProperty(AudioChannelConfiguration sourceAudioChannelConfiguration)
		{
			IsCopyFromSource = MatchesSourceConfiguration(sourceAudioChannelConfiguration);
		}

		public bool MatchesSourceConfiguration(AudioChannelConfiguration sourceAudioChannelConfiguration)
		{
			if (sourceAudioChannelConfiguration == null)
				throw new ArgumentNullException(nameof(sourceAudioChannelConfiguration));
			if (sourceAudioChannelConfiguration.AudioDolbyDecodingRequiredProfileParameter == null)
				throw new ArgumentException("Audio Dolby Decoding Profile Parameter is null", nameof(sourceAudioChannelConfiguration));

			int channelOffset = 0;
			for (int channel = 1; channel < 16; channel += 2)
			{
				if (!MatchesAudioChannelConfiguration(sourceAudioChannelConfiguration, channel, ref channelOffset)) return false;
			}

			return true;
		}

		private bool MatchesAudioChannelConfiguration(AudioChannelConfiguration sourceAudioChannelConfiguration, int channel, ref int channelOffset)
		{
			AudioChannelPair sourcePair = sourceAudioChannelConfiguration.AudioChannelPairs.FirstOrDefault(p => p.Channel == channel);
			if (sourcePair == null) return false;

			bool dolbyChannelSelected = sourcePair.FirstChannelProfileParameter.StringValue.StartsWith("Dolby");
			bool dolbyDecodingRequired = sourcePair.DolbyDecodingProfileParameter.StringValue == "Yes";
			if (dolbyChannelSelected && dolbyDecodingRequired)
			{
				// If the source pair has channel = dolby and dolby decoding is required
				// then the child needs 4 channel pairs with stereo dolby channels
				int counter = 1;
				int helperChannel = channel;
				while (helperChannel <= 16 && counter <= 4)
				{
					if (!MatchesDolbyConfiguration(sourcePair, helperChannel, counter)) return false;

					counter++;
					helperChannel += 2;
				}

				channelOffset = counter + 1;
			}
			else if (channel + channelOffset <= 16)
			{
				if (!MatchesRegularAudioChannelPair(sourcePair, channel, channelOffset)) return false;
			}
			else
			{
				// Nothing to compare
			}

			return true;
		}

		private bool MatchesDolbyConfiguration(AudioChannelPair sourcePair, int helperChannel, int counter)
		{
			AudioChannelPair childPair = AudioChannelPairs.FirstOrDefault(p => p.Channel == helperChannel);
			if (childPair == null) return false;

			bool isEqual = true;
			isEqual &= String.Format("{0} A{1}&A{2}", sourcePair.FirstChannelProfileParameter.StringValue, 2 * counter - 1, 2 * counter) == childPair.FirstChannelProfileParameter.StringValue;
			isEqual &= String.Format("{0} A{1}&A{2}", sourcePair.SecondChannelProfileParameter.StringValue, 2 * counter - 1, 2 * counter) == childPair.SecondChannelProfileParameter.StringValue;
			return isEqual;
		}

		private bool MatchesRegularAudioChannelPair(AudioChannelPair sourcePair, int channel, int channelOffset)
		{
			AudioChannelPair childPair = AudioChannelPairs.FirstOrDefault(p => p.Channel == channel + channelOffset);
			if (childPair == null) return false;

			bool isEqual = true;
			isEqual &= sourcePair.FirstChannelProfileParameter.StringValue == childPair.FirstChannelProfileParameter.StringValue;
			isEqual &= sourcePair.FirstChannelDescriptionProfileParameter.StringValue == childPair.FirstChannelDescriptionProfileParameter.StringValue;
			isEqual &= sourcePair.SecondChannelProfileParameter.StringValue == childPair.SecondChannelProfileParameter.StringValue;
			isEqual &= sourcePair.SecondChannelDescriptionProfileParameter.StringValue == childPair.SecondChannelDescriptionProfileParameter.StringValue;
			return isEqual;
		}

		/// <summary>
		/// Copies the values of the given Source Audio Channel Configuration into the current object. 
		/// </summary>
		/// <param name="sourceAudioChannelConfiguration">The Audio Channel Configuration to copy.</param>
		/// <exception cref="AudioChannelPairNotFoundException"/>
		public void CopyFromSource(AudioChannelConfiguration sourceAudioChannelConfiguration)
		{
			if (sourceAudioChannelConfiguration == null) throw new ArgumentNullException(nameof(sourceAudioChannelConfiguration));
			if (!AudioChannelPairs.Any()) return; // Nothing to copy if the service doesn't specify any audio channel pairs (e.g. Eurovision Tx service)

			lastDisplayedPair = sourceAudioChannelConfiguration.LastDisplayedAudioPairchannel;

			int channelOffset = 0;
			for (int channel = 1; channel < 16; channel += 2)
			{
				AudioChannelPair sourcePair = sourceAudioChannelConfiguration.AudioChannelPairs.FirstOrDefault(p => p.Channel == channel) ?? throw new AudioChannelPairNotFoundException(channel, "source");

				bool dolbyChannelSelected = sourcePair.FirstChannelProfileParameter.StringValue.StartsWith("Dolby");
				bool dolbyDecodingRequired = sourcePair.DolbyDecodingProfileParameter?.StringValue == "Yes"; // Only applies to source services
				if (dolbyChannelSelected && dolbyDecodingRequired)
				{
					// If the source pair has channel = dolby and dolby decoding is required
					// then the child needs 4 channel pairs with stereo dolby channels
					int counter = CopyDolbyAudioPairs(channel);
					channelOffset = counter + 1;
				}
				else if (channel + channelOffset <= 16)
				{
					AudioChannelPair pair = AudioChannelPairs.FirstOrDefault(p => p.Channel == channel + channelOffset) ?? throw new AudioChannelPairNotFoundException((channel + channelOffset), "child");
					CopyAudioChannelPair(sourceAudioChannelConfiguration, sourcePair, pair, channel);
				}
				else
				{
					// Nothing to copy
				}
			}

			IsCopyFromSource = true;
		}

		private int CopyDolbyAudioPairs(int channel)
		{
			int counter = 1;
			int helperChannel = channel;
			while (helperChannel <= 16 && counter <= 4)
			{
				AudioChannelPair pair = AudioChannelPairs.FirstOrDefault(p => p.Channel == helperChannel) ?? throw new AudioChannelPairNotFoundException(helperChannel, "child");

				pair.IsStereo = true;
				pair.FirstChannelProfileParameter.Value = pair.FirstChannelOptions.FirstOrDefault(c => c.Value.EndsWith(String.Format("A{0}&A{1}", 2 * counter - 1, 2 * counter)));
				pair.SecondChannelProfileParameter.Value = pair.SecondChannelOptions.FirstOrDefault(c => c.Value.EndsWith(String.Format("A{0}&A{1}", 2 * counter - 1, 2 * counter)));

				counter++;
				helperChannel += 2;
			}

			return counter;
		}

		private static void CopyAudioChannelPair(AudioChannelConfiguration sourceAudioChannelConfiguration, AudioChannelPair sourcePair, AudioChannelPair destinationPair, int channel)
		{
			destinationPair.IsStereo = sourcePair.IsStereo;
			destinationPair.FirstChannelProfileParameter.Value = sourcePair.FirstChannelProfileParameter.Value;
			destinationPair.FirstChannelDescriptionProfileParameter.Value = sourcePair.FirstChannelDescriptionProfileParameter.Value;
			destinationPair.SecondChannelProfileParameter.Value = sourcePair.SecondChannelProfileParameter.Value;
			destinationPair.SecondChannelDescriptionProfileParameter.Value = sourcePair.SecondChannelDescriptionProfileParameter.Value;

			if (sourceAudioChannelConfiguration.isReception)
			{
				destinationPair.FirstChannel = AudioChannelOption.FromSource(channel, sourcePair.IsStereo, sourcePair.FirstChannelProfileParameter.StringValue, sourcePair.FirstChannelDescriptionProfileParameter.StringValue);
				destinationPair.SecondChannel = AudioChannelOption.FromSource(channel + 1, sourcePair.IsStereo, sourcePair.SecondChannelProfileParameter.StringValue, sourcePair.SecondChannelDescriptionProfileParameter.StringValue);
			}
			else
			{
				destinationPair.FirstChannel = sourcePair.FirstChannel;
				destinationPair.SecondChannel = sourcePair.SecondChannel;
			}
		}

		public void InitializeUiValues(AudioChannelConfiguration sourceAudioChannelConfiguration, Helpers helpers)
		{
			List<AudioChannelOption> sourceOptions = sourceAudioChannelConfiguration.GetSourceOptions();
			helpers.Log(nameof(AudioChannelConfiguration), nameof(InitializeUiValues), $"Source Options: {String.Join(", ", sourceOptions)}");

			if (isCopyFromSource)
			{
				CopyFromSource(sourceAudioChannelConfiguration);
			}
			else
			{
				List<AudioChannelOption> availableOptions = new List<AudioChannelOption>(sourceOptions);
				foreach (var pair in AudioChannelPairs)
				{
					helpers.Log(nameof(AudioChannelConfiguration), nameof(InitializeUiValues), $"Pair {pair}");

					if (pair.IsStereo)
					{
						InitializeUiForStereoAudioChannel(pair, availableOptions, helpers);
					}
					else
					{
						InitializeUiForMonoAudioChannel(pair, availableOptions, helpers);
					}
				}
			}

			SetSourceOptions(sourceOptions);
		}

		private static void InitializeUiForStereoAudioChannel(AudioChannelPair pair, List<AudioChannelOption> availableOptions, Helpers helpers = null)
		{
			var possibleOption = availableOptions.FirstOrDefault(x => x.IsStereo && x.Value.Equals(pair.FirstChannelProfileParameter.Value) && x.OtherDescription.Equals(pair.FirstChannelDescriptionProfileParameter.Value));
			if (possibleOption != null)
			{
				helpers?.Log(nameof(AudioChannelConfiguration), nameof(InitializeUiValues), $"Mapping first channel to {possibleOption}");
				availableOptions.Remove(possibleOption);
				pair.FirstChannel = possibleOption;
				pair.SecondChannel = possibleOption;
			}
			else
			{
				helpers?.Log(nameof(AudioChannelConfiguration), nameof(InitializeUiValues), $"No option available for first channel");
				pair.FirstChannel = AudioChannelOption.None();
				pair.SecondChannel = AudioChannelOption.None();
			}
		}

		private static void InitializeUiForMonoAudioChannel(AudioChannelPair pair, List<AudioChannelOption> availableOptions, Helpers helpers = null)
		{
			// First channel
			var possibleFirstChannelOption = availableOptions.FirstOrDefault(x => !x.IsStereo && x.Value.Equals(pair.FirstChannelProfileParameter.Value) && x.OtherDescription.Equals(pair.FirstChannelDescriptionProfileParameter.Value));
			if (possibleFirstChannelOption != null)
			{
				helpers?.Log(nameof(AudioChannelConfiguration), nameof(InitializeUiValues), $"Mapping first channel to {possibleFirstChannelOption}");
				availableOptions.Remove(possibleFirstChannelOption);
				pair.FirstChannel = possibleFirstChannelOption;
			}
			else
			{
				helpers?.Log(nameof(AudioChannelConfiguration), nameof(InitializeUiValues), $"No option available for first channel");
				pair.FirstChannel = AudioChannelOption.None();
			}

			// Second channel
			var possibleSecondChannelOption = availableOptions.FirstOrDefault(x => !x.IsStereo && x.Value.Equals(pair.SecondChannelProfileParameter.Value) && x.OtherDescription.Equals(pair.SecondChannelDescriptionProfileParameter.Value));
			if (possibleSecondChannelOption != null)
			{
				helpers?.Log(nameof(AudioChannelConfiguration), nameof(InitializeUiValues), $"Mapping first channel to {possibleSecondChannelOption}");
				availableOptions.Remove(possibleSecondChannelOption);
				pair.SecondChannel = possibleSecondChannelOption;
			}
			else
			{
				helpers?.Log(nameof(AudioChannelConfiguration), nameof(InitializeUiValues), $"No option available for second channel");
				pair.SecondChannel = AudioChannelOption.None();
			}
		}

		/// <summary>
		/// Initialize the properties linked to audio profile parameters.
		/// </summary>
		/// <param name="audioProfileParameters">The list of audio profile parameters.</param>
		private void Initialize(IEnumerable<ProfileParameter> audioProfileParameters)
		{
			AudioDolbyDecodingRequiredProfileParameter = audioProfileParameters.FirstOrDefault(a => a.Id == ProfileParameterGuids.AudioDolbyDecodingRequired);
			AudioEmbeddingRequiredProfileParameter = audioProfileParameters.FirstOrDefault(a => a.Id == ProfileParameterGuids.AudioEmbeddingRequired);
			AudioDeembeddingRequiredProfileParameter = audioProfileParameters.FirstOrDefault(a => a.Id == ProfileParameterGuids.AudioDeembeddingRequired);
			AudioShufflingRequiredProfileParameter = audioProfileParameters.FirstOrDefault(a => a.Id == ProfileParameterGuids.AudioShufflingRequired);

			foreach (var audioProfileParameter in audioProfileParameters)
			{
				// there is only 1 parameter used to indicate if audio dolby decoding is required
				if (AudioDolbyDecodingRequiredProfileParameter != null && AudioDolbyDecodingRequiredProfileParameter.Id == audioProfileParameter.Id) continue;

				// check that this audio channel profile parameter is not yet processed
				// every Audio Channel Pair uses a number of profile parameters 
				if (AudioChannelPairs.Any(p => p.Contains(audioProfileParameter))) continue;

				int channel;
				if (!Int32.TryParse(audioProfileParameter.Name.Split(' ').Last(), out channel) || channel % 2 == 0)
				{
					// an Audio Channel Pair contains the configuration of 2 audio channels
					// we can skip this in case the channel is not uneven as it will be automatically added to the correct audio channel pair already
					continue;
				}

				var firstChannel = audioProfileParameter;
				var firstChannelDescription = audioProfileParameters.FirstOrDefault(p => p.Name == String.Format("{0} Description", firstChannel.Name));
				var secondChannel = audioProfileParameters.FirstOrDefault(p => p.Name == firstChannel.Name.Replace(channel.ToString(), (channel + 1).ToString()));
				var secondChannelDescription = audioProfileParameters.FirstOrDefault(p => p.Name == String.Format("{0} Description", secondChannel.Name));

				var audioChannel = new AudioChannelPair(firstChannel, firstChannelDescription, secondChannel, secondChannelDescription, AudioDolbyDecodingRequiredProfileParameter, isReception);
				if (!audioChannelPairs.Contains(audioChannel)) audioChannelPairs.Add(audioChannel);

				if (audioChannel.ShouldDisplay) lastDisplayedPair = audioChannel.Channel;
				if (audioChannel.Channel > maxDisplayedPair) maxDisplayedPair = audioChannel.Channel;
			}
		}

		public void Clear()
		{
			if (AudioDolbyDecodingRequiredProfileParameter != null)
			{
				AudioDolbyDecodingRequiredProfileParameter.Value = false;
			}

			foreach (AudioChannelPair pair in AudioChannelPairs)
			{
				pair.Clear();
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine($"Is copy from source: {IsCopyFromSource} | ");
			if (AudioDeembeddingRequiredProfileParameter != null) sb.AppendLine($"Audio deembedding required: {AudioDeembeddingRequiredProfileParameter.StringValue} | ");
			if (AudioEmbeddingRequiredProfileParameter != null) sb.AppendLine($"Audio embedding required: {AudioEmbeddingRequiredProfileParameter.StringValue} | ");
			if (AudioShufflingRequiredProfileParameter != null) sb.AppendLine($"Audio shuffling required: {AudioShufflingRequiredProfileParameter.StringValue} | ");

			foreach (var audioChannelPair in AudioChannelPairs)
			{
				sb.AppendLine($"{audioChannelPair} | ");
			}

			return sb.ToString();
		}

		public List<AudioChannelOption> GetSourceOptions()
		{
			if (isReception)
			{
				List<AudioChannelOption> options = new List<AudioChannelOption>();
				foreach (var audioChannelPair in audioChannelPairs)
				{
					options.AddRange(audioChannelPair.GetSourceOptions());
				}

				// TODO: Add Dolby channels to possible options if Dobly Decoding is enabled

				return options.Where(x => !String.Equals(x.Value, Constants.None)).ToList();
			}
			else
			{
				return allAvailableAudioChannelOptions;
			}
		}

		private List<AudioChannelOption> allAvailableAudioChannelOptions = new List<AudioChannelOption>();

		public void SetSourceOptions(IEnumerable<AudioChannelOption> sourceOptions)
		{
			if (isReception) throw new InvalidOperationException("Unable to set the source options on a reception service");
			allAvailableAudioChannelOptions = new List<AudioChannelOption>(sourceOptions);

			// Verify if all audio channel pairs have valid configuration
			//foreach (AudioChannelPair pair in AudioChannelPairs)
			//{
			//	if (pair.FirstChannel != null && !allAvailableAudioChannelOptions.Contains(pair.FirstChannel))
			//	{
			//		pair.FirstChannel = AudioChannelOption.None();
			//	}

			//	if (pair.SecondChannel != null && !allAvailableAudioChannelOptions.Contains(pair.SecondChannel))
			//	{
			//		pair.SecondChannel = AudioChannelOption.None();
			//	}
			//}

			UpdateSelectableOptions();
		}

		public void UpdateSelectableOptions()
		{
			var availableOptions = FilterAvailableOptions(allAvailableAudioChannelOptions);

			foreach (var audioChannelPair in audioChannelPairs)
			{
				List<AudioChannelOption> firstChannelOptions = new List<AudioChannelOption>(availableOptions)
				{
					AudioChannelOption.None()
				};

				if (audioChannelPair.FirstChannel != null)
				{
					if (!audioChannelPair.FirstChannel.IsNone)
					{
						firstChannelOptions.Add(audioChannelPair.FirstChannel);
					}
				}

				List<AudioChannelOption> secondChannelOptions = new List<AudioChannelOption>(availableOptions)
				{
					AudioChannelOption.None()
				};

				if (audioChannelPair.SecondChannel != null)
				{
					if (!audioChannelPair.SecondChannel.IsNone)
					{
						secondChannelOptions.Add(audioChannelPair.SecondChannel);
					}
				}

				audioChannelPair.FirstChannelOptions = firstChannelOptions.OrderBy(x => x.DisplayValue).ToList();
				audioChannelPair.SecondChannelOptions = secondChannelOptions.OrderBy(x => x.DisplayValue).ToList();
			}
		}

		/// <summary>
		/// Returns the Source Audio Channel options that are not being used by any of the Audio Channel Pairs.
		/// </summary>
		/// <param name="sourceOptions">All options that are generated from the Source Audio Configuration.</param>
		/// <returns>Source Audio Channel options that are not being used by any of the Audio Channel Pairs.</returns>
		private IEnumerable<AudioChannelOption> FilterAvailableOptions(IEnumerable<AudioChannelOption> sourceOptions)
		{
			List<AudioChannelOption> availableOptions = new List<AudioChannelOption>(sourceOptions);

			foreach (var pair in AudioChannelPairs)
			{
				if (pair.FirstChannel != null && availableOptions.Contains(pair.FirstChannel)) availableOptions.Remove(pair.FirstChannel);
				if (pair.SecondChannel != null && availableOptions.Contains(pair.SecondChannel)) availableOptions.Remove(pair.SecondChannel);
			}

			return availableOptions;
		}
	}
}