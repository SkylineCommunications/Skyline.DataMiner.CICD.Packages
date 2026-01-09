namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.AvidInterplayPAM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.AvidInterplayPAM;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.NonLive;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    public class AvidInterplayPamManager : INonLiveIntegrationManager
	{
		private readonly int sleepTimeForElementCommunication = 100;

		private readonly int maxAmountOfRetriesForElementCommunication = 20;

		private readonly int sleepTimeForResponse = 250;

		private readonly int maxAmountOfRetriesForResponse = 80;

		private readonly Element element;

		private readonly List<CachedResponse> cachedResponses = new List<CachedResponse>();

		public Helpers Helpers { get; private set; }

		public AvidInterplayPamManager(Helpers helpers, InterplayPamElements elementName)
		{
			Helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
			var allIplayElements = helpers.Engine.FindElementsByProtocol(AvidInterplayPamProtocol.Name);

			switch (elementName)
			{
				case InterplayPamElements.Helsinki:
					element = allIplayElements.SingleOrDefault(e => e.ElementName.Contains("Helsinki") || e.ElementName.Contains("helsinki") || e.ElementName.Contains("HKI"));
					if (element == null)
						throw new ElementNotFoundException(AvidInterplayPamProtocol.Name, InterplayPamElements.Helsinki);
					break;
				case InterplayPamElements.Tampere:
					element = allIplayElements.SingleOrDefault(e => e.ElementName.Contains("Tampere") || e.ElementName.Contains("tampere") || e.ElementName.Contains("TRE"));
					if (element == null)
						throw new ElementNotFoundException(AvidInterplayPamProtocol.Name, InterplayPamElements.Tampere);
					break;
				case InterplayPamElements.Vaasa:
					element = allIplayElements.SingleOrDefault(e => e.ElementName.Contains("Vaasa") || e.ElementName.Contains("vaasa") || e.ElementName.Contains("VSA"));
					if (element == null)
						throw new ElementNotFoundException(AvidInterplayPamProtocol.Name, InterplayPamElements.Vaasa);
					break;
				default:
					throw new ArgumentException(nameof(elementName));
			}
		}

		public List<Folder> GetRootFolders()
		{
			var response = GetFilesAndFolders(String.Empty);
			var rootFolders = new List<Folder>();
			foreach(string rootPath in response.AccessiblePaths)
			{
				string friendlyName = rootPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
				rootFolders.Add(new Folder { FriendlyName = friendlyName, URL = rootPath });
			}

			return rootFolders;
		}

		public NonLiveManagerResponse GetChildren(string parentPath)
		{
			var response = GetFilesAndFolders(parentPath);
			return new NonLiveManagerResponse
			{
				Files = response.RequestedFiles,
				Folders = response.RequestedFolders
			};
		}

		private Response GetFilesAndFolders(string folderPath)
		{
			return GetFilesAndFolders(Helpers.Engine.UserLoginName, folderPath);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userLoginName"></param>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ElementNotActiveException"/>
		/// <exception cref="ElementDidNotLogRequestException"/>
		private Response GetFilesAndFolders(string userLoginName, string folderPath)
		{
			if (String.IsNullOrWhiteSpace(userLoginName)) throw new ArgumentNullException(nameof(userLoginName));

			if (!element.IsActive) throw new ElementNotActiveException(element.Name);

			Response response;
			if (cachedResponses.Any(r => r.RequestedFolderPath == folderPath))
			{
				response = GetCachedResponse(folderPath);
			}
			else
			{
				SendRequest(userLoginName, folderPath, out string requestId);

				if (!RequestSuccessfullyReceivedByElement(requestId))
					throw new ElementDidNotLogRequestException(element.ElementName);

				if (!RequestSuccessfullyProcessedByElement(requestId))
					throw new IplayServiceDidNotRespondException(element.ElementName);

				response = GetResponse(requestId);

				SetFullUrlsAndFriendlyNamesOnFolders(response, folderPath);

				CacheResponse(folderPath, response);
			}

			return response;
		}

		private Response GetCachedResponse(string folderPath)
		{
			return cachedResponses.Single(r => r.RequestedFolderPath == folderPath).Response;
		}

		private void CacheResponse(string folderPath, Response response)
		{
			cachedResponses.Add(new CachedResponse { RequestedFolderPath = folderPath, Response = response });
		}

		private void SendRequest(string userLoginName, string folderPath, out string requestId)
		{
			requestId = Guid.NewGuid().ToString();

			Request request = new Request { Id = requestId, Username = userLoginName, FolderPath = folderPath };

			element.SetParameter(AvidInterplayPamProtocol.Parameter.ExternalFolderRequest, JsonConvert.SerializeObject(request, Formatting.None));
		}

		private bool RequestSuccessfullyReceivedByElement(string requestId)
		{
			int retries = 0;
			string[] requestKeys;
			do
			{
				requestKeys = element.GetTablePrimaryKeys(AvidInterplayPamProtocol.FolderRequestsTable.PID);
				retries++;
				Thread.Sleep(sleepTimeForElementCommunication);
			}
			while (retries < maxAmountOfRetriesForElementCommunication && !requestKeys.Contains(requestId));

			bool requestSuccessfullyReceived = requestKeys.Contains(requestId);

			if (!requestSuccessfullyReceived)
				Helpers.Log(nameof(AvidInterplayPamManager), nameof(RequestSuccessfullyReceivedByElement), "Request not successfully received");

			return requestSuccessfullyReceived;
		}

		private bool RequestSuccessfullyProcessedByElement(string requestId)
		{
			int retries = 0;
			RequestStatus requestStatus;
			do
			{
				requestStatus = (RequestStatus)Convert.ToInt32(element.GetParameterByPrimaryKey(AvidInterplayPamProtocol.FolderRequestsTable.StatusColumnPid, requestId));
				retries++;

				Thread.Sleep(sleepTimeForResponse);
			}
			while (retries < maxAmountOfRetriesForResponse && requestStatus == RequestStatus.Pending);

			bool requestSuccessfullyProcessed = requestStatus == RequestStatus.Completed;

			if (!requestSuccessfullyProcessed)
				Helpers.Log(nameof(AvidInterplayPamManager), nameof(RequestSuccessfullyProcessedByElement), "Request not successfully processed");

			return requestSuccessfullyProcessed;
		}

		private Response GetResponse(string requestId)
		{
			string responseString = null;
			try
			{
				responseString = element.GetParameterByPrimaryKey(AvidInterplayPamProtocol.FolderRequestsTable.ResponseColumnPid, requestId).ToString();

				Response response = JsonConvert.DeserializeObject<Response>(responseString);

				return response;
			}
			catch (Exception)
			{
				Helpers.Log(nameof(AvidInterplayPamManager), nameof(GetResponse), $"Exception while deserializing {responseString}");
				throw;
			}
		}

		private void SetFullUrlsAndFriendlyNamesOnFolders(Response response, string folderPath)
		{
			foreach (var folder in response.RequestedFolders)
			{
				folder.FriendlyName = folder.URL.Trim('/');
				folder.URL = folderPath + folder.URL;
			}
		}
	}
}