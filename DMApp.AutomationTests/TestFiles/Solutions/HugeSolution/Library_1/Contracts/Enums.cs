namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
	public enum ContractStatus
	{
		Closed = 0,

		Open = 1
	}

	public enum ContractType
	{
		Other = 0,

		BaseContract = 1
	}

	public enum SimultaneousServices
	{
		Unlimited = 0,

		One = 1,

		Two = 2,

		Three = 3,

		Four = 4,

		Five = 5,

		Six = 6,

		Seven = 7,

		Eight = 8,

		Nine = 9,

		Ten = 10
	}

	public enum SignalSourceType
	{
		Main,

		Backup
	}

	public enum TemplateAction
	{
		Add = 0,
		Edit = 1,
		Delete = 2,
		Get = 3
	}
}