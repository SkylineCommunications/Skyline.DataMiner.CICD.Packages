namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Plasma
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations.Integrations.Plasma.Enums;
	using Skyline.DataMiner.Utils.YLE.Integrations.Plasma;
	using System;
	using System.Linq;

	public static class ProgramExtensions
	{
		public static bool ShouldAddPgmNewsRecording(this Program program, Helpers helpers)
		{
			if (!string.IsNullOrWhiteSpace(program.Title) && helpers.OrderManagerElement.TryGetPlasmaNewsRecordingConfiguration(program.Title, out bool shouldAddPgmNewsRecording))
			{
				return shouldAddPgmNewsRecording;
			}
			else
			{
				return false;
			}
		}

		public static bool IsLiveNews(this Program program, Helpers helpers)
		{
			if (!string.IsNullOrWhiteSpace(program.Title) && helpers.OrderManagerElement.TryGetPlasmaNewsInclusion(program.Title, out bool isNews))
			{
				// configuration in order manager overwrites all other rules.
				helpers.Log(nameof(ProgramExtensions), nameof(IsLiveNews), $"Program with title {program.Title} is {(isNews ? String.Empty : "not ")}marked as News program because of Plasma Inclusion rule coming from Order Manager element");
				return isNews;
			}

			if (program.Category == Category.Uutiset && program.SubCategory == SubCategory.Uutisbulletiini)
			{
				helpers.Log(nameof(ProgramExtensions), nameof(IsLiveNews), $"Program with title {program.Title}, category {program.Category.GetDescription()} and subcategory {program.SubCategory.GetDescription()} is marked as Live News.");
				return true;
			}
			else if (program.Category == Category.Uutiset && program.SubCategory == SubCategory.Uutislähetys) // "UUTISBULLETIINI" OR "UUTISLÄHETYS"
			{
				helpers.Log(nameof(ProgramExtensions), nameof(IsLiveNews), $"Program with title {program.Title}, category {program.Category.GetDescription()} and subcategory {program.SubCategory.GetDescription()} is marked as Live News.");
				return true;
			}
			else if (program.Category == Category.Ajankohtainen && program.SubCategory == SubCategory.Keskustelu) // “KESKUSTELU” OR “HAASTATTELU”
			{
				helpers.Log(nameof(ProgramExtensions), nameof(IsLiveNews), $"Program with title {program.Title}, category {program.Category.GetDescription()} and subcategory {program.SubCategory.GetDescription()} is marked as Live News.");
				return true;
			}
			else if (program.Category == Category.Urheilu && program.SubCategory == SubCategory.UrheiluUutislahetys)
			{
				helpers.Log(nameof(ProgramExtensions), nameof(IsLiveNews), $"Program with title {program.Title}, category {program.Category.GetDescription()} and subcategory {program.SubCategory.GetDescription()} is marked as Live News.");
				return true;
			}
			else
			{
				helpers.Log(nameof(ProgramExtensions), nameof(IsLiveNews), $"Program with title {program.Title}, category {program.Category.GetDescription()} and subcategory {program.SubCategory.GetDescription()} is NOT marked as Live News.");
				return false;
			}
		}

		public static RecordingConfiguration GetStandardTvRecordingConfiguration(this Program program)
		{
			return new RecordingConfiguration
			{
				IsConfigured = true,
				PlasmaTvChannelName = program.LiveTransmission.Channel.GetDescription(),
				PlasmaProgramName = program.Title,
				FastAreenaCopy = program.AreenaCopyRequired,
				SubtitleProxy = program.SubtitleCopyRequired,
				ProxyFormat = program.GetProxyFormat(),
				FastRerunCopy = program.FastRerunCopyRequired
			};
		}

		public static RecordingConfiguration GetStandardTvBackupRecordingConfiguration(this Program program)
		{
			return new RecordingConfiguration
			{
				IsConfigured = true,
				PlasmaTvChannelName = program.LiveTransmission.Channel.GetDescription(),
				PlasmaProgramName = program.Title
			};
		}

		public static RecordingConfiguration GetNewsRecordingConfiguration(this Program program, Helpers helpers)
		{
			return new RecordingConfiguration
			{
				IsConfigured = true,
				PlasmaTvChannelName = program.LiveTransmission.Channel.GetDescription(),
				PlasmaProgramName = program.Title,
				IsPlasmaLiveNews = program.IsLiveNews(helpers),
				RecordingFileDestination = FileDestination.UaIplay // Should always be UA IPlay for Plasma News Recordings
			};
		}

		public static ProxyFormat GetProxyFormat(this Program program)
		{
			if (String.IsNullOrWhiteSpace(program.SubtitleCopyFormat)) return ProxyFormat.Both;

			switch (program.SubtitleCopyFormat.Trim().ToUpper())
			{
				case "MPEG1":
				case "MPEG-1":
					return ProxyFormat.Mpeg1;
				case "MPEG4":
				case "MPEG-4":
					return ProxyFormat.Mpeg4;
				default:
					return ProxyFormat.Both;
			}
		}

		//public static string GetCleanProjectNumber(this Program program)
		//      {
		//	return program.ProjectNumber.Split('-').Last();
		//}

		//public static string GetCleanProductNumber(this Program program)
		//      {
		//	return program.ProductNumber.Split('-').Last();
		//}
	}
}
