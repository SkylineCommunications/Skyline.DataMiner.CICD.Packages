namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration
{
	using System.Collections.Generic;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	public static class OrderManagerProtocol
	{
		public static readonly string Name = "Finnish Broadcasting Company Order Manager";

		public static readonly int EnableResourceElementTimeoutPid = 70;
		public static readonly int ReprocessIntegrationOrdersPid = 20;
		public static readonly int DeleteIntegrationOrdersPid = 21;
		public static readonly int ClearIntegrationOrdersFailuresPid = 22;

		public static readonly int CeitonElementIdParameterId = 100;
		public static readonly int PlasmaElementIdParameterId = 120;
		public static readonly int FeenixElementIdParameterId = 140;
		public static readonly int PebbleBeachElementIdParameterId = 160;
		public static readonly int EbuElementIdParameterId = 180;

		public static readonly int CeitonStatusParameterId = 104;
		public static readonly int PlasmaStatusParameterId = 124;
		public static readonly int FeenixStatusParameterId = 144;
		public static readonly int PebbleBeachStatusParameterId = 165;
		public static readonly int EbuStatusParameterId = 184;

		public static readonly int IntegrationResponseParameterId = 201;

		public static readonly int LockRequestParameterId = 5200;

		public static readonly int OrderUpdateParameterId = 11;

		public static readonly int EventUpdateParameterId = 13;

		public static readonly int RecurringOrderUpdateParameterId = 2499;

		public static readonly int RecurringOrdersScriptStatusParameterId = 2494;

		public static readonly int RecurringOrdersLatestScriptStartParameterId = 2495;

		public static readonly int RecurringOrdersSlidingWindowSizeParameterId = 2420;

		public static readonly int RecurringOrdersMaxBookingAmountParameterId = 2421;

		public static readonly int DefaultServiceResourceAllocationWindowStartParameterId = 4100;
		public static readonly int DefaultServiceResourceAllocationWindowEndParameterId = 4101;

		public static readonly int OrderServiceDeletionDelayParameterId = 2200;

		public static class UserGroupsTable
		{
			public static readonly int TablePid = 3000;

			public static readonly int EurovisionElementParameterId = 3004;
		}

		public static class EventsTable
		{
			public static readonly int TablePid = 2500;

			public static readonly int DeleteParameterId = 2507;
		}

		public static class RecurringOrdersTable
		{
			public static readonly int TablePid = 2400;
		}

		public static class OrdersTable
		{
			public static readonly int TablePid = 2000;

			public static readonly int NameParameterId = 2002;

			public static readonly int BookServicesStatusParameterId = 2005;

			public static readonly int DeleteParameterId = 2006;

			public static readonly int BookServicesStatusIdx = 4;

			public static readonly int ResourceOverbookedRetryCounterReadPid = 2021;

			public static readonly int ResourceOverbookedRetryCounterWritePid = 2022;
		}

		public static class IntegrationOrdersTable
		{
			public enum Status
			{
				Pending = 2
			}

			public static readonly int TablePid = 1200;

			public static readonly int IntegrationIdIdx = 1;

			public static readonly int IntegrationTypeIdx = 2;

			public static readonly int StatusPid = 1203;

			public static readonly int OrderIdPid = 1208;

			public static readonly int OrderIdIdx = 8;

			public static readonly int StatusWritePid = 1303;

			public static readonly int LastProcessedAtIdx = 10;

			public static readonly int LastProcessedAtPid = 1210;
		}

		public static class LockRequestsTable
		{
			public static readonly int TablePid = 5000;

			public static readonly int StatusPid = 5006;

			public static readonly int ResponsePid = 5008;
		}

		public static class ExternalServiceConfigurationRequests
		{
			public static readonly int ExternalServiceConfigurationRequestPid = 5700;

			public static readonly int ExternalServiceConfigurationRequestsTablePid = 5500;
			public static readonly int ExternalServiceConfigurationRequestsTableRequestStatusColumnPid = 5505;
			public static readonly int ExternalServiceConfigurationRequestsTableRequestStatusServiceConfigColumnPid = 5504;
		}

		public static class DeviceAutomationConfigurationTable
		{
			public static readonly int TablePid = 2800;

			public static readonly IReadOnlyDictionary<int, IntegrationType> DiscreetToIntegrationType = new Dictionary<int, IntegrationType>
			{
				{ 0, IntegrationType.None },
				{ 1, IntegrationType.Plasma },
				{ 2, IntegrationType.Feenix },
			};
		}

		public static class PlasmaNewsInclusionConfigurationTable
		{
			public static readonly int TablePid = 4300;
		}

		public static class PlasmaNewsRecordingConfigurationTable
		{
			public static readonly int TablePid = 4500;
		}

		public static class ServiceResourceAllocationWindowTable
		{
			public static readonly int TablePid = 4000;

			public static readonly int ServicePid = 4002;
			public static readonly int StartPid = 4003;
			public static readonly int EndPid = 4004;
		}

		public static class AutoHideMessiOrdersConfigurationTable
		{
			public static readonly int TablePid = 4400;
			public static readonly int OrderDescriptionIdx = 1;
			public static readonly int StateIdx = 2;
			public static readonly int IntegrationIdx = 3;
			public static readonly int RegexCheckIdx = 4;


			public static readonly IReadOnlyDictionary<int, IntegrationType> DiscreetToIntegrationType = new Dictionary<int, IntegrationType>
			{
				{ 0, IntegrationType.None },
				{ 1, IntegrationType.Ceiton },
				{ 2, IntegrationType.Plasma },
				{ 3, IntegrationType.Feenix },
				{ 4, IntegrationType.Eurovision },
				{ 5, IntegrationType.PebbleBeach },
			};
		}

		public class EvsMessiNewsTargetsTable
		{
			public static readonly int TablePid = 4600;

			public class Pid
			{
				public static readonly int Instance = 4601;

				public static readonly int RecordingFileDestinationPath = 4602;

				public static readonly int Target = 4603;

				public static readonly int DefaultState = 4604;
			}

			public class Idx
			{
				public static readonly int Instance = 0;

				public static readonly int RecordingFileDestinationPath = 1;

				public static readonly int Target = 2;

				public static readonly int DefaultState = 3;
			}
		}
	}
}