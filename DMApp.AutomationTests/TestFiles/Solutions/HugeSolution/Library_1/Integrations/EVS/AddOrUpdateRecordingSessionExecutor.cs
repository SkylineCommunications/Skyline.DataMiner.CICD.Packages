namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.EVS
{
	using System;
	using Skyline.DataMiner.ConnectorAPI.EVS.IPD_VIA.Messages;
	using Skyline.DataMiner.Core.InterAppCalls.Common.CallSingle;
	using Skyline.DataMiner.Core.InterAppCalls.Common.MessageExecution;

	public class AddOrUpdateRecordingSessionExecutor : MessageExecutor<AddOrUpdateRecordingSessionResult>
	{
		public AddOrUpdateRecordingSessionExecutor(AddOrUpdateRecordingSessionResult message) : base(message)
		{
		
		}

		public override void DataGets(object dataSource)
		{
			// not required
		}

		public override void Parse()
		{
			// not required
		}

		public override bool Validate()
		{
			return Message.Success;
		}

		public override void Modify()
		{
			// not required
		}

		public override void DataSets(object dataDestination)
		{
			var service = (Service.Service)dataDestination;
			if (service == null) throw new ArgumentException("Not a service", "dataDestination");

			service.EvsId = Message.RecordingSession.Id;
		}

		public override Message CreateReturnMessage()
		{
			return null;
		}
	}
}