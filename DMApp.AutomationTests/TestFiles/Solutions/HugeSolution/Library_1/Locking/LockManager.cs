namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Helpers = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Helpers;

	public class LockManager
	{
		private readonly Helpers helpers;
		private readonly Element orderManagerElement;
		private readonly string username;
		private readonly Dictionary<ObjectTypes, Dictionary<string, ExternalRequestLockResponse>> locks = new Dictionary<ObjectTypes, Dictionary<string, ExternalRequestLockResponse>>();

		/// <summary>
		/// Initializes a new LockManager.
		/// </summary>
		/// <param name="helpers">Link with DataMiner.</param>
		/// <exception cref="ArgumentNullException">Thrown when the provided Engine is null.</exception>
		/// <exception cref="ElementByProtocolNotFoundException">Thrown when no active Order Manager Element is found on the DMS.</exception>
		public LockManager(Helpers helpers)
		{
			this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));

			username = helpers.Engine.UserLoginName;
			orderManagerElement = helpers.Engine.FindElementsByProtocol(OrderManagerProtocol.Name).FirstOrDefault() ?? throw new ElementByProtocolNotFoundException(OrderManagerProtocol.Name);

			locks.Add(ObjectTypes.Event, new Dictionary<string, ExternalRequestLockResponse>());
			locks.Add(ObjectTypes.Order, new Dictionary<string, ExternalRequestLockResponse>());
		}

		/// <summary>
		/// Retrieves a lock for the Event specified by its ID.
		/// </summary>
		/// <param name="jobId">ID of the Event.</param>
		/// <param name="extendLock"></param>
		public LockInfo RequestEventLock(Guid jobId, bool extendLock = false)
		{
			return RequestEventLock(jobId.ToString(), extendLock);
		}

		/// <summary>
		/// Retrieves a lock for the Event specified by its ID.
		/// </summary>
		/// <param name="jobId">ID of the Event.</param>
		/// <param name="extendLock"></param>
		public LockInfo RequestEventLock(string jobId, bool extendLock = false)
		{
			helpers.LogMethodStart(nameof(LockManager), nameof(RequestEventLock), out var stopwatch);

			InternalEventLocks.TryGetValue(jobId, out ExternalRequestLockResponse storedLock);
			if (storedLock != null && !extendLock)
			{
				helpers.LogMethodCompleted(nameof(LockManager), nameof(RequestEventLock), null, stopwatch);
				return storedLock.GetLockInfo();
			}
			
			ExternalRequestLockResponse response = RequestLock(jobId, ObjectTypes.Event, extendLock);
			if (response == null)
			{
				helpers.LogMethodCompleted(nameof(LockManager), nameof(RequestEventLock), null, stopwatch);
				return new LockInfo(false, "error", jobId, TimeSpan.Zero);
			}

            if (!response.IsLockExtended) InternalEventLocks[jobId] = response;

			var result = response.GetLockInfo();

			helpers.Log(nameof(LockManager), nameof(RequestEventLock), "Lock is" + (result.IsLockGranted ? string.Empty : " not") + " granted for event " + jobId);
			helpers.LogMethodCompleted(nameof(LockManager), nameof(RequestEventLock), null, stopwatch);

			return result;
		}

		/// <summary>
		/// Releases the lock for the Event specified by its ID.
		/// </summary>
		/// <param name="jobId">ID of the Event.</param>
		public void ReleaseEventLock(Guid jobId)
		{
			ReleaseEventLock(jobId.ToString());
		}

		/// <summary>
		/// Releases the lock for the Event specified by its ID.
		/// </summary>
		/// <param name="jobId">ID of the Event.</param>
		public void ReleaseEventLock(string jobId)
		{
			InternalEventLocks.TryGetValue(jobId, out ExternalRequestLockResponse storedLock);
			if (storedLock == null) return;

			// Check if lock should be released
			LockInfo lockInfo = storedLock.GetLockInfo();
			if (lockInfo == null || !lockInfo.IsLockGranted) return;

			if (ReleaseLock(jobId, ObjectTypes.Event) != null)
			{
				InternalEventLocks.Remove(jobId);
			}
		}

		/// <summary>
		/// Retrieves a lock for the Order specified by its ID.
		/// </summary>
		/// <param name="orderId">ID of the Order.</param>
		/// <param name="extendLock"></param>
		public LockInfo RequestOrderLock(Guid orderId, bool extendLock = false)
		{
			return RequestOrderLock(orderId.ToString(), extendLock);
		}

		/// <summary>
		/// Retrieves a lock for the Order specified by its ID.
		/// </summary>
		/// <param name="orderId">ID of the Order.</param>
		/// <param name="extendLock"></param>
		public LockInfo RequestOrderLock(string orderId, bool extendLock = false)
		{
			helpers.LogMethodStart(nameof(LockManager), nameof(RequestOrderLock), out var stopwatch);

            if (!extendLock && InternalOrderLocks.TryGetValue(orderId, out var storedLock))
            {
				helpers.LogMethodCompleted(nameof(LockManager), nameof(RequestOrderLock), null, stopwatch);
				return storedLock.GetLockInfo();
            }

			var response = RequestLock(orderId, ObjectTypes.Order, extendLock);
			if (response == null)
			{
				helpers.LogMethodCompleted(nameof(LockManager), nameof(RequestOrderLock), null, stopwatch);
				return new LockInfo(false, "error", orderId, TimeSpan.Zero);
			}

			if (!response.IsLockExtended) InternalOrderLocks[orderId] = response;

			var result = response.GetLockInfo();

			helpers.Log(nameof(LockManager), nameof(RequestOrderLock), "Lock is" + (result.IsLockGranted ? string.Empty : " not") + " granted for order " + orderId);
			helpers.LogMethodCompleted(nameof(LockManager), nameof(RequestOrderLock), null, stopwatch);

			return result;
		}

		/// <summary>
		/// Releases the lock for the Order specified by its ID.
		/// </summary>
		/// <param name="orderId">ID of the Order.</param>
		public void ReleaseOrderLock(Guid orderId)
		{
			ReleaseOrderLock(orderId.ToString());
		}

		/// <summary>
		/// Releases the lock for the Order specified by its ID.
		/// </summary>
		/// <param name="orderId">ID of the Order.</param>
		public void ReleaseOrderLock(string orderId)
		{
			InternalOrderLocks.TryGetValue(orderId, out ExternalRequestLockResponse storedLock);
			if (storedLock == null) return;

			// Check if lock should be released
			LockInfo lockInfo = storedLock.GetLockInfo();
			if (lockInfo == null || !lockInfo.IsLockGranted) return;

			if (ReleaseLock(orderId, ObjectTypes.Order) != null)
			{
				InternalOrderLocks.Remove(orderId);
				// UnlockOrderInUi(Guid.Parse(nonLiveOrderIdLabel)); WIP
			}
		}

		/// <summary>
		/// Releases all locks taken by this LockManager instance.
		/// </summary>
		public void ReleaseLocks()
		{
			// Release Event Locks
			List<string> eventsToRelease = new List<string>(InternalEventLocks.Keys);
			foreach (string eventId in eventsToRelease)
			{
				helpers.Log(nameof(LockManager), nameof(ReleaseLocks), $"Releasing lock for event {eventId}");
				ReleaseEventLock(eventId);
			}

			// Release Order Locks
			List<string> ordersToRelease = new List<string>(InternalOrderLocks.Keys);
			foreach (string orderId in ordersToRelease)
			{
				helpers.Log(nameof(LockManager), nameof(ReleaseLocks), $"Releasing lock for order {orderId}");
				ReleaseOrderLock(orderId);
			}
		}

		private ExternalRequestLockResponse RequestLock(string objectId, ObjectTypes objectType, bool extendLock = false)
		{
			Guid id = Guid.NewGuid();
			DataMinerInterface.Element.SetParameter(helpers, orderManagerElement, OrderManagerProtocol.LockRequestParameterId, new ExternalRequestLockRequest { Id = id.ToString(), ObjectId = objectId, ObjectType = objectType, Username = username, IsLockExtended = extendLock }.Serialize());

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			int retries = 0;
			bool requestAddedToTable = DataMinerInterface.Element.GetTablePrimaryKeys(helpers, orderManagerElement, OrderManagerProtocol.LockRequestsTable.TablePid).Contains(id.ToString());
			
			while (!requestAddedToTable && retries < 20)
            {
                Thread.Sleep(50);
                requestAddedToTable = DataMinerInterface.Element.GetTablePrimaryKeys(helpers, orderManagerElement, OrderManagerProtocol.LockRequestsTable.TablePid).Contains(id.ToString());
                retries++;
            }

            stopwatch.Stop();

			if (!requestAddedToTable)
			{
				helpers.Log(nameof(LockManager), nameof(RequestLock), $"Lock request for {objectType} {objectId} was not added to the Order Manager lock table after {stopwatch.ElapsedMilliseconds} milliseconds");
				return null;
			}

			stopwatch.Restart();
			retries = 0;
			RequestStates status = (RequestStates)Convert.ToInt32(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, orderManagerElement, OrderManagerProtocol.LockRequestsTable.StatusPid, id.ToString()));
            bool requestHandled = status != RequestStates.Pending;
			
			while (!requestHandled && retries < 30)
            {
                Thread.Sleep(50);
                status = (RequestStates)Convert.ToInt32(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, orderManagerElement, OrderManagerProtocol.LockRequestsTable.StatusPid, id.ToString()));
                requestHandled = status != RequestStates.Pending;
                retries++;
            }

            stopwatch.Stop();

			if (status != RequestStates.Ok)
			{
				helpers.Log(nameof(LockManager), nameof(RequestLock), $"Lock request for {objectType} {objectId} has status {status} in Order Manager lock table after {stopwatch.ElapsedMilliseconds} milliseconds");
				return null;
			}

			return ExternalRequestLockResponse.Deserialize(Convert.ToString(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, orderManagerElement, OrderManagerProtocol.LockRequestsTable.ResponsePid, id.ToString())));
		}

        private ExternalReleaseLockResponse ReleaseLock(string objectId, ObjectTypes objectType)
        {
            Guid id = Guid.NewGuid();
            DataMinerInterface.Element.SetParameter(helpers, orderManagerElement, OrderManagerProtocol.LockRequestParameterId, new ExternalReleaseLockRequest { Id = id.ToString(), ObjectId = objectId, ObjectType = objectType }.Serialize());

            int retries = 0;
            bool requestAddedToTable = DataMinerInterface.Element.GetTablePrimaryKeys(helpers, orderManagerElement, OrderManagerProtocol.LockRequestsTable.TablePid).Contains(id.ToString());

            while (!requestAddedToTable && retries < 20)
            {
                Thread.Sleep(50);
                requestAddedToTable = DataMinerInterface.Element.GetTablePrimaryKeys(helpers, orderManagerElement, OrderManagerProtocol.LockRequestsTable.TablePid).Contains(id.ToString());
                retries++;
            }

            if (!requestAddedToTable)
            {
                return null;
            }

            retries = 0;
            RequestStates status = (RequestStates)Convert.ToInt32(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, orderManagerElement, OrderManagerProtocol.LockRequestsTable.StatusPid, id.ToString()));
            bool requestHandled = status != RequestStates.Pending;

            while (!requestHandled && retries < 30)
            {
                Thread.Sleep(50);
                status = (RequestStates)Convert.ToInt32(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, orderManagerElement, OrderManagerProtocol.LockRequestsTable.StatusPid, id.ToString()));
                requestHandled = status != RequestStates.Pending;
                retries++;
            }

            if (status != RequestStates.Ok)
            {
                return null;
            }

            return ExternalReleaseLockResponse.Deserialize(Convert.ToString(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, orderManagerElement, OrderManagerProtocol.LockRequestsTable.ResponsePid, id.ToString())));
        }

        private Dictionary<string, ExternalRequestLockResponse> InternalEventLocks
		{
			get
			{
				return locks[ObjectTypes.Event];
			}
		}

		private Dictionary<string, ExternalRequestLockResponse> InternalOrderLocks
		{
			get
			{
				return locks[ObjectTypes.Order];
			}
		}

		/// <summary>
		/// Returns all of the Event Locks that were denied by this instance of the LockManager.
		/// </summary>
		/// <returns>All denied Event Locks.</returns>
		public IEnumerable<LockInfo> GetDeniedEventLocks()
		{
			return InternalEventLocks.Values.Where(x => !x.IsLockGranted).Select(x => x.GetLockInfo()).ToList();
		}

		/// <summary>
		/// Returns all of the Order Locks that were denied by this instance of the LockManager.
		/// </summary>
		/// <returns>All denied Order Locks.</returns>
		public IEnumerable<LockInfo> GetDeniedOrderLocks()
		{
			return InternalOrderLocks.Values.Where(x => !x.IsLockGranted).Select(x => x.GetLockInfo()).ToList();
		}

		/// <summary>
		/// Returns a value indicating if all of the requested Event locks are granted or not.
		/// </summary>
		public bool AreEventLocksGranted
		{
			get
			{
				return InternalEventLocks.All(x => x.Value.IsLockGranted);
			}
		}

		/// <summary>
		/// Returns a value indicating if all of the requested Order locks are granted or not.
		/// </summary>
		public bool AreOrderLocksGranted
		{
			get
			{
				return InternalOrderLocks.All(x => x.Value.IsLockGranted);
			}
		}

		/// <summary>
		/// Returns a value indicating if all of the requested locks (regardless if they are Event or Order locks) are granted or not.
		/// </summary>
		public bool AreLocksGranted
		{
			get
			{
				return AreEventLocksGranted && AreOrderLocksGranted;
			}
		}
	}
}