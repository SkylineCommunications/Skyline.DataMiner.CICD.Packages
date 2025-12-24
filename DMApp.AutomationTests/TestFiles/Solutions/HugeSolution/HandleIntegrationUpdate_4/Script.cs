namespace HandleIntegrationUpdate_4
{
	using System;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Ceiton;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Feenix;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.PebbleBeach;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Plasma;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderManagerElement;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.YLE.Integrations;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	public class Script : IDisposable
	{
		private Helpers helpers;
		private bool disposedValue;

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			engine.Timeout = TimeSpan.FromHours(1);

			helpers = new Helpers(engine, Scripts.HandleIntegrationUpdate);

			OrderManagerElement orderManagerElement = null;
			IntegrationRequest request = null;
			try
			{
				orderManagerElement = new OrderManagerElement(helpers);
				request = InitializeRequest(engine);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Exception during initialization: {e.Message}");
				Dispose();
				return;
			}

			Integration integration = null;
			try
			{
				integration = InitializeIntegration(orderManagerElement, request.IntegrationType);
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Exception initializing integration: {e.Message}");
				orderManagerElement.SendResponse(new IntegrationResponse(request)
				{
					Status = UpdateStatus.Failed,
					AdditionalInformation = $"Exception initializing integration {request.IntegrationType}: {e.Message}"
				});

				Dispose();
				return;
			}

			if (integration == null) return;

			try
			{
				integration.HandleUpdate(request.IntegrationUpdateId, request.IntegrationData);
			}
			catch (LockNotGrantedException e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Lock not granted");
				orderManagerElement.SendResponse(new IntegrationResponse(request)
				{
					Status = UpdateStatus.Failed,
					AdditionalInformation = e.ToString(),
				});
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(Run), $"Exception handling update: {e}");
				orderManagerElement.SendResponse(new IntegrationResponse(request)
				{
					Status = UpdateStatus.Failed,
					AdditionalInformation = $"HandleUpdate|No action executed|Exception handling update: {e}",
				});
			}
			finally
			{
				Dispose();
			}
		}

		private static IntegrationRequest InitializeRequest(IEngine engine)
		{
			string updateValue = engine.GetScriptParam("update").Value;
			if (String.IsNullOrEmpty(updateValue)) throw new ScriptParameterException("Update script parameter is empty");

			var request = JsonConvert.DeserializeObject<IntegrationRequest>(updateValue);

			return request;
		}

		private Integration InitializeIntegration(OrderManagerElement orderManagerElement, IntegrationType type)
		{
			switch (type)
			{
				case IntegrationType.Ceiton:
					return new CeitonIntegration(helpers, orderManagerElement);
				case IntegrationType.Plasma:
					return new PlasmaIntegration(helpers, orderManagerElement);
				case IntegrationType.Feenix:
					return new FeenixIntegration(helpers, orderManagerElement);
				case IntegrationType.Eurovision:
					return new EurovisionIntegration(helpers, orderManagerElement);
				case IntegrationType.PebbleBeach:
					return new PebbleBeachIntegration(helpers, orderManagerElement);
				default:
					throw new UnsupportedIntegrationException(type);
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
						helpers.LockManager.ReleaseLocks();
					}
					catch(Exception e)
					{
						helpers.Log(nameof(Script), nameof(Dispose), $"Exception releasing locks: {e}");
					}

					helpers.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}