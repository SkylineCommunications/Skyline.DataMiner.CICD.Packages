namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
    using Skyline.DataMiner.Library.Solutions.SRM;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public static class FunctionGuids
	{
		private static readonly ConcurrentDictionary<string, Guid> functionIdCache = new ConcurrentDictionary<string, Guid>();

#pragma warning disable S2339 // Public constant members should not be used
		public static readonly Guid Dummy = Guid.Empty;
		public const string DummyString = "00000000-0000-0000-0000-000000000000";

		public static readonly Guid Satellite = new Guid(SatelliteString);
		public const string SatelliteString = "b4ac401c-455c-4e62-85fc-d0261dd9b922";

		public static readonly Guid Antenna = new Guid(AntennaString);
		public const string AntennaString = "3e578238-3588-4c74-820f-def427cc9953";

		public static readonly Guid Demodulating = new Guid(DemodulatingString);
		public const string DemodulatingString = "f6af0f47-fc8c-4be2-9eba-50593e1488bf";

		public static readonly Guid Decoding = new Guid(DecodingString);
		public const string DecodingString = "89c8f8ae-ef77-45b1-b6d3-6a2d8a87d9f1";

		public static readonly Guid Recording = new Guid(RecordingString);
		public const string RecordingString = "b141ee05-2154-4b5e-82bd-3751bc8ccd5b";

		public static readonly Guid Destination = new Guid(DestinationString);
		public const string DestinationString = "8e9af1bd-6ebb-48e3-b817-161c5a767b83";

		public static readonly Guid FiberDecoding = new Guid("f52e89f0-c646-4bf9-84ee-1fb126c247d1");

		public static readonly Guid FiberSource = new Guid("5ab13709-198d-42e2-b0ed-cb48caa96852");

		public static readonly Guid FiberDestination = new Guid("9ab71ea5-13ce-4bc8-b412-9bd2df69a66f");

		public static readonly Guid LiveUPasilaSource = new Guid("0f855e47-797a-4fae-b4ea-bf581ab57b8e");

		public const string LiveUPasilaSourceString = "0f855e47-797a-4fae-b4ea-bf581ab57b8e";

		public static readonly Guid GenericEncoding = new Guid("a6c7084d-c0b8-4b37-be7a-014f15eed3b4");

		public static readonly Guid GenericModulating = new Guid(GenericModulatingString);
		public const string GenericModulatingString = "24042a26-41e0-47d7-9ba9-59b14f11f21c";
#pragma warning restore S2339 // Public constant members should not be used

		public static readonly Guid MatrixInputAsi = GetFunctionDefinitionIdByName("Matrix Input ASI");

		public static readonly Guid MatrixOutputAsi = GetFunctionDefinitionIdByName("Matrix Output ASI");

		public static readonly Guid MatrixInputLband = GetFunctionDefinitionIdByName("Matrix Input LBand");

		public static readonly Guid MatrixOutputLband = GetFunctionDefinitionIdByName("Matrix Output LBand");

		public static readonly Guid MatrixInputSdi = GetFunctionDefinitionIdByName("Matrix Input SDI");

		public static readonly Guid MatrixOutputSdi = GetFunctionDefinitionIdByName("Matrix Output SDI");

		public static readonly Guid IpDecodingOutput = GetFunctionDefinitionIdByName("IP Decoding Output");

		public static readonly Guid IpDecoding= GetFunctionDefinitionIdByName("IP Decoding");

		public static readonly IEnumerable<Guid> AllMatrixGuids = new[] { MatrixInputAsi, MatrixOutputAsi, MatrixInputSdi, MatrixOutputSdi, MatrixInputLband, MatrixOutputLband };

		public static readonly IEnumerable<Guid> AllMatrixInputGuids = new[] { MatrixInputSdi, MatrixInputAsi, MatrixInputLband };

		public static readonly IEnumerable<Guid> AllMatrixOutputGuids = new[] { MatrixOutputAsi, MatrixOutputSdi, MatrixOutputLband };

		public static readonly IReadOnlyDictionary<Guid, Guid> MatrixFunctionPairs = new Dictionary<Guid, Guid>
		                                                                    {
			                                                                    { MatrixInputAsi, MatrixOutputAsi },
			                                                                    { MatrixInputLband, MatrixOutputLband },
			                                                                    { MatrixInputSdi, MatrixOutputSdi },
			                                                                    { MatrixOutputAsi, MatrixInputAsi },
			                                                                    { MatrixOutputLband, MatrixInputLband },
			                                                                    { MatrixOutputSdi, MatrixInputSdi }
		                                                                    };

		public static bool IsMatrixFunction(Guid functionID)
		{
			return AllMatrixGuids.Contains(functionID);
		}

		private static Guid GetFunctionDefinitionIdByName(string name)
        {
			if (!functionIdCache.ContainsKey(name))
			{
				// This try catch was added because Unit tests don't have access to a DM System.
				try
                {
					BuildFunctionIdCache();
				}
				catch (Exception)
                {
					return Guid.NewGuid();
                }
			}

			return functionIdCache[name];
        }

		private static void BuildFunctionIdCache()
        {
			var activeProtocolFunctionVersions = SrmManagers.ProtocolFunctionManager.GetAllProtocolFunctions(true).Select(p => p.ProtocolFunctionVersions.FirstOrDefault());
			foreach (var activeProtocolFunctionVersion in activeProtocolFunctionVersions)
			{
				foreach (var functionDefinition in activeProtocolFunctionVersion.FunctionDefinitions)
				{
					var functionDefinitionId = functionDefinition.FunctionDefinitionID.Id;
					functionIdCache[functionDefinition.Name] = functionDefinitionId;
				}
			}
		}
	}
}