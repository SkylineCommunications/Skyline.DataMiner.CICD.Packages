namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities
{
	using System;
	using System.Linq;
	using System.Threading;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;

	public enum ExternalRequestStatus
	{
		Pending = 0,
		Completed = 1,
		Failed = 2
	}

	public static class ElementAPI
	{
		public static string SendRequestAndGetResponse(Helpers helpers, Element element, int requestParameterId, int responseTableParameterId, int responseTableStatusParameterId, int responseTableResponseParameterId, int responseTableRemoveEntryParameterId, Guid requestId, string request, int sendRequestRetries = 20, int getResponseRetries = 30)
		{
			var sleepTime = 50;
			DataMinerInterface.Element.SetParameter(helpers, element, requestParameterId, request);

			// Send Request
			int retries = 0;
			bool requestAddedToTable = DataMinerInterface.Element.GetTablePrimaryKeys(helpers, element, responseTableParameterId).Contains(requestId.ToString());
			while (!requestAddedToTable && retries < sendRequestRetries)
			{
				Thread.Sleep(sleepTime);
				requestAddedToTable = DataMinerInterface.Element.GetTablePrimaryKeys(helpers, element, responseTableParameterId).Contains(requestId.ToString());
				retries++;
			}

			if (!requestAddedToTable)
			{
				helpers.Log(nameof(ElementAPI), nameof(SendRequestAndGetResponse), $"Request was not added to the Requests Table in time. (Waited {(sendRequestRetries* sleepTime)} ms)");
				return null;
			}

			// Wait for request to be processed
			retries = 0;
			var status = (ExternalRequestStatus)Convert.ToInt32(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, element, responseTableStatusParameterId, requestId.ToString()));
			bool requestHandled = status != ExternalRequestStatus.Pending;
			while (!requestHandled && retries < getResponseRetries)
			{
				Thread.Sleep(sleepTime);
				status = (ExternalRequestStatus)Convert.ToInt32(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, element, responseTableStatusParameterId, requestId.ToString()));
				requestHandled = status != ExternalRequestStatus.Pending;
				retries++;
			}

			if (status != ExternalRequestStatus.Completed)
			{
				helpers.Log(nameof(ElementAPI), nameof(SendRequestAndGetResponse), $"Request was not set to Completed in time. (Waited {(getResponseRetries* sleepTime)} ms)");
				return null;
			}

			string response = Convert.ToString(DataMinerInterface.Element.GetParameterByPrimaryKey(helpers, element, responseTableResponseParameterId, requestId.ToString()));

            // Remove external request entry from table.
            DataMinerInterface.Element.SetParameterByPrimaryKey(helpers, element, responseTableRemoveEntryParameterId, requestId.ToString(), 1);

            retries = 0;
            bool requestRemovedFromTable = !DataMinerInterface.Element.GetTablePrimaryKeys(helpers, element, responseTableParameterId).Contains(requestId.ToString());
            while (!requestRemovedFromTable && retries < sendRequestRetries)
			{
                Thread.Sleep(sleepTime);
                requestRemovedFromTable = !DataMinerInterface.Element.GetTablePrimaryKeys(helpers, element, responseTableParameterId).Contains(requestId.ToString());
                retries++;
            }

            if (!requestRemovedFromTable)
            {
                helpers.Log(nameof(ElementAPI), nameof(SendRequestAndGetResponse), "Request was not removed from the Requests Table in time.");
            }

            return response;
		}
	}
}
