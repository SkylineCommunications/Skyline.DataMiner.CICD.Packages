namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

	public interface IEurovisionBooking
	{
		void InitializeDateTimes();

		void UpdateService(Service service);

		EurovisionBookingDetails GetDetails();

		void InitDetails(string transmissionNumber, string workOrderId, EurovisionBookingDetails details);
	}
}