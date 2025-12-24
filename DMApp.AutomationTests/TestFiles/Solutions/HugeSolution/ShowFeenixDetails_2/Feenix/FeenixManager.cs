namespace ShowFeenixDetails_2.Feenix
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using ColumnFilter = Skyline.DataMiner.Core.DataMinerSystem.Common.ColumnFilter;
	using ComparisonOperator = Skyline.DataMiner.Core.DataMinerSystem.Common.ComparisonOperator;

	public class FeenixManager
	{
		private const string PROTOCOL_NAME = "Finnish Broadcasting Company Feenix";
		private const int LiveStreamOrdersTablePid = 1000;
		private const int LiveStreamOrdersTableAreenaIdPid = 1001;
		private const int LiveStreamOrdersTableStopButtonPid = 1059;

		private readonly Helpers helpers;
		private readonly IDmsElement element;

		public FeenixManager(Helpers helpers)
		{
			this.helpers = helpers;
			OrderManagerElement orderManager = new OrderManagerElement(helpers);
			this.element = orderManager.FeenixElement;
		}

		public void SendStopNotification(string areenaId)
		{
			Element engineElement = helpers.Engine.FindElement(element.AgentId, element.Id);
			engineElement.SetParameterByPrimaryKey(LiveStreamOrdersTableStopButtonPid, areenaId, 1);
		}

		public LiveStreamOrder GetLiveStreamOrder(string orderId)
		{
			if (!OrderExists(orderId))
				return null;

			IDmsTable liveStreamOrdersTable = element.GetTable(LiveStreamOrdersTablePid);
			object[] liveStreamOrderRow = liveStreamOrdersTable.QueryData(new ColumnFilter[] { new ColumnFilter { Pid = LiveStreamOrdersTableAreenaIdPid, Value = orderId, ComparisonOperator = ComparisonOperator.Equal } }).FirstOrDefault();

			if (liveStreamOrderRow == null)
			{
				return null;
			}
			else
			{
				var cleanedRow = ReplaceEmptyValuesWithDescription(liveStreamOrderRow);
				return new LiveStreamOrder
				{
					OrderGeneral = new OrderGeneral
					{
						YleId = Convert.ToString(cleanedRow[0]),
						TitleMainFin = Convert.ToString(cleanedRow[10]),
						TitleMainSwe = Convert.ToString(cleanedRow[11]),
						TitleMainSmi = Convert.ToString(cleanedRow[FeenixProtocol.LiveStreamOrdersTable.MainSamiTitleIdx]),
						TitlePromoFin = Convert.ToString(cleanedRow[12]),
						TitlePromoSwe = Convert.ToString(cleanedRow[13]),
						DescriptionMainFin = Convert.ToString(cleanedRow[1]),
						DescriptionMainSwe = Convert.ToString(cleanedRow[2]),
						DescriptionMainSmi = Convert.ToString(cleanedRow[FeenixProtocol.LiveStreamOrdersTable.MainDescriptionSamiIdx]),
						DescriptionShortFin = Convert.ToString(cleanedRow[3]),
						DescriptionShortSwe = Convert.ToString(cleanedRow[4]),
						DescriptionShortSmi = Convert.ToString(cleanedRow[FeenixProtocol.LiveStreamOrdersTable.ShortDescriptionSamiIdx]),
						AlternativeId = Convert.ToString(cleanedRow[7]),
						ContentRatingAgeRestriction = Convert.ToString(cleanedRow[18]),
						PlasmaId = Convert.ToString(cleanedRow[72])
					},
					OrderMetadata = new OrderMetadata
					{
						MetaDataRelationMainClassId = Convert.ToString(cleanedRow[59]),
						MetaDataRelationMainClassFinnishTitle = Convert.ToString(cleanedRow[60]),
						MetaDataRelationMainClassSwedishTitle = Convert.ToString(cleanedRow[61]),
						MetaDataRelationSubClassId = Convert.ToString(cleanedRow[62]),
						MetaDataRelationSubClassFinnishTitle = Convert.ToString(cleanedRow[63]),
						MetaDataRelationSubClassSwedishTitle = Convert.ToString(cleanedRow[64]),
						MetaDataRelationContentClassId = Convert.ToString(cleanedRow[65]),
						MetaDataRelationContentClassFinnishTitle = Convert.ToString(cleanedRow[66]),
						MetaDataRelationContentClassSwedishTitle = Convert.ToString(cleanedRow[67]),
						MetaDataRelationReportingClassId = Convert.ToString(cleanedRow[68]),
						MetaDataRelationReportingClassFinnishTitle = Convert.ToString(cleanedRow[69]),
						MetaDataRelationReportingClassSwedishTitle = Convert.ToString(cleanedRow[70]),
						MetadataModificationCreated = GetDateTimeFromRowEntry(cleanedRow[55]),
						MetadataModificationModified = GetDateTimeFromRowEntry(cleanedRow[56]),
						MetadataModificationDeleted = GetDateTimeFromRowEntry(cleanedRow[57])
					},
					OrderSeriesInformation = new OrderSeriesInformation
					{
						MemberOfTitleMainFin = Convert.ToString(cleanedRow[29]),
						MemberOfTitleMainSwe = Convert.ToString(cleanedRow[30]),
						MemberOfTitleMainSmi = Convert.ToString(cleanedRow[FeenixProtocol.LiveStreamOrdersTable.MemberOfMainTitleSamiIdx]),
						MemberOfDescrFin = Convert.ToString(cleanedRow[22]),
						MemberOfDescrSwe = Convert.ToString(cleanedRow[23]),
						MemberOfDescrSmi = Convert.ToString(cleanedRow[FeenixProtocol.LiveStreamOrdersTable.MemberOfDescriptionSamiIdx]),
					},
					OrderVersion = new OrderVersion
					{
						VersionType = Convert.ToString(cleanedRow[36]),
						VersionMediaResourceFormatMode = Convert.ToString(cleanedRow[37]),
						VersionMediaResourceFormatFrameRate = Convert.ToString(cleanedRow[38]),
						VersionMediaResourceHasSourceName = Convert.ToString(cleanedRow[39]),
						VersionMediaResourceHasSourceType = Convert.ToString(cleanedRow[40]),
						VersionMediaResourceContactName = Convert.ToString(cleanedRow[73]),
						VersionMediaResourceHasSourceLabel = "Not available",
						VersionMediaResourceLanguage = Convert.ToString(cleanedRow[41]),
						VersionMediaResourceID = Convert.ToString(cleanedRow[42]),
						VersionMediaResourceType = Convert.ToString(cleanedRow[43]),
						VersionPublicationEventServiceID = Convert.ToString(cleanedRow[44]),
						VersionPublicationEventServiceKind = Convert.ToString(cleanedRow[45]),
						VersionPublicationEventPublisherID = Convert.ToString(cleanedRow[46]),
						VersionPublicationEventStartTime = GetDateTimeFromRowEntry(cleanedRow[47]),
						VersionPublicationEventEndTime = GetDateTimeFromRowEntry(cleanedRow[48]),
						VersionPublicationEventType = Convert.ToString(cleanedRow[49]),
						VersionPublicationEventDuration = Convert.ToString(cleanedRow[50]),
						VersionPublicationEventRegion = Convert.ToString(cleanedRow[51])
					}
				};
			}
		}

		private static DateTime? GetDateTimeFromRowEntry(object rowEntry)
		{
			try
			{
				return DateTime.FromOADate((double)rowEntry);
			}
			catch
			{
				return null;
			}
		}

		private bool OrderExists(string orderId)
		{
			try
			{
				IDmsTable ordersTable = element.GetTable(LiveStreamOrdersTablePid);
				string[] primaryKeys = ordersTable.GetPrimaryKeys();

				return primaryKeys.Contains(orderId);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(FeenixManager), nameof(OrderExists), $"Error while checking if order {orderId} exists: {e}");
				return false;
			}
		}


		private static object[] ReplaceEmptyValuesWithDescription(object[] row)
		{
			object[] cleanedRow = row;

			for (int i = 0; i < row.Length; i++)
			{
				if (row[i] == null)
				{
					cleanedRow[i] = Constants.NotFound;
				}
				else if (row[i].ToString() == "-1")
				{
					cleanedRow[i] = Constants.NotFound;
				}
				else
				{
					// Nothing to do
				}
			}

			return cleanedRow;
		}
	}
}