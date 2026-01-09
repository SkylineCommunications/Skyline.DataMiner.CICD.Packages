namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
    using System;
    using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public abstract class ProcessingRelatedService : LiveVideoService
	{
		public static readonly string AudioProcessingFunctionRequiredTrueProfileParameterValue = "Yes";
		public static readonly string AudioProcessingFunctionRequiredFalseProfileParameterValue = "No";

		protected ProcessingRelatedService(Helpers helpers, Service service, LiveVideoOrder order) : base(helpers, service, order)
		{
		}

		public Profile.ProfileParameter VideoFormatProfileParameter => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.VideoFormat);

		public Profile.ProfileParameter RecordingVideoFormatProfileParameter => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.RecordingVideoFormat);

		/// <summary>
		/// The configured video format for this Live Video service.
		/// </summary>
		public string VideoFormat
		{
			get => (VideoFormatProfileParameter ?? RecordingVideoFormatProfileParameter)?.StringValue;
			set
			{
				var videoFormatParameters = GetAllServiceProfileParameters().Where(p => p.Id == ProfileParameterGuids.VideoFormat || p.Id == ProfileParameterGuids.RecordingVideoFormat).ToList();

				foreach (var videoFormatParameter in videoFormatParameters)
				{
					videoFormatParameter.Value = value;
				}
			}
		}

		/// <summary>
		/// Indicates what remote graphics are required.
		/// </summary>
		public string RemoteGraphics
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.RemoteGraphics)?.StringValue;
			set
			{
				var remoteGraphicsParameters = GetAllServiceProfileParameters().Where(p => p.Id == ProfileParameterGuids.RemoteGraphics).ToList();

				foreach (var remoteGraphicsParameter in remoteGraphicsParameters)
				{
					remoteGraphicsParameter.Value = value;
				}
			}
		}

		/// <summary>
		/// Indicates if audio embedding is required.
		/// </summary>
		public bool AudioEmbeddingRequired
		{
			get => AudioEmbeddingProfileParameter?.StringValue == AudioProcessingFunctionRequiredTrueProfileParameterValue;

			internal set
			{
				var audioParameter = AudioEmbeddingProfileParameter;

				if (audioParameter != null)
				{
					audioParameter.Value = value ? AudioProcessingFunctionRequiredTrueProfileParameterValue : AudioProcessingFunctionRequiredFalseProfileParameterValue;
				}
			}
		}

        public Profile.ProfileParameter AudioEmbeddingProfileParameter
        {
            get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioEmbeddingRequired);
		}

        /// <summary>
        /// Indicates if audio deembedding is required.
        /// </summary>
        public bool AudioDeembeddingRequired
		{
			get => AudioDeembeddingProfileParameter?.StringValue == AudioProcessingFunctionRequiredTrueProfileParameterValue;

			internal set
			{
				var audioParameter = AudioDeembeddingProfileParameter;

				if (audioParameter != null)
				{
					audioParameter.Value = value ? AudioProcessingFunctionRequiredTrueProfileParameterValue : AudioProcessingFunctionRequiredFalseProfileParameterValue;
				}
			}
		}

		public Profile.ProfileParameter AudioDeembeddingProfileParameter
        {
            get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioDeembeddingRequired);
		}

		/// <summary>
		/// Indicates if audio shuffling is required.
		/// </summary>
		public bool AudioShufflingRequired
		{
			get => AudioShufflingProfileParameter?.StringValue == AudioProcessingFunctionRequiredTrueProfileParameterValue;

			internal set
			{
				var audioParameter = AudioShufflingProfileParameter;

				if (audioParameter != null)
				{
					audioParameter.Value = value ? AudioProcessingFunctionRequiredTrueProfileParameterValue : AudioProcessingFunctionRequiredFalseProfileParameterValue;
				}
			}
		}

        public Profile.ProfileParameter AudioShufflingProfileParameter
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioShufflingRequired);
		}

        /// <summary>
        /// Indicates if audio dolby decoding is required.
        /// </summary>
        public bool AudioDolbyDecodingRequired
		{
			get => AudioDolbyDecodingProfileParameter?.StringValue == AudioProcessingFunctionRequiredTrueProfileParameterValue;

			internal set
			{
				var audioDolbyDecodingParameter = AudioDolbyDecodingProfileParameter;

				if (audioDolbyDecodingParameter != null)
				{
					audioDolbyDecodingParameter.Value = value ? AudioProcessingFunctionRequiredTrueProfileParameterValue : AudioProcessingFunctionRequiredFalseProfileParameterValue;
				}
			}
		}

		public Profile.ProfileParameter AudioDolbyDecodingProfileParameter
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioDolbyDecodingRequired);
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 1.
		/// </summary>
		public string AudioChannel1 
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 2.
		/// </summary>
		public string AudioChannel2
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel2)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 3.
		/// </summary>
		public string AudioChannel3
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel3)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 4.
		/// </summary>
		public string AudioChannel4
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel4)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 5.
		/// </summary>
		public string AudioChannel5
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel5)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 6.
		/// </summary>
		public string AudioChannel6
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel6)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 7.
		/// </summary>
		public string AudioChannel7
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel7)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 8.
		/// </summary>
		public string AudioChannel8
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel8)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 9.
		/// </summary>
		public string AudioChannel9
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel9)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 10.
		/// </summary>
		public string AudioChannel10
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel10)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 11.
		/// </summary>
		public string AudioChannel11
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel11)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 12.
		/// </summary>
		public string AudioChannel12
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel12)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 13.
		/// </summary>
		public string AudioChannel13
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel13)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 14.
		/// </summary>
		public string AudioChannel14
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel14)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 15.
		/// </summary>
		public string AudioChannel15
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel15)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}

		/// <summary>
		/// Indicates the configuration for Audio Channel 16.
		/// </summary>
		public string AudioChannel16
		{
			get => GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel16)?.StringValue ?? String.Empty;
			protected set
			{
				var audioChannelProfileParameter = GetAllServiceProfileParameters().FirstOrDefault(p => p.Id == ProfileParameterGuids.AudioChannel1);

				if (audioChannelProfileParameter != null) audioChannelProfileParameter.Value = value;
			}
		}
	}
}
