namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.QAPortal
{
    using Newtonsoft.Json;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using System;
    using System.Net.Http;
    using System.Text;

    public class QAPortalHelper : IDisposable
	{
		private readonly Helpers helpers;

		private readonly SystemSettings systemSettings;
		private readonly HttpClient client;

		private bool disposedValue;

		public QAPortalHelper(Helpers helpers)
		{
			this.helpers = helpers;

			systemSettings = SystemSettings.Load();

			client = new HttpClient();
			if (!String.IsNullOrEmpty(systemSettings.ApiKey)) client.DefaultRequestHeaders.Add("Token", systemSettings.ApiKey);
			if (!String.IsNullOrEmpty(systemSettings.ClientId)) client.DefaultRequestHeaders.Add("ClientId", systemSettings.ClientId);
		}

		public void PublishPerformanceTestResult(string testName, TimeSpan value, string extraInfo = null)
        {
			if (StartPerformanceTest(testName)) 
			{
				AddPerformanceMetric(testName, value, extraInfo); 
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
                    try
                    {
                        client?.Dispose();
                    }
                    catch (Exception)
                    {
						// ignore
                    }
				}

				disposedValue = true;
			}
		}

		private bool StartPerformanceTest(string testName)
		{
			var succeeded = PostMessage(
				systemSettings.PerformanceStartTestUrl,
				new StartPerformanceTestMessage
				{
					AgentName = Engine.SLNetRaw.ServerDetails.AgentName,
					Author = "VSC",
					SquadName = "The Pioneers",
					DataMinerVersion = "10.1.12-CU0",
					MaxEndTimeValue = DateTime.Now.AddMinutes(1),
					TestName = testName,
					AmountOfActions = 1
				});

			return succeeded;
		}

		private void AddPerformanceMetric(string testName, TimeSpan value, string extraInfo = null)
		{
			PostMessage(
				systemSettings.PerformanceAddTestUrl,
				new AddPerformanceMetricMessage
				{
					Agent = new Agent
					{
						Name = Engine.SLNetRaw.ServerDetails.AgentName
					},
					AmountOfActions = 1,
					DataMinerVersion = "10.1.12-CU0",
					Test = new Test
					{
						Name = testName
					},
					DateValue = DateTime.UtcNow,
					Failed = false,
					UnitValue = TestResultUnit.Millisecond,
					Timing = (int)value.TotalMilliseconds,
					ExtraInfo = extraInfo ?? string.Empty
				});
		}

#pragma warning disable S3994 // URI Parameters should not be strings
        private bool PostMessage(string url, object message)
#pragma warning restore S3994 // URI Parameters should not be strings
        {
			var json = JsonConvert.SerializeObject(message);

			using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
			{
#pragma warning disable S4005 // "System.Uri" arguments should be used instead of strings
                var response = client.PostAsync(url, content).Result;
#pragma warning restore S4005 // "System.Uri" arguments should be used instead of strings
                if (!response.IsSuccessStatusCode)
				{
					var responseContent = response.Content;
					var responseString = responseContent.ReadAsStringAsync().Result;

					helpers.Log(nameof(QAPortalHelper), nameof(PostMessage), $"Posting to QAPortal failed: {responseString}");
					return false;
				}

				return true;
			}
		}

		~QAPortalHelper()
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
}