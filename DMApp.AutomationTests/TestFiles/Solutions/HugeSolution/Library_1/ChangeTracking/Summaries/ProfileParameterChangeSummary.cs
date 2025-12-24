namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ChangeTracking.Summaries
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;

	public class ProfileParameterChangeSummary : ChangeSummary
	{
		[Flags]
		private enum TypesOfProfileParameterChanges
		{
			None = 0,
			AudioProcessingProfileParametersChanged = 1,
			VideoProcessingProfileParametersChanged = 2,
			GraphicsProcessingProfileParametersChanged = 4,
			CapabilitiesChanged = 8,
			OtherProfileParametersChanged = 16,
            NewAudioProcessingShouldBeAdded = 32,
            NewGraphicsProcessingShouldBeAdded = 64,
            NewVideoProcessingShouldBeAdded = 128,
		}

		private const TypesOfProfileParameterChanges AllTypesOfProfileParametersChanges =
			TypesOfProfileParameterChanges.AudioProcessingProfileParametersChanged |
			TypesOfProfileParameterChanges.GraphicsProcessingProfileParametersChanged |
			TypesOfProfileParameterChanges.VideoProcessingProfileParametersChanged |
			TypesOfProfileParameterChanges.CapabilitiesChanged |
			TypesOfProfileParameterChanges.OtherProfileParametersChanged;

		private TypesOfProfileParameterChanges typesOfChanges = TypesOfProfileParameterChanges.None;

		public override bool IsChanged => typesOfChanges != TypesOfProfileParameterChanges.None;

		public bool AudioProcessingProfileParametersChanged => typesOfChanges.HasFlag(TypesOfProfileParameterChanges.AudioProcessingProfileParametersChanged);

        public bool NewAudioProcessingNeedsToBeAdded => typesOfChanges.HasFlag(TypesOfProfileParameterChanges.NewAudioProcessingShouldBeAdded);

        public bool VideoProcessingProfileParametersHaveChanged => typesOfChanges.HasFlag(TypesOfProfileParameterChanges.VideoProcessingProfileParametersChanged);

        public bool NewVideoProcessingNeedsToBeAdded => typesOfChanges.HasFlag(TypesOfProfileParameterChanges.NewVideoProcessingShouldBeAdded);

        public bool GraphicsProcessingProfileParametersHaveChanged => typesOfChanges.HasFlag(TypesOfProfileParameterChanges.GraphicsProcessingProfileParametersChanged);

        public bool NewGraphicsProcessingNeedsToBeAdded => typesOfChanges.HasFlag(TypesOfProfileParameterChanges.NewGraphicsProcessingShouldBeAdded);

        public bool CapabilitiesChanged => typesOfChanges.HasFlag(TypesOfProfileParameterChanges.CapabilitiesChanged);

		public bool OtherProfileParametersChanged => typesOfChanges.HasFlag(TypesOfProfileParameterChanges.OtherProfileParametersChanged);

		public bool OnlyAudioProcessingProfileParametersChanged => typesOfChanges.HasFlag(AllTypesOfProfileParametersChanges);

		public bool OnlyVideoProcessingProfileParametersChanged => typesOfChanges.HasFlag(AllTypesOfProfileParametersChanges);

        public bool OnlyGraphicsProcessingProfileParametersChanged => typesOfChanges.HasFlag(AllTypesOfProfileParametersChanges);

		public bool OnlyCapabilitiesChanged => typesOfChanges.HasFlag(AllTypesOfProfileParametersChanges);

		public bool OnlyOtherProfileParametersChanged => typesOfChanges.HasFlag(AllTypesOfProfileParametersChanges);

		public void MarkAudioProcessingProfileParametersChanged()
		{
			typesOfChanges |= TypesOfProfileParameterChanges.AudioProcessingProfileParametersChanged | TypesOfProfileParameterChanges.CapabilitiesChanged;
		}

        public void MarkNewAudioProcessingNeeded()
        {
            typesOfChanges |= TypesOfProfileParameterChanges.NewAudioProcessingShouldBeAdded;
        }

        public void MarkVideoProcessingProfileParametersChanged()
		{
			typesOfChanges |= TypesOfProfileParameterChanges.VideoProcessingProfileParametersChanged | TypesOfProfileParameterChanges.CapabilitiesChanged;
		}

        public void MarkNewVideoProcessingNeeded()
        {
            typesOfChanges |= TypesOfProfileParameterChanges.NewVideoProcessingShouldBeAdded;
        }

        public void MarkGraphicsProcessingProfileParametersChanged()
		{
			typesOfChanges |= TypesOfProfileParameterChanges.GraphicsProcessingProfileParametersChanged | TypesOfProfileParameterChanges.CapabilitiesChanged;
		}

        public void MarkNewGraphicsProcessingNeeded()
        {
            typesOfChanges |= TypesOfProfileParameterChanges.NewGraphicsProcessingShouldBeAdded;
        }

        public void MarkCapabilitiesChanged()
		{
			typesOfChanges |= TypesOfProfileParameterChanges.CapabilitiesChanged;
		}

		public void MarkOtherProfileParametersChanged()
		{
			typesOfChanges |= TypesOfProfileParameterChanges.OtherProfileParametersChanged;
		}

		public void MarkChanges(ProfileParameter profileParameter, object oldValue, object newValue)
		{
			bool currentValueIsDefaultValue = newValue == null || newValue.Equals(profileParameter.DefaultValue?.StringValue ?? String.Empty);
			bool oldValueWasDefaultValue = oldValue == null || oldValue.Equals(profileParameter.DefaultValue?.StringValue ?? String.Empty);
			bool valueChangedFromDefaultToNonDefault = oldValueWasDefaultValue && !currentValueIsDefaultValue;

			if (ProfileParameterGuids.AllAudioProcessingRequiredGuids.Contains(profileParameter.Id))
			{
				MarkAudioProcessingChanges(valueChangedFromDefaultToNonDefault);
			}
			else if (profileParameter.Id == ProfileParameterGuids.VideoFormat)
			{
				MarkVideoProcessingChanges(valueChangedFromDefaultToNonDefault);
			}
			else if (profileParameter.Id == ProfileParameterGuids.RemoteGraphics)
			{
				MarkGraphicsProcessingChanges(valueChangedFromDefaultToNonDefault);
			}
			else if (profileParameter.IsCapability) MarkCapabilitiesChanged();
			else MarkOtherProfileParametersChanged();
		}

		private void MarkGraphicsProcessingChanges(bool valueChangedFromDefaultToNonDefault)
		{
			MarkGraphicsProcessingProfileParametersChanged();

			if (GraphicsProcessingProfileParametersHaveChanged && valueChangedFromDefaultToNonDefault)
			{
				MarkNewGraphicsProcessingNeeded();
			}
		}

		private void MarkVideoProcessingChanges(bool valueChangedFromDefaultToNonDefault)
		{
			MarkVideoProcessingProfileParametersChanged();

			if (VideoProcessingProfileParametersHaveChanged && valueChangedFromDefaultToNonDefault)
			{
				MarkNewVideoProcessingNeeded();
			}
		}

		private void MarkAudioProcessingChanges(bool valueChangedFromDefaultToNonDefault)
		{
			MarkAudioProcessingProfileParametersChanged();

			if (AudioProcessingProfileParametersChanged && valueChangedFromDefaultToNonDefault)
			{
				MarkNewAudioProcessingNeeded();
			}
		}

		public override bool TryAddChangeSummary(ChangeSummary changeSummaryToAdd)
		{
			if (changeSummaryToAdd is ProfileParameterChangeSummary profileParameterChangeSummaryToAdd)
			{
				typesOfChanges |= profileParameterChangeSummaryToAdd.typesOfChanges;
				return true;
			}

			return false;
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}

}
