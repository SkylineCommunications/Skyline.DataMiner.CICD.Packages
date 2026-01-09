namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.Library.Exceptions;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class AudioProcessingService : RoutingRequiringService
	{
		public const string AudioEmbeddingFunctionDefinitionName = "Audio Embedding";
		public const string AudioDeembeddingFunctionDefinitionName = "Audio Deembedding";
		public const string AudioShufflingFunctionDefinitionName = "Audio Shuffling";
		public const string AudioDolbyDecodingFunctionDefinitionName = "Audio Dolby Decoding";

		public static readonly int MatrixSdiInputAudioDeembeddingFunctionNodeId = 3;

		public static readonly int MatrixSdiOutputAudioDolbyDecodingFunctionNodeId = 2;

		public static readonly int MatrixSdiInputAudioDolbyDecodingFunctionNodeId = 7;

		public static readonly int MatrixSdiOutputAudioShufflingFunctionNodeId = 6;

		public static readonly int MatrixSdiInputAudioShufflingFunctionNodeId = 10;

		public static readonly int MatrixSdiOutputAudioEmbeddingFunctionNodeId = 9;

		private FunctionResource inputResource;

		private FunctionResource outputResource;

		private readonly Dictionary<Guid, string> audioChannelValues = new Dictionary<Guid, string>
		{
			{ ProfileParameterGuids.InputAudioChannel1, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel2, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel3, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel4, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel5, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel6, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel7, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel8, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel9, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel10, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel11, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel12, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel13, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel14, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel15, String.Empty },
			{ ProfileParameterGuids.InputAudioChannel16, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel1, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel2, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel3, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel4, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel5, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel6, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel7, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel8, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel9, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel10, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel11, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel12, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel13, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel14, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel15, String.Empty },
			{ ProfileParameterGuids.OutputAudioChannel16, String.Empty },
		};

		public AudioProcessingService(Helpers helpers, Service service, LiveVideoOrder liveVideoOrder)
			: base(helpers, service, liveVideoOrder)
		{
			UpdateInputOutputResource(service.Functions);

			foreach (var function in service.Functions)
			{
				foreach (var parameter in function.Parameters)
				{
					if (!audioChannelValues.ContainsKey(parameter.Id)) continue;
					audioChannelValues[parameter.Id] = Convert.ToString(parameter.Value);
				}
			}
		}

		public string InputAudioChannel1
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel1];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel1] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel1, value);
			}
		}

		public string InputAudioChannel2
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel2];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel2] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel2, value);
			}
		}

		public string InputAudioChannel3
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel3];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel3] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel3, value);
			}
		}

		public string InputAudioChannel4
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel4];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel4] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel4, value);
			}
		}

		public string InputAudioChannel5
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel5];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel5] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel5, value);
			}
		}

		public string InputAudioChannel6
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel6];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel6] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel6, value);
			}
		}

		public string InputAudioChannel7
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel7];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel7] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel7, value);
			}
		}

		public string InputAudioChannel8
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel8];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel8] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel8, value);
			}
		}

		public string InputAudioChannel9
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel9];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel9] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel9, value);
			}
		}

		public string InputAudioChannel10
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel10];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel10] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel10, value);
			}
		}

		public string InputAudioChannel11
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel11];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel11] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel11, value);
			}
		}

		public string InputAudioChannel12
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel12];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel12] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel12, value);
			}
		}

		public string InputAudioChannel13
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel13];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel13] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel13, value);
			}
		}

		public string InputAudioChannel14
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel14];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel14] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel14, value);
			}
		}

		public string InputAudioChannel15
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel15];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel15] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel15, value);
			}
		}

		public string InputAudioChannel16
		{
			get => audioChannelValues[ProfileParameterGuids.InputAudioChannel16];

			set
			{
				audioChannelValues[ProfileParameterGuids.InputAudioChannel16] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.InputAudioChannel16, value);
			}
		}

		public string OutputAudioChannel1
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel1];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel1] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel1, value);
			}
		}

		public string OutputAudioChannel2
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel2];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel2] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel2, value);
			}
		}

		public string OutputAudioChannel3
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel3];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel3] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel3, value);
			}
		}

		public string OutputAudioChannel4
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel4];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel4] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel4, value);
			}
		}

		public string OutputAudioChannel5
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel5];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel5] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel5, value);
			}
		}

		public string OutputAudioChannel6
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel6];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel6] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel6, value);
			}
		}

		public string OutputAudioChannel7
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel7];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel7] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel7, value);
			}
		}

		public string OutputAudioChannel8
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel8];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel8] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel8, value);
			}
		}

		public string OutputAudioChannel9
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel9];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel9] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel9, value);
			}
		}

		public string OutputAudioChannel10
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel10];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel10] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel10, value);
			}
		}

		public string OutputAudioChannel11
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel11];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel11] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel11, value);
			}
		}

		public string OutputAudioChannel12
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel12];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel12] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel12, value);
			}
		}

		public string OutputAudioChannel13
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel13];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel13] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel13, value);
			}
		}

		public string OutputAudioChannel14
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel14];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel14] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel14, value);
			}
		}

		public string OutputAudioChannel15
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel15];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel15] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel15, value);
			}
		}

		public string OutputAudioChannel16
		{
			get => audioChannelValues[ProfileParameterGuids.OutputAudioChannel16];

			set
			{
				audioChannelValues[ProfileParameterGuids.OutputAudioChannel16] = value;
				UpdateServiceAudioChannelParameter(ProfileParameterGuids.OutputAudioChannel16, value);
			}
		}

		/// <summary>
		/// The input resource for this Audio Processing Service.
		/// This overrides the input resource set when initializing the Live Video Services as an Audio Processing service doesn't require all functions.
		/// </summary>
		public override FunctionResource InputResource
		{
			get
			{
				return inputResource;
			}
		}

		/// <summary>
		/// The output resource for this Audio Processing Service.
		/// This overrides the output resource set when initializing the Live Video Services as an Audio Processing service doesn't require all functions.
		/// </summary>
		public override FunctionResource OutputResource
		{
			get
			{
				return outputResource;
			}
		}

		public Function.Function AudioEmbeddingFunction
		{
			get => Service.Functions.FirstOrDefault(f => f.Name == AudioEmbeddingFunctionDefinitionName);
		}

		public Function.Function AudioDeEmbeddingFunction
		{
			get => Service.Functions.FirstOrDefault(f => f.Name == AudioDeembeddingFunctionDefinitionName);
		}

		public Function.Function AudioShufflingFunction
		{
			get => Service.Functions.FirstOrDefault(f => f.Name == AudioShufflingFunctionDefinitionName);
		}

		public Function.Function AudioDolbyDecodingFunction
		{
			get => Service.Functions.FirstOrDefault(f => f.Name == AudioDolbyDecodingFunctionDefinitionName);
		}

		public static AudioProcessingService GenerateNewAudioProcessingService(Helpers helpers, SourceService source, EndPointService child)
		{
			var serviceDefinition = helpers.ServiceDefinitionManager.AudioProcessingServiceDefinition ?? throw new ServiceDefinitionNotFoundException("Unable to find Audio Processing SD");

			var service = new DisplayedService
			{
				Start = child.Service.Start,
				End = child.Service.End,
				PreRoll = GetPreRoll(serviceDefinition, source.Service),
				PostRoll = ServiceManager.GetPostRollDuration(serviceDefinition),
				Definition = serviceDefinition,
				BackupType = source.Service.BackupType
			};

			foreach (var node in serviceDefinition.Diagram.Nodes)
			{
				var functionDefinition = serviceDefinition.FunctionDefinitions.Single(fd => fd.Label == node.Label);

				var function = new Function.DisplayedFunction(helpers, node, functionDefinition);

				CopyProfileParameterValues(function, source, child);

				service.Functions.Add(function);
			}

			helpers.Log(nameof(AudioProcessingService), nameof(GenerateNewAudioProcessingService), $"Generated new audio processing service: '{service.GetConfiguration().Serialize()}'");

			var audioProcessingService = new AudioProcessingService(helpers, service, source.LiveVideoOrder) { IsNew = true };

			audioProcessingService.Service.AcceptChanges();

			return audioProcessingService;
		}

		private static void CopyProfileParameterValues(Function.DisplayedFunction function, SourceService source, EndPointService child)
		{
			switch (function.Name)
			{
				case AudioDeembeddingFunctionDefinitionName:
					function.RequiresResource = child.AudioDeembeddingRequired;
					function.Parameters.Single(p => p.Id == ProfileParameterGuids.AudioDeembeddingRequired).Value = child.AudioDeembeddingRequired ? AudioProcessingFunctionRequiredTrueProfileParameterValue : AudioProcessingFunctionRequiredFalseProfileParameterValue;
					break;
				case AudioDolbyDecodingFunctionDefinitionName:
					function.RequiresResource = source.AudioDolbyDecodingRequired;
					function.Parameters.Single(p => p.Id == ProfileParameterGuids.AudioDolbyDecodingRequired).Value = source.AudioDolbyDecodingRequired ? AudioProcessingFunctionRequiredTrueProfileParameterValue : AudioProcessingFunctionRequiredFalseProfileParameterValue;
					break;
				case AudioShufflingFunctionDefinitionName:
					CopyAudioShufflingProfileParameterValues(function, source, child);
					break;
				case AudioEmbeddingFunctionDefinitionName:
					function.RequiresResource = child.AudioEmbeddingRequired;
					function.Parameters.Single(p => p.Id == ProfileParameterGuids.AudioEmbeddingRequired).Value = child.AudioEmbeddingRequired ? AudioProcessingFunctionRequiredTrueProfileParameterValue : AudioProcessingFunctionRequiredFalseProfileParameterValue;
					break;
				case "Matrix Input SDI":
					CopyMatrixInputSdiProfileParameterValues(function, source, child);
					break;

				case "Matrix Output SDI":
					CopyMatrixOutputSdiProfileParameterValues(function, source, child);
					break;
				default:
					throw new NotSupportedException($"Function {function.Name} ({function.Id}) is not supported for Audio Processing services");
			}
		}

		private static void CopyAudioShufflingProfileParameterValues(Function.DisplayedFunction function, SourceService source, EndPointService child)
		{
			function.RequiresResource = child.AudioShufflingRequired;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.AudioShufflingRequired).Value = child.AudioShufflingRequired ? AudioProcessingFunctionRequiredTrueProfileParameterValue : AudioProcessingFunctionRequiredFalseProfileParameterValue;

			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel1).Value = source.AudioChannel1;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel2).Value = source.AudioChannel2;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel3).Value = source.AudioChannel3;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel4).Value = source.AudioChannel4;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel5).Value = source.AudioChannel5;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel6).Value = source.AudioChannel6;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel7).Value = source.AudioChannel7;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel8).Value = source.AudioChannel8;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel9).Value = source.AudioChannel9;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel10).Value = source.AudioChannel10;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel11).Value = source.AudioChannel11;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel12).Value = source.AudioChannel12;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel13).Value = source.AudioChannel13;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel14).Value = source.AudioChannel14;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel15).Value = source.AudioChannel15;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.InputAudioChannel16).Value = source.AudioChannel16;

			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel1).Value = child.AudioChannel1;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel2).Value = child.AudioChannel2;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel3).Value = child.AudioChannel3;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel4).Value = child.AudioChannel4;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel5).Value = child.AudioChannel5;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel6).Value = child.AudioChannel6;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel7).Value = child.AudioChannel7;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel8).Value = child.AudioChannel8;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel9).Value = child.AudioChannel9;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel10).Value = child.AudioChannel10;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel11).Value = child.AudioChannel11;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel12).Value = child.AudioChannel12;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel13).Value = child.AudioChannel13;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel14).Value = child.AudioChannel14;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel15).Value = child.AudioChannel15;
			function.Parameters.Single(p => p.Id == ProfileParameterGuids.OutputAudioChannel16).Value = child.AudioChannel16;
		}

		private static void CopyMatrixInputSdiProfileParameterValues(Function.DisplayedFunction function, SourceService source, EndPointService child)
		{
			if (function.Definition.Label.Contains(AudioDeembeddingFunctionDefinitionName))
				function.RequiresResource = child.AudioDeembeddingRequired && (source.AudioDolbyDecodingRequired || child.AudioShufflingRequired || child.AudioEmbeddingRequired);
			else if (function.Definition.Label.Contains(AudioDolbyDecodingFunctionDefinitionName))
				function.RequiresResource = source.AudioDolbyDecodingRequired && (child.AudioShufflingRequired || child.AudioEmbeddingRequired);
			else if (function.Definition.Label.Contains(AudioShufflingFunctionDefinitionName))
				function.RequiresResource = child.AudioShufflingRequired && child.AudioEmbeddingRequired;
			else
			{
				// Nothing to copy
			}
		}

		private static void CopyMatrixOutputSdiProfileParameterValues(Function.DisplayedFunction function, SourceService source, EndPointService child)
		{
			if (function.Definition.Label.Contains(AudioDolbyDecodingFunctionDefinitionName))
				function.RequiresResource = source.AudioDolbyDecodingRequired && child.AudioDeembeddingRequired;
			else if (function.Definition.Label.Contains(AudioShufflingFunctionDefinitionName))
				function.RequiresResource = child.AudioShufflingRequired && (child.AudioDeembeddingRequired || source.AudioDolbyDecodingRequired);
			else if (function.Definition.Label.Contains(AudioEmbeddingFunctionDefinitionName))
				function.RequiresResource = child.AudioEmbeddingRequired && (child.AudioDeembeddingRequired || source.AudioDolbyDecodingRequired || child.AudioShufflingRequired);
			else
			{
				// Nothing to copy
			}
		}

		public void UpdateValuesBasedOn(EndPointService endPointService)
		{
			if (endPointService is null) throw new ArgumentNullException(nameof(endPointService));

			UpdateFunctions(endPointService);
			Log(nameof(UpdateValuesBasedOn), $"Updated functions based on {endPointService.Service.Name}");

			if (!LiveVideoOrder.Order.ConvertedFromRunningToStartNow)
			{
				Service.Start = endPointService.Service.Start;
				Service.End = endPointService.Service.End;
				Log(nameof(UpdateValuesBasedOn), $"Updated timing to {Service.TimingInfoToString(Service)} based on {endPointService.Service.Name}");
			}
		}

		/// <summary>
		/// Updates the audio processing configuration of the linked end point services.
		/// Update is needed to make sure add or update/book services flow use the correct requirements when resources got changed through update service.
		/// </summary>
		public void UpdateEndPointServicesAudioProcessingConfiguration(List<EndPointService> endPointServicesUsingThisAudioProcessing)
		{
			foreach (var endPointService in endPointServicesUsingThisAudioProcessing)
			{
				endPointService?.UpdateEmbeddingBasedOnLinkedAudioProcessingService();
				endPointService?.UpdateDeEmbeddingBasedOnLinkedAudioProcessingService();
				endPointService?.UpdateAudioShufflingBasedOnLinkedAudioProcessingService();
			}
		}

		/// <summary>
		/// Updates the audio processing Dolby Decoding configuration of the linked source service.
		/// Update is needed to make sure add or update/book services flow use the correct requirements when resources got changed through update service.
		/// </summary>
		public void UpdateSourceDolbyDecodingRequirement()
		{
			var audioProcessingAudioDolbyDecodingFunction = AudioDolbyDecodingFunction;
			bool audioDolbyDecodingRequired = audioProcessingAudioDolbyDecodingFunction?.Resource != null && audioProcessingAudioDolbyDecodingFunction.Name != Constants.None;

			Helpers.Log(nameof(EndPointService), nameof(UpdateSourceDolbyDecodingRequirement), $"Source audio dolby decoding required value will be set to: {audioDolbyDecodingRequired}");

			if (Source is null)
			{
				Log(nameof(UpdateSourceDolbyDecodingRequirement), $"WARNING: Source is null for {Service.Name}");
			}
			else
			{
				Source.AudioDolbyDecodingRequired = audioDolbyDecodingRequired;
			}
		}

		/// <summary>
		/// Update the input and output resource based on what functions of the Audio Processing service are required.
		/// </summary>
		/// <param name="functions">The functions of the Audio Processing service.</param>
		private void UpdateInputOutputResource(List<Function.Function> functions)
		{
			if (UpdateAudioDeembeddingInputResource(functions)) return;
			if (UpdateAudioDolbyDecodingResource(functions)) return;
			if (UpdateAudioShufflingResource(functions)) return;
			UpdateAudioEmbeddingResource(functions);
		}

		private bool UpdateAudioDeembeddingInputResource(List<Function.Function> functions)
		{
			if (!AudioDeembeddingRequired) return false;

			var audioDeembeddingFunction = functions.FirstOrDefault(f => f.Name == AudioDeembeddingFunctionDefinitionName);
			if (audioDeembeddingFunction != null) inputResource = audioDeembeddingFunction.Resource;
			if (!AudioDolbyDecodingRequired && !AudioShufflingRequired && !AudioEmbeddingRequired && audioDeembeddingFunction != null)
			{
				outputResource = audioDeembeddingFunction.Resource;
				return true;
			}

			return false;
		}

		private bool UpdateAudioDolbyDecodingResource(List<Function.Function> functions)
		{
			if (!AudioDolbyDecodingRequired) return false;

			var audioDolbyDecodingFunction = functions.FirstOrDefault(f => f.Name == AudioDolbyDecodingFunctionDefinitionName);
			if (!AudioDeembeddingRequired && audioDolbyDecodingFunction != null)
				inputResource = audioDolbyDecodingFunction.Resource;
			if (!AudioShufflingRequired && !AudioEmbeddingRequired && audioDolbyDecodingFunction != null)
			{
				outputResource = audioDolbyDecodingFunction.Resource;
				return true;
			}

			return false;
		}

		private bool UpdateAudioShufflingResource(List<Function.Function> functions)
		{
			if (!AudioShufflingRequired) return false;

			var audioShufflingFunction = functions.FirstOrDefault(f => f.Name == AudioShufflingFunctionDefinitionName);
			if (!AudioDeembeddingRequired && !AudioDolbyDecodingRequired && audioShufflingFunction != null)
				inputResource = audioShufflingFunction.Resource;

			if (!AudioEmbeddingRequired && audioShufflingFunction != null)
			{
				outputResource = audioShufflingFunction.Resource;
				return true;
			}

			return false;
		}

		private void UpdateAudioEmbeddingResource(List<Function.Function> functions)
		{
			if (AudioEmbeddingRequired)
			{
				var audioEmbeddingFunction = functions.FirstOrDefault(f => f.Name == AudioEmbeddingFunctionDefinitionName);
				if (!AudioDeembeddingRequired && !AudioDolbyDecodingRequired && !AudioShufflingRequired && audioEmbeddingFunction != null)
					inputResource = audioEmbeddingFunction.Resource;
				if (audioEmbeddingFunction != null) outputResource = audioEmbeddingFunction.Resource;
			}
		}

		public bool HasEqualValuesAs(AudioProcessingService second)
		{
			bool isMatching = true;

			isMatching &= AudioEmbeddingRequired == second.AudioEmbeddingRequired;
			isMatching &= AudioDeembeddingRequired == second.AudioDeembeddingRequired;
			isMatching &= AudioShufflingRequired == second.AudioShufflingRequired;
			isMatching &= AudioDolbyDecodingRequired == second.AudioDolbyDecodingRequired;

			Log(nameof(HasEqualValuesAs), $"This service has {nameof(AudioEmbeddingRequired)}='{AudioEmbeddingRequired}', {nameof(AudioDeembeddingRequired)}='{AudioDeembeddingRequired}', {nameof(AudioShufflingRequired)}='{AudioShufflingRequired}', {nameof(AudioDolbyDecodingRequired)}='{AudioDolbyDecodingRequired}'. \nSecond service {second.Service.Name} has {nameof(AudioEmbeddingRequired)}='{second.AudioEmbeddingRequired}', {nameof(AudioDeembeddingRequired)}='{second.AudioDeembeddingRequired}', {nameof(AudioShufflingRequired)}='{second.AudioShufflingRequired}', {nameof(AudioDolbyDecodingRequired)}='{second.AudioDolbyDecodingRequired}'.");

			isMatching &= second.InputAudioChannel1 == InputAudioChannel1;
			isMatching &= second.InputAudioChannel2 == InputAudioChannel2;
			isMatching &= second.InputAudioChannel3 == InputAudioChannel3;
			isMatching &= second.InputAudioChannel4 == InputAudioChannel4;
			isMatching &= second.InputAudioChannel5 == InputAudioChannel5;
			isMatching &= second.InputAudioChannel6 == InputAudioChannel6;
			isMatching &= second.InputAudioChannel7 == InputAudioChannel7;
			isMatching &= second.InputAudioChannel8 == InputAudioChannel8;
			isMatching &= second.InputAudioChannel9 == InputAudioChannel9;
			isMatching &= second.InputAudioChannel10 == InputAudioChannel10;
			isMatching &= second.InputAudioChannel11 == InputAudioChannel11;
			isMatching &= second.InputAudioChannel12 == InputAudioChannel12;
			isMatching &= second.InputAudioChannel13 == InputAudioChannel13;
			isMatching &= second.InputAudioChannel14 == InputAudioChannel14;
			isMatching &= second.InputAudioChannel15 == InputAudioChannel15;
			isMatching &= second.InputAudioChannel16 == InputAudioChannel16;

			Log(nameof(HasEqualValuesAs), $"This service has {nameof(InputAudioChannel1)}='{InputAudioChannel1}', {nameof(InputAudioChannel2)}='{InputAudioChannel2}', {nameof(InputAudioChannel3)}='{InputAudioChannel3}', {nameof(InputAudioChannel4)}='{InputAudioChannel4}', {nameof(InputAudioChannel5)}='{InputAudioChannel5}', {nameof(InputAudioChannel6)}='{InputAudioChannel6}', {nameof(InputAudioChannel7)}='{InputAudioChannel7}', {nameof(InputAudioChannel8)}='{InputAudioChannel8}', {nameof(InputAudioChannel9)}='{InputAudioChannel9}', {nameof(InputAudioChannel10)}='{InputAudioChannel10}', {nameof(InputAudioChannel11)}='{InputAudioChannel11}', {nameof(InputAudioChannel12)}='{InputAudioChannel12}', {nameof(InputAudioChannel13)}='{InputAudioChannel13}', {nameof(InputAudioChannel14)}='{InputAudioChannel14}', {nameof(InputAudioChannel15)}='{InputAudioChannel15}', {nameof(InputAudioChannel16)}='{InputAudioChannel16}'. \nSecond service {second.Service.Name} has {nameof(InputAudioChannel1)}='{second.InputAudioChannel1}', {nameof(InputAudioChannel2)}='{second.InputAudioChannel2}', {nameof(InputAudioChannel3)}='{second.InputAudioChannel3}', {nameof(InputAudioChannel4)}='{second.InputAudioChannel4}', {nameof(InputAudioChannel5)}='{second.InputAudioChannel5}', {nameof(InputAudioChannel6)}='{second.InputAudioChannel6}', {nameof(InputAudioChannel7)}='{second.InputAudioChannel7}', {nameof(InputAudioChannel8)}='{second.InputAudioChannel8}', {nameof(InputAudioChannel9)}='{second.InputAudioChannel9}', {nameof(InputAudioChannel10)}='{second.InputAudioChannel10}', {nameof(InputAudioChannel11)}='{second.InputAudioChannel11}', {nameof(InputAudioChannel12)}='{second.InputAudioChannel12}', {nameof(InputAudioChannel13)}='{second.InputAudioChannel13}', {nameof(InputAudioChannel14)}='{second.InputAudioChannel14}', {nameof(InputAudioChannel15)}='{second.InputAudioChannel15}', {nameof(InputAudioChannel16)}='{second.InputAudioChannel16}'.");

			isMatching &= second.OutputAudioChannel1 == OutputAudioChannel1;
			isMatching &= second.OutputAudioChannel2 == OutputAudioChannel2;
			isMatching &= second.OutputAudioChannel3 == OutputAudioChannel3;
			isMatching &= second.OutputAudioChannel4 == OutputAudioChannel4;
			isMatching &= second.OutputAudioChannel5 == OutputAudioChannel5;
			isMatching &= second.OutputAudioChannel6 == OutputAudioChannel6;
			isMatching &= second.OutputAudioChannel7 == OutputAudioChannel7;
			isMatching &= second.OutputAudioChannel8 == OutputAudioChannel8;
			isMatching &= second.OutputAudioChannel9 == OutputAudioChannel9;
			isMatching &= second.OutputAudioChannel10 == OutputAudioChannel10;
			isMatching &= second.OutputAudioChannel11 == OutputAudioChannel11;
			isMatching &= second.OutputAudioChannel12 == OutputAudioChannel12;
			isMatching &= second.OutputAudioChannel13 == OutputAudioChannel13;
			isMatching &= second.OutputAudioChannel14 == OutputAudioChannel14;
			isMatching &= second.OutputAudioChannel15 == OutputAudioChannel15;
			isMatching &= second.OutputAudioChannel16 == OutputAudioChannel16;

			Log(nameof(HasEqualValuesAs), $"This service has {nameof(OutputAudioChannel1)}='{OutputAudioChannel1}', {nameof(OutputAudioChannel2)}='{OutputAudioChannel2}', {nameof(OutputAudioChannel3)}='{OutputAudioChannel3}', {nameof(OutputAudioChannel4)}='{OutputAudioChannel4}', {nameof(OutputAudioChannel5)}='{OutputAudioChannel5}', {nameof(OutputAudioChannel6)}='{OutputAudioChannel6}', {nameof(OutputAudioChannel7)}='{OutputAudioChannel7}', {nameof(OutputAudioChannel8)}='{OutputAudioChannel8}', {nameof(OutputAudioChannel9)}='{OutputAudioChannel9}', {nameof(OutputAudioChannel10)}='{OutputAudioChannel10}', {nameof(OutputAudioChannel11)}='{OutputAudioChannel11}', {nameof(OutputAudioChannel12)}='{OutputAudioChannel12}', {nameof(OutputAudioChannel13)}='{OutputAudioChannel13}', {nameof(OutputAudioChannel14)}='{OutputAudioChannel14}', {nameof(OutputAudioChannel15)}='{OutputAudioChannel15}', {nameof(OutputAudioChannel16)}='{OutputAudioChannel16}'. \nSecond service {second.Service.Name} has {nameof(OutputAudioChannel1)}='{second.OutputAudioChannel1}', {nameof(OutputAudioChannel2)}='{second.OutputAudioChannel2}', {nameof(OutputAudioChannel3)}='{second.OutputAudioChannel3}', {nameof(OutputAudioChannel4)}='{second.OutputAudioChannel4}', {nameof(OutputAudioChannel5)}='{second.OutputAudioChannel5}', {nameof(OutputAudioChannel6)}='{second.OutputAudioChannel6}', {nameof(OutputAudioChannel7)}='{second.OutputAudioChannel7}', {nameof(OutputAudioChannel8)}='{second.OutputAudioChannel8}', {nameof(OutputAudioChannel9)}='{second.OutputAudioChannel9}', {nameof(OutputAudioChannel10)}='{second.OutputAudioChannel10}', {nameof(OutputAudioChannel11)}='{second.OutputAudioChannel11}', {nameof(OutputAudioChannel12)}='{second.OutputAudioChannel12}', {nameof(OutputAudioChannel13)}='{second.OutputAudioChannel13}', {nameof(OutputAudioChannel14)}='{second.OutputAudioChannel14}', {nameof(OutputAudioChannel15)}='{second.OutputAudioChannel15}', {nameof(OutputAudioChannel16)}='{second.OutputAudioChannel16}'.");

			return isMatching;
		}

		public bool HasMatchingConfiguration(EndPointService child)
		{
			bool isMatching = true;

			isMatching &= AudioEmbeddingRequired == child.AudioEmbeddingRequired;
			isMatching &= AudioDeembeddingRequired == child.AudioDeembeddingRequired;
			isMatching &= AudioShufflingRequired == child.AudioShufflingRequired;
			isMatching &= AudioDolbyDecodingRequired == Source.AudioDolbyDecodingRequired;

			if (AudioShufflingRequired && !AudioDolbyDecodingRequired)
			{
				isMatching &= AreShufflingInputChannelsMatching();
				isMatching &= AreShufflingOutputChannelsMatching(child);
			}

			if (Service.Start > child.Service.Start || Service.End < child.Service.End)
			{
				isMatching = false;
			}

			return isMatching;
		}

		private bool AreShufflingOutputChannelsMatching(EndPointService child)
		{
			bool isMatching = true;

			isMatching &= child.AudioChannel1 == OutputAudioChannel1;
			isMatching &= child.AudioChannel2 == OutputAudioChannel2;
			isMatching &= child.AudioChannel3 == OutputAudioChannel3;
			isMatching &= child.AudioChannel4 == OutputAudioChannel4;
			isMatching &= child.AudioChannel5 == OutputAudioChannel5;
			isMatching &= child.AudioChannel6 == OutputAudioChannel6;
			isMatching &= child.AudioChannel7 == OutputAudioChannel7;
			isMatching &= child.AudioChannel8 == OutputAudioChannel8;

			isMatching &= child.AudioChannel9 == OutputAudioChannel9;
			isMatching &= child.AudioChannel10 == OutputAudioChannel10;
			isMatching &= child.AudioChannel11 == OutputAudioChannel11;
			isMatching &= child.AudioChannel12 == OutputAudioChannel12;
			isMatching &= child.AudioChannel13 == OutputAudioChannel13;
			isMatching &= child.AudioChannel14 == OutputAudioChannel14;
			isMatching &= child.AudioChannel15 == OutputAudioChannel15;
			isMatching &= child.AudioChannel16 == OutputAudioChannel16;

			return isMatching;
		}

		private bool AreShufflingInputChannelsMatching()
		{
			bool isMatching = true;

			isMatching &= Source.AudioChannel1 == InputAudioChannel1;
			isMatching &= Source.AudioChannel2 == InputAudioChannel2;
			isMatching &= Source.AudioChannel3 == InputAudioChannel3;
			isMatching &= Source.AudioChannel4 == InputAudioChannel4;
			isMatching &= Source.AudioChannel5 == InputAudioChannel5;
			isMatching &= Source.AudioChannel6 == InputAudioChannel6;
			isMatching &= Source.AudioChannel7 == InputAudioChannel7;
			isMatching &= Source.AudioChannel8 == InputAudioChannel8;

			isMatching &= Source.AudioChannel9 == InputAudioChannel9;
			isMatching &= Source.AudioChannel10 == InputAudioChannel10;
			isMatching &= Source.AudioChannel11 == InputAudioChannel11;
			isMatching &= Source.AudioChannel12 == InputAudioChannel12;
			isMatching &= Source.AudioChannel13 == InputAudioChannel13;
			isMatching &= Source.AudioChannel14 == InputAudioChannel14;
			isMatching &= Source.AudioChannel15 == InputAudioChannel15;
			isMatching &= Source.AudioChannel16 == InputAudioChannel16;

			return isMatching;
		}

		private void UpdateFunctions(EndPointService child)
		{
			UpdateEmbedding(child);
			UpdateDeembedding(child);

			// dolby decoding needs to be updated first because it could be this will disable shuffling in the child service
			UpdateDolbyDecoding(Source, child);
			UpdateShuffling(Source, child);

			UpdateMatrixFunctions(Source, child);
		}

		private void UpdateEmbedding(EndPointService child)
		{
			Helpers?.Log(nameof(AudioProcessingService), nameof(UpdateEmbedding), $"Update Embedding required value to: {child.AudioEmbeddingRequired}");

			AudioEmbeddingRequired = child.AudioEmbeddingRequired;

			UpdateResourceRequirements(AudioEmbeddingFunctionDefinitionName, child.AudioEmbeddingRequired);
		}

		private void UpdateDeembedding(EndPointService child)
		{
			Helpers?.Log(nameof(AudioProcessingService), nameof(UpdateDeembedding), $"Update DeEmbedding required value to: {child.AudioDeembeddingRequired}");

			AudioDeembeddingRequired = child.AudioDeembeddingRequired;

			UpdateResourceRequirements(AudioDeembeddingFunctionDefinitionName, child.AudioDeembeddingRequired);
		}

		private void UpdateShuffling(SourceService source, EndPointService child)
		{
			Helpers?.Log(nameof(AudioProcessingService), nameof(UpdateShuffling), $"Update audio shuffling required value to: {child.AudioShufflingRequired}");

			AudioShufflingRequired = child.AudioShufflingRequired;

			UpdateResourceRequirements(AudioShufflingFunctionDefinitionName, child.AudioShufflingRequired);

			// when no audio shuffling is needed then the remaining parameters don't need to be updated
			// when audio dolby decoding is needed and shuffing is also needed then the reamining parameters need to be manually configured
			if (!AudioShufflingRequired || AudioDolbyDecodingRequired) return;

			UpdateShufflingInputChannels1To8(source);
			UpdateShufflingInputChannels9To16(source);

			UpdateShufflingOutputChannels1To8(child);
			UpdateShufflingOutputChannels9To16(child);
		}

		private void UpdateShufflingInputChannels1To8(SourceService source)
		{
			if (source.AudioChannel1 != InputAudioChannel1) InputAudioChannel1 = source.AudioChannel1;
			if (source.AudioChannel2 != InputAudioChannel2) InputAudioChannel2 = source.AudioChannel2;
			if (source.AudioChannel3 != InputAudioChannel3) InputAudioChannel3 = source.AudioChannel3;
			if (source.AudioChannel4 != InputAudioChannel4) InputAudioChannel4 = source.AudioChannel4;
			if (source.AudioChannel5 != InputAudioChannel5) InputAudioChannel5 = source.AudioChannel5;
			if (source.AudioChannel6 != InputAudioChannel6) InputAudioChannel6 = source.AudioChannel6;
			if (source.AudioChannel7 != InputAudioChannel7) InputAudioChannel7 = source.AudioChannel7;
			if (source.AudioChannel8 != InputAudioChannel8) InputAudioChannel8 = source.AudioChannel8;
		}

		private void UpdateShufflingInputChannels9To16(SourceService source)
		{
			if (source.AudioChannel10 != InputAudioChannel10) InputAudioChannel10 = source.AudioChannel10;
			if (source.AudioChannel11 != InputAudioChannel11) InputAudioChannel11 = source.AudioChannel11;
			if (source.AudioChannel12 != InputAudioChannel12) InputAudioChannel12 = source.AudioChannel12;
			if (source.AudioChannel13 != InputAudioChannel13) InputAudioChannel13 = source.AudioChannel13;
			if (source.AudioChannel14 != InputAudioChannel14) InputAudioChannel14 = source.AudioChannel14;
			if (source.AudioChannel15 != InputAudioChannel15) InputAudioChannel15 = source.AudioChannel15;
			if (source.AudioChannel16 != InputAudioChannel16) InputAudioChannel16 = source.AudioChannel16;
		}

		private void UpdateShufflingOutputChannels1To8(EndPointService child)
		{
			if (child.AudioChannel1 != OutputAudioChannel1) OutputAudioChannel1 = child.AudioChannel1;
			if (child.AudioChannel2 != OutputAudioChannel2) OutputAudioChannel2 = child.AudioChannel2;
			if (child.AudioChannel3 != OutputAudioChannel3) OutputAudioChannel3 = child.AudioChannel3;
			if (child.AudioChannel4 != OutputAudioChannel4) OutputAudioChannel4 = child.AudioChannel4;
			if (child.AudioChannel5 != OutputAudioChannel5) OutputAudioChannel5 = child.AudioChannel5;
			if (child.AudioChannel6 != OutputAudioChannel6) OutputAudioChannel6 = child.AudioChannel6;
			if (child.AudioChannel7 != OutputAudioChannel7) OutputAudioChannel7 = child.AudioChannel7;
			if (child.AudioChannel8 != OutputAudioChannel8) OutputAudioChannel8 = child.AudioChannel8;
		}

		private void UpdateShufflingOutputChannels9To16(EndPointService child)
		{
			if (child.AudioChannel9 != OutputAudioChannel9) OutputAudioChannel9 = child.AudioChannel9;
			if (child.AudioChannel10 != OutputAudioChannel10) OutputAudioChannel10 = child.AudioChannel10;
			if (child.AudioChannel11 != OutputAudioChannel11) OutputAudioChannel11 = child.AudioChannel11;
			if (child.AudioChannel12 != OutputAudioChannel12) OutputAudioChannel12 = child.AudioChannel12;
			if (child.AudioChannel13 != OutputAudioChannel13) OutputAudioChannel13 = child.AudioChannel13;
			if (child.AudioChannel14 != OutputAudioChannel14) OutputAudioChannel14 = child.AudioChannel14;
			if (child.AudioChannel15 != OutputAudioChannel15) OutputAudioChannel15 = child.AudioChannel15;
			if (child.AudioChannel16 != OutputAudioChannel16) OutputAudioChannel16 = child.AudioChannel16;
		}

		private void UpdateDolbyDecoding(SourceService source, EndPointService child)
		{
			if (!AudioDolbyDecodingRequired && source.AudioDolbyDecodingRequired)
			{
				// disable the audio shuffling in the child
				// this needs to be manually enabled by an operator in case dolby decoding of the source audio is required
				child.AudioShufflingRequired = false;
			}

			Helpers?.Log(nameof(AudioProcessingService), nameof(UpdateDolbyDecoding), $"Update dolby decoding required value to: {source.AudioDolbyDecodingRequired}");

			AudioDolbyDecodingRequired = source.AudioDolbyDecodingRequired;

			UpdateResourceRequirements(AudioDolbyDecodingFunctionDefinitionName, source.AudioDolbyDecodingRequired);
		}

		private void UpdateMatrixFunctions(SourceService source, EndPointService child)
		{
			UpdateMatrixInputFunctions(source, child);
			UpdateMatrixOutputFunctions(source, child);

			Log(nameof(UpdateMatrixFunctions), $"Functions RequiresResource properties after update: {string.Join(",", Service.Functions.Select(f => $"{f.Definition.Label}={f.RequiresResource}"))}");
		}

		private void UpdateMatrixInputFunctions(SourceService source, EndPointService child)
		{
			var matrixInputAudioDeembeddingFunction = Service.Functions.FirstOrDefault(f => f.NodeId == MatrixSdiInputAudioDeembeddingFunctionNodeId);
			if (matrixInputAudioDeembeddingFunction != null)
			{
				matrixInputAudioDeembeddingFunction.RequiresResource = child.AudioDeembeddingRequired && (source.AudioDolbyDecodingRequired || child.AudioShufflingRequired || child.AudioEmbeddingRequired);
			}

			var matrixInputAudioShufflingFunction = Service.Functions.FirstOrDefault(f => f.NodeId == MatrixSdiInputAudioShufflingFunctionNodeId);
			if (matrixInputAudioShufflingFunction != null)
			{
				matrixInputAudioShufflingFunction.RequiresResource = child.AudioShufflingRequired && child.AudioEmbeddingRequired;
			}

			var matrixInputAudioDolbyDecodingFunction = Service.Functions.FirstOrDefault(f => f.NodeId == MatrixSdiInputAudioDolbyDecodingFunctionNodeId);
			if (matrixInputAudioDolbyDecodingFunction != null)
			{
				matrixInputAudioDolbyDecodingFunction.RequiresResource = source.AudioDolbyDecodingRequired && (child.AudioShufflingRequired || child.AudioEmbeddingRequired);
			}
		}

		private void UpdateMatrixOutputFunctions(SourceService source, EndPointService child)
		{
			var matrixOutputAudioEmbeddingFunction = Service.Functions.FirstOrDefault(f => f.NodeId == MatrixSdiOutputAudioEmbeddingFunctionNodeId);
			if (matrixOutputAudioEmbeddingFunction != null)
			{
				matrixOutputAudioEmbeddingFunction.RequiresResource = child.AudioEmbeddingRequired && (child.AudioDeembeddingRequired || source.AudioDolbyDecodingRequired || child.AudioShufflingRequired);
			}

			var matrixOutputAudioShufflingFunction = Service.Functions.FirstOrDefault(f => f.NodeId == MatrixSdiOutputAudioShufflingFunctionNodeId);
			if (matrixOutputAudioShufflingFunction != null)
			{
				matrixOutputAudioShufflingFunction.RequiresResource = child.AudioShufflingRequired && (child.AudioDeembeddingRequired || source.AudioDolbyDecodingRequired);
			}

			var matrixOutputAudioDolbyDecodingFunction = Service.Functions.FirstOrDefault(f => f.NodeId == MatrixSdiOutputAudioDolbyDecodingFunctionNodeId);
			if (matrixOutputAudioDolbyDecodingFunction != null)
			{
				matrixOutputAudioDolbyDecodingFunction.RequiresResource = source.AudioDolbyDecodingRequired && child.AudioDeembeddingRequired;
			}
		}

		private void UpdateServiceAudioChannelParameter(Guid parameterId, string value)
		{
			if (Service == null)
			{
				return;
			}

			foreach (var function in Service.Functions)
			{
				if (function.Parameters == null)
				{
					continue;
				}

				var parameter = function.Parameters.FirstOrDefault(p => p.Id == parameterId);
				if (parameter != null)
				{
					parameter.Value = value;

					return;
				}
			}
		}

		public override bool ProcessesSignalSameAs(RoutingRequiringService service)
		{
			if (service is AudioProcessingService other)
			{
				return HasEqualValuesAs(other);
			}
			else
			{
				Log(nameof(ProcessesSignalSameAs), $"Service {service.Service.Name} is not a {nameof(AudioProcessingService)}");
				return false;
			}
		}
	}
}