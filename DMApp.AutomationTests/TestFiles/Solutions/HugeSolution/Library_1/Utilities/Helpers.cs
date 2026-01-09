namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using IngestExport;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.EVS;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Metrics;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Notes;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Profile;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.QAPortal;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reservations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Resources;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.ResourceAssignment;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks.NonLiveUserTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Contexts;
	using Skyline.DataMiner.Net.Time;
	using Skyline.DataMiner.Utils.YLE.Integrations.Helpers;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public class Helpers : IDisposable, ILogger
	{
		private readonly Dictionary<Service, ResourceAssignmentHandler> resourceAssignmentHandlers = new Dictionary<Service, ResourceAssignmentHandler>();

		private readonly OrderLogger orderLogger;

		private OrderManagerElement orderManagerElement;
		private FixedFileLogger metricLogger;
		private EventManager eventManager;
		private IServiceDefinitionManager serviceDefinitionManager;
		private ServiceManager serviceManager;
		private OrderManager orderManager;
		private ResourceManager resourceManager;
		private LockManager lockManager;
		private ContractManager contractManager;
		private UserTaskManager userTaskManager;
		private NonLiveUserTaskManager nonLiveUserTaskManager;
		private IProfileManager profileManager;
        private INoteManager noteManager;
        private NonLiveOrderManager nonLiveOrderManager;
		private QAPortalHelper qaPortalHelper;
		private LogCollectorHelper logCollectorHelper;
		private EvsManager evsManager;
		private ReservationManager reservationManager;

		private Stopwatch scriptStopwatch;

        private bool disposedValue; // to detect redundant calls
		private int multiThreadingLevel;

        public Helpers(IEngine engine, Scripts script, params Guid[] orderIds)
		{
			Engine = engine ?? throw new ArgumentNullException(nameof(engine));
			Engine.SetFlag(RunTimeFlags.NoKeyCaching);
			Engine.SetFlag(RunTimeFlags.NoInformationEvents);
			Engine.SetFlag(RunTimeFlags.NoCheckingSets);

			Context = Context.Factory(engine, script);

			try
			{
				MetricCreator = new MetricCreator(this) { ScriptName = script.GetDescription() };
				MetricCreator.PerformanceDropDetected += (sender, args) => RunLogCollectorConditional();
			}
			catch (Exception)
			{
				// Try-catch to enable mocking of Helpers in unit tests
			}

			orderLogger = new OrderLogger(Engine) { DefaultFilePath = script.GetDescription() };

			AddOrderReferencesForLogging(orderIds);

			LogScriptStart();
		}

		private void LogScriptStart()
		{
			Log($"START SCRIPT {Context.Script.GetDescription()}", $"Script inputs: '{string.Join(";", Context.ScriptParameters.Select(i => $"{i.Name}='{i.Value}'"))}'", $"{Engine.UserDisplayName} ({Engine.UserLoginName})");

			LogMethodStart(Context.Script.GetDescription(), "Run", out scriptStopwatch);
		}

		public IEngine Engine { get; }
		
		public Context Context { get; }

		public event EventHandler<ProgressReportedEventArgs> ProgressReported;

		public MetricCreator MetricCreator { get; } 

		public OrderManagerElement OrderManagerElement
		{
			get => orderManagerElement ?? (orderManagerElement = new OrderManagerElement(this));
			set => orderManagerElement = value;
		}

		public ReservationManager ReservationManager
		{
			get => reservationManager ?? (reservationManager = new ReservationManager(this));
			set => reservationManager = value;
		}

		public EvsManager EvsManager
		{
			get => evsManager ?? (evsManager = new EvsManager(this));
			set => evsManager = value;
		}

		public OrderManager OrderManager
		{
			get => orderManager ?? (orderManager = new OrderManager(this));
			set => orderManager = value;
		}

		public ServiceManager ServiceManager
		{
			get => serviceManager ?? (serviceManager = new ServiceManager(this));
			set => serviceManager = value;
		}

		public IServiceDefinitionManager ServiceDefinitionManager
		{
			get => serviceDefinitionManager ?? (serviceDefinitionManager = new ServiceDefinitionManager(this));
			set => serviceDefinitionManager = value;
		}

		public EventManager EventManager
		{
			get => eventManager ?? (eventManager = new EventManager(this));
			set => eventManager = value;
		}

		public ResourceManager ResourceManager
		{
			get => resourceManager ?? (resourceManager = new ResourceManager(this));
			set => resourceManager = value;
		}

		public IProfileManager ProfileManager
		{
			get => profileManager ?? (profileManager = new ProfileManager(this));
			set => profileManager = value;
		}

		public NonLiveOrderManager NonLiveOrderManager
		{
			get => nonLiveOrderManager ?? (nonLiveOrderManager = new NonLiveOrderManager(this));
			set => nonLiveOrderManager = value;
		}

		public ContractManager ContractManager
		{
			get => contractManager ?? (contractManager = new ContractManager(this));
			set => contractManager = value;
		}

		public LockManager LockManager
		{
			get => lockManager ?? (lockManager = new LockManager(this));
			set => lockManager = value;
		}

		public UserTaskManager UserTaskManager
		{
			get => userTaskManager ?? (userTaskManager = new UserTaskManager(this));
			set => userTaskManager = value;
		}

		public NonLiveUserTaskManager NonLiveUserTaskManager
		{
			get => nonLiveUserTaskManager ?? (nonLiveUserTaskManager = new NonLiveUserTaskManager(this));
			set => nonLiveUserTaskManager = value;
		}

		public INoteManager NoteManager
        {
            get => noteManager ?? (noteManager = new NoteManager(this));
            set => noteManager = value;
        }

		public QAPortalHelper QAPortalHelper
		{
			get => qaPortalHelper ?? (qaPortalHelper = new QAPortalHelper(this));
			set => qaPortalHelper = value;
		}

		public LogCollectorHelper LogCollectorHelper
		{
			get => logCollectorHelper ?? (logCollectorHelper = new LogCollectorHelper(this));
			set => logCollectorHelper = value;
		}

		public void ReportProgress(string message)
		{
			ProgressReported?.Invoke(this, new ProgressReportedEventArgs(message));
		}

		public void AddOrderReferencesForLogging(params Guid[] orderIds)
		{
			foreach (var orderId in orderIds)
			{
				orderLogger.FileNames.Add($@"{Configuration.Constants.OrderLoggingDirectoryName}\{orderId}");

				if (metricLogger == null)
				{
					metricLogger = new FixedFileLogger(Engine, $@"C:\Skyline_Data\{Configuration.Constants.MetricLoggingDirectoryName}\{orderId}.txt");
				}
				else
				{
					metricLogger.LogfilePaths.Add($@"C:\Skyline_Data\{Configuration.Constants.MetricLoggingDirectoryName}\{orderId}.txt");
				}
			}
		}

		public ResourceAssignmentHandler GetResourceAssignmentHandler(Service service, Order orderContainingService, Dictionary<string, TimeRangeUtc> overwrittenFunctionTimeRanges = null)
		{
			if (service == null) throw new ArgumentNullException(nameof(service));
			if (orderContainingService == null) throw new ArgumentNullException(nameof(orderContainingService));

			if (overwrittenFunctionTimeRanges != null)
			{
				// don't save RAHs when the service time range is overwritten, these represent special use cases and should not be reused

				return ResourceAssignmentHandler.Factory(this, service, orderContainingService, overwrittenFunctionTimeRanges);
			}

			if (!resourceAssignmentHandlers.ContainsKey(service))
			{
				Log(nameof(Helpers), nameof(GetResourceAssignmentHandler), $"No existing resource assignment handler found for service: {service.Name}");

				var resourceAssignmentHandler = ResourceAssignmentHandler.Factory(this, service, orderContainingService);

				resourceAssignmentHandlers.Add(service, resourceAssignmentHandler);
			}
			
			return resourceAssignmentHandlers[service];
		}

		public void ClearCache()
		{
			ResourceManager.ClearCache();
			resourceAssignmentHandlers.Clear();
		}

		public void Log(string nameOfClass, string nameOfMethod, string message)
		{
			orderLogger.Log(nameOfClass, nameOfMethod, message);
		}

		public void Log(string nameOfClass, string nameOfMethod, string message, string nameOfObject)
		{
			orderLogger.Log(nameOfClass, nameOfMethod, message, nameOfObject);
		}

		public void LogMethodStart(string nameOfClass, string nameOfMethod, out Stopwatch stopwatch, string nameOfObject = null, bool usesMultiThreading = false)
		{
			Log(nameOfClass, nameOfMethod, "Start", nameOfObject);

			if (multiThreadingLevel == 0)
			{
				MetricCreator.StartMethodCallMetric(nameOfClass, nameOfMethod, nameOfObject);
			}

			if (usesMultiThreading)
			{
				multiThreadingLevel++;
			}

			stopwatch = Stopwatch.StartNew();
		}

		public void LogMethodCompleted(string nameOfClass, string nameOfMethod, string nameOfObject = null, Stopwatch stopwatch = null, bool usesMultiThreading = false)
		{
			stopwatch?.Stop();

			if (usesMultiThreading)
			{
				multiThreadingLevel--;
				if(multiThreadingLevel < 0) multiThreadingLevel = 0;
			}

			if (multiThreadingLevel == 0)
			{
				MetricCreator.TryCompleteMethodCallMetric(nameOfClass, nameOfMethod, nameOfObject, stopwatch?.Elapsed);
			}		

			Log(nameOfClass, nameOfMethod, $"Completed [{stopwatch?.Elapsed}]", nameOfObject);
		}

		protected void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					LogMethodCompleted(Context.Script.GetDescription(), "Run", null, scriptStopwatch);

					Log($"STOP SCRIPT {Context.Script}", string.Empty, string.Empty);

					orderLogger.Dispose();
					qaPortalHelper?.Dispose();

					if (metricLogger != null && MetricCreator.TryGetSerializedMetrics(out var serializedMetrics))
					{
						try
						{
							metricLogger.Log(serializedMetrics);
							metricLogger.Dispose();
						}
						catch (Exception e)
						{
							Engine.Log(nameof(Helpers), nameof(Dispose), $"An exception occurred: {e}");
						}
					}
				}

				disposedValue = true;
			}
		}

		private void RunLogCollectorConditional()
		{
			try
			{
				// log collection disabled as too many packages were being created (verified on staging)

				/*
				var scriptStartTime = MetricCreator.ScriptExecutionMetric.StartTime.Value;
				
				if (!triggeredLogCollection && LogCollectorHelper.LogCollectionRequired(scriptStartTime))
				{
					LogCollectorHelper.RunLogCollectorAsync();
					triggeredLogCollection = true;
				}

				*/
			}
			catch (Exception ex)
			{
				Engine.Log(nameof(Helpers), nameof(Dispose), $"Exception occurred: {ex}");
			}
		}

		~Helpers()
		 {
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(false);
		 }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}

	public class ProgressReportedEventArgs : EventArgs
	{
		public ProgressReportedEventArgs(string progress)
		{
			Progress = progress;
		}

		public string Progress { get; private set; }
	}
}
