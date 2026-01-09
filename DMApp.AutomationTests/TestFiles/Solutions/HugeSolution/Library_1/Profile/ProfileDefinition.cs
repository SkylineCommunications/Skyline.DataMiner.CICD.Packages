namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile
{
	using Library_1.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;

	public class ProfileDefinition : ICloneable
    {
        private ProfileDefinition()
        {

        }

		private ProfileDefinition(ProfileDefinition other)
		{
			ProfileParameters = other.ProfileParameters.Select(pp => pp.Clone()).Cast<ProfileParameter>().ToList();

			CloneHelper.CloneProperties(other, this);
		}

        public ProfileDefinition(Net.Profiles.ProfileDefinition srmProfileDefinition)
        {
            if (srmProfileDefinition == null) throw new ArgumentNullException(nameof(srmProfileDefinition));

            Id = srmProfileDefinition.ID;
            Name = srmProfileDefinition.Name;
            ProfileParameters = GetProfileParameters(srmProfileDefinition);
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<ProfileParameter> ProfileParameters { get; set; } = new List<ProfileParameter>();

        public static ProfileDefinition DummyProfileDefinition()
        {
            return new ProfileDefinition
            {
                Id = Guid.Empty,
                Name = "Dummy",
                ProfileParameters = GetDummyProfileParameters()
            };
        }

        private IEnumerable<ProfileParameter> GetProfileParameters(Net.Profiles.ProfileDefinition srmProfileDefinition)
        {
            var profileParameters = new HashSet<ProfileParameter>();

            if (srmProfileDefinition == null) return profileParameters;

            foreach (var profileParameter in srmProfileDefinition.Parameters)
            {
                var existingProfileParameter = profileParameters.FirstOrDefault(p => p.Equals(profileParameter));

                if (existingProfileParameter == null)
                {
                    profileParameters.Add(new ProfileParameter(profileParameter));
                }
            }

            return profileParameters;
        }

        private static IEnumerable<ProfileParameter> GetDummyProfileParameters()
        {
            return new List<ProfileParameter>
            {
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel1,
                    Name = "Audio Channel 1",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel1Description,
                    Name = "Audio Channel 1 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel2,
                    Name = "Audio Channel 2",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel2Description,
                    Name = "Audio Channel 2 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel3,
                    Name = "Audio Channel 3",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel3Description,
                    Name = "Audio Channel 3 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel4,
                    Name = "Audio Channel 4",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel4Description,
                    Name = "Audio Channel 4 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel5,
                    Name = "Audio Channel 5",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel5Description,
                    Name = "Audio Channel 5 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel6,
                    Name = "Audio Channel 6",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel6Description,
                    Name = "Audio Channel 6 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel7,
                    Name = "Audio Channel 7",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel7Description,
                    Name = "Audio Channel 7 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel8,
                    Name = "Audio Channel 8",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel8Description,
                    Name = "Audio Channel 8 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel9,
                    Name = "Audio Channel 9",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel9Description,
                    Name = "Audio Channel 9 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel10,
                    Name = "Audio Channel 10",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel10Description,
                    Name = "Audio Channel 10 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel11,
                    Name = "Audio Channel 11",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel11Description,
                    Name = "Audio Channel 11 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel12,
                    Name = "Audio Channel 12",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel12Description,
                    Name = "Audio Channel 12 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel13,
                    Name = "Audio Channel 13",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel13Description,
                    Name = "Audio Channel 13 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel14,
                    Name = "Audio Channel 14",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel14Description,
                    Name = "Audio Channel 14 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel15,
                    Name = "Audio Channel 15",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel15Description,
                    Name = "Audio Channel 15 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel16,
                    Name = "Audio Channel 16",
                    Value = "None"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioChannel16Description,
                    Name = "Audio Channel 16 Description",
                    Value = String.Empty
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.AudioDolbyDecodingRequired,
                    Name = "Audio Dolby Decoding Required",
                    Value = "No"
                },
                new ProfileParameter
                {
                    Id = ProfileParameterGuids.VideoFormat,
                    Name = "Video Format",
                    Value = "1080i50"
                }
            };
        }

		public object Clone()
		{
			return new ProfileDefinition(this);
		}
	}
}