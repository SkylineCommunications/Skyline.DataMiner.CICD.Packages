namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
    using System.Collections.Generic;

    public interface IContractManager
	{
		UserInfo GetUserInfo(string userLoginName, Event @event = null);

		UserInfo GetBaseUserInfo(string userLoginName);

		ExternalCompanyResponse RequestCompanyContractDetails(Event @event);

		ExternalCompanyResponse RequestCompanyContractDetails(string company);

		ExternalUserResponse RequestUserContractDetails(string userName);

		string[] GetAllCompanies();

        Dictionary<string, int> GetAllCompanySecurityViewIds();
	}
}
