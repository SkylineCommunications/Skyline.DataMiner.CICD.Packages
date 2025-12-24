namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public class BroadcastCenter
	{
		public string Id { get; set; }

		public string Code { get; set; }

		public string Name { get; set; }

		public City City { get; set; }

		public VideoDefinition[] SupportedVideoDefinitions { get; set; }

		public Dictionary<VideoDefinition, List<VideoResolution>> SupportedVideoDefinitionResolutions { get; private set; } = new Dictionary<VideoDefinition, List<VideoResolution>>();

		public Dictionary<VideoDefinition, List<VideoBitrate>> SupportedVideoDefinitionBitrates { get; private set; } = new Dictionary<VideoDefinition, List<VideoBitrate>>();

		public Dictionary<VideoDefinition, List<VideoBandwidth>> SupportedVideoDefinitionBandwidths { get; private set; } = new Dictionary<VideoDefinition, List<VideoBandwidth>>();

		public Dictionary<VideoDefinition, List<VideoAspectRatio>> SupportedVideoDefinitionAspectRatios { get; private set; } = new Dictionary<VideoDefinition, List<VideoAspectRatio>>();

		public VideoFrameRate[] SupportedVideoFrameRates { get; set; }

		public Facility[] Facilities { get; set; }

		public bool SupportsUNI { get; set; }

		public bool SupportsOSSUNI { get; set; }

		public void Update(IActionableElement eurovision, IEngine engine)
		{
			if (SupportedVideoDefinitions == null) UpdateVideoDefinitions(eurovision, engine);
			if (SupportedVideoFrameRates == null) UpdateVideoFrameRates(eurovision, engine);
		}

		public void UpdateFacilities(IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 8200, new string[] { String.Format("fullFilter=8202 == '{0}';forcefulltable=true", Id) });
			if (columns == null || columns.Count < 5) return;

			var productIds = columns[2];
			var productCodes = columns[3];
			var productNames = columns[4];

			Facilities = new Facility[productIds.Length];
			for (int i = 0; i < productIds.Length; i++)
			{
				Facilities[i] = new Facility((string)productIds[i], (string)productCodes[i], (string)productNames[i]);
			}
		}

		private void UpdateVideoDefinitions(IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 8100, new string[] { String.Format("fullFilter=8102 == '{0}';forcefulltable=true", Id) });
			if (columns == null || columns.Count < 3) return;

			List<VideoDefinition> videoDefinitions = new List<VideoDefinition>();
			foreach (string code in columns[2])
			{
				if (String.IsNullOrEmpty(code)) continue;
				videoDefinitions.Add(new VideoDefinition(code));
			}

			SupportedVideoDefinitions = videoDefinitions.ToArray();

			UpdateVideoDefinitionResolutions(eurovision, engine);
			UpdateVideoDefinitionBitrates(eurovision, engine);
			UpdateVideoDefinitionBandwidths(eurovision, engine);
			UpdateVideoDefinitionAspectRatios(eurovision, engine);
		}

		private void UpdateVideoDefinitionResolutions(IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 4410, new string[] { String.Format("fullFilter={0};forcefulltable=true", String.Join(" OR ", SupportedVideoDefinitions.Where(b => b?.Code != null).Select(b => String.Format("4412 == '{0}'", b.Code)))) });
			if (columns == null || columns.Count < 3) return;

			List<VideoDefinition> videoDefinitions = new List<VideoDefinition>();
			List<VideoResolution> videoResolutions = new List<VideoResolution>();
			for (int i = 0; i < columns[1].Length; i++)
			{
				if (String.IsNullOrEmpty(columns[1][i]) || String.IsNullOrEmpty(columns[2][i])) continue;
				videoDefinitions.Add(new VideoDefinition(columns[1][i]));
				videoResolutions.Add(new VideoResolution(columns[2][i]));
			}

			SupportedVideoDefinitionResolutions = new Dictionary<VideoDefinition, List<VideoResolution>>();
			for (int i = 0; i < videoDefinitions.Count; i++)
			{
				List<VideoResolution> resolutions;
				if (!SupportedVideoDefinitionResolutions.TryGetValue(videoDefinitions[i], out resolutions))
				{
					resolutions = new List<VideoResolution>();
					SupportedVideoDefinitionResolutions.Add(videoDefinitions[i], resolutions);
				}

				resolutions.Add(videoResolutions[i]);
			}
		}

		private void UpdateVideoDefinitionBitrates(IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 8110, new string[] { String.Format("fullFilter=8112 == '{0}';forcefulltable=true", Id) });
			if (columns == null || columns.Count < 4) return;

			List<VideoDefinition> videoDefinitions = new List<VideoDefinition>();
			List<VideoBitrate> videoBitrates = new List<VideoBitrate>();
			string videoBitrateName;
			for (int i = 0; i < columns[2].Length; i++)
			{
				if (String.IsNullOrEmpty(columns[2][i]) || String.IsNullOrEmpty(columns[3][i])) continue;
				videoBitrateName = Convert.ToString(eurovision.GetParameterByPrimaryKey(4602, columns[3][i]));

				if (String.IsNullOrEmpty(videoBitrateName)) continue;

				videoDefinitions.Add(new VideoDefinition(columns[2][i]));
				videoBitrates.Add(new VideoBitrate(columns[3][i], videoBitrateName));
			}

			SupportedVideoDefinitionBitrates = new Dictionary<VideoDefinition, List<VideoBitrate>>();
			for (int i = 0; i < videoDefinitions.Count; i++)
			{
				List<VideoBitrate> bitrates;
				if (!SupportedVideoDefinitionBitrates.TryGetValue(videoDefinitions[i], out bitrates))
				{
					bitrates = new List<VideoBitrate>();
					SupportedVideoDefinitionBitrates.Add(videoDefinitions[i], bitrates);
				}

				bitrates.Add(videoBitrates[i]);
			}
		}

		private void UpdateVideoDefinitionBandwidths(IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 4430, new string[] { "forcefulltable=true" });
			if (columns == null || columns.Count < 3) return;

			List<VideoDefinition> videoDefinitions = new List<VideoDefinition>();
			List<VideoBandwidth> videoBandwidths = new List<VideoBandwidth>();
			for(int i = 0; i < columns[1].Length; i++)
			{
				if (String.IsNullOrEmpty(columns[1][i]) || String.IsNullOrEmpty(columns[2][i])) continue;
				videoDefinitions.Add(new VideoDefinition(columns[1][i]));
				videoBandwidths.Add(new VideoBandwidth(columns[2][i]));
			}

			SupportedVideoDefinitionBandwidths = new Dictionary<VideoDefinition, List<VideoBandwidth>>();
			for (int i = 0; i < videoDefinitions.Count; i++)
			{
				List<VideoBandwidth> bandwidths;
				if (!SupportedVideoDefinitionBandwidths.TryGetValue(videoDefinitions[i], out bandwidths))
				{
					bandwidths = new List<VideoBandwidth>();
					SupportedVideoDefinitionBandwidths.Add(videoDefinitions[i], bandwidths);
				}

				bandwidths.Add(videoBandwidths[i]);
			}
		}

		private void UpdateVideoDefinitionAspectRatios(IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 4420, new string[] { "forcefulltable=true" });
			if (columns == null || columns.Count < 3) return;

			List<VideoDefinition> videoDefinitions = new List<VideoDefinition>();
			List<VideoAspectRatio> videoAspectRatios = new List<VideoAspectRatio>();
			for(int i = 0; i < columns[1].Length; i++)
			{
				if (String.IsNullOrEmpty(columns[1][i]) || String.IsNullOrEmpty(columns[2][i])) continue;
				videoDefinitions.Add(new VideoDefinition(columns[1][i]));
				videoAspectRatios.Add(new VideoAspectRatio(columns[2][i]));
			}

			SupportedVideoDefinitionAspectRatios = new Dictionary<VideoDefinition, List<VideoAspectRatio>>();
			for (int i = 0; i < videoDefinitions.Count; i++)
			{
				List<VideoAspectRatio> aspectRatios;
				if (!SupportedVideoDefinitionAspectRatios.TryGetValue(videoDefinitions[i], out aspectRatios))
				{
					aspectRatios = new List<VideoAspectRatio>();
					SupportedVideoDefinitionAspectRatios.Add(videoDefinitions[i], aspectRatios);
				}

				aspectRatios.Add(videoAspectRatios[i]);
			}
		}

		private void UpdateVideoFrameRates(IActionableElement eurovision, IEngine engine)
		{
			var columns = eurovision.GetTable(engine, 4300, new string[] { "forcefulltable=true" });
			if (columns == null || columns.Count < 2) return;

			string[] frameRateCodes = columns[0];
			string[] frameRateNames = columns[1];

			List<VideoFrameRate> frameRates = new List<VideoFrameRate>();
			for (int i = 0; i < frameRateCodes.Length; i++)
			{
				if (String.IsNullOrEmpty(frameRateCodes[i]) || String.IsNullOrEmpty(frameRateNames[i])) continue;
				frameRates.Add(new VideoFrameRate(frameRateCodes[i], frameRateNames[i]));
			}

			SupportedVideoFrameRates = frameRates.ToArray();
		}
	}
}