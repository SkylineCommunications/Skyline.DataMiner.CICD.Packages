namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.Contracts
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;

	public class ContractNotFoundException : MediaServicesException
	{
		public ContractNotFoundException(string contractName) : base($"Unable to find Contract with name {contractName}")
		{

		}

		public ContractNotFoundException(string contractName, Event.Event @event) : base($"Unable to find Contract with name {contractName} defined in Event {@event.Name}")
		{

		}

		public ContractNotFoundException(ContractType contractType) : base ("Unable to find Contract with type " + contractType.ToString())
		{

		}
	}
}
