namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
	using System;

	public class MultilateralTransmission
	{
		public readonly string Id;
		public readonly string Type;
		public readonly DateTime BeginDate;
		public readonly DateTime EndDate;
		public readonly DateTime ProgramBeginDate;
		public readonly DateTime ProgramEndDate;
		public readonly string Nature1;
		public readonly string Nature2;
		public readonly string TransmissionNumber;
		public readonly string Status;
		public readonly string ProductCode;
		public readonly bool IsAlreadyBooked;
		public readonly string DisplayValue;


		public MultilateralTransmission(string id, string type, DateTime beginDate, DateTime endDate, DateTime programBeginDate, DateTime programEndDate, string nature1, string nature2, string transmissionNumber, string status, string productCode, bool isAlreadyBooked)
		{
			Id = id;
			Type = type;
			BeginDate = beginDate;
			EndDate = endDate;
			ProgramBeginDate = programBeginDate;
			ProgramEndDate = programEndDate;
			Nature1 = nature1;
			Nature2 = nature2;
			TransmissionNumber = transmissionNumber;
			Status = status;
			ProductCode = productCode;
			IsAlreadyBooked = isAlreadyBooked;

			DisplayValue = $"{Nature1} [{BeginDate.ToShortDateString()} {BeginDate.ToShortTimeString()} - {EndDate.ToShortTimeString()} GMT] ({Id})";
			if (IsAlreadyBooked) DisplayValue += " - booked";
		}
	}
}