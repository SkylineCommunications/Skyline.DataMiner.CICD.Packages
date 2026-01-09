namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
	using System;

	using Newtonsoft.Json;

	public class ExternalCompanyResponse : ExternalResponse
	{
		/// <summary>
		/// Company for which the contract information was requested.
		/// </summary>
		public string Company { get; set; }

		/// <summary>
		/// Gets or sets the ID of the View associated with this company.
		/// </summary>
		public int SecurityViewId { get; set; }

		/// <summary>
		/// Contains the companies that should be able to get visibility rights to the items of this company.
		/// </summary>
		public Company[] LinkedCompanies { get; set; }

        public static ExternalCompanyResponse Deserialize(string serializedRequest)
		{
			try
			{
				return JsonConvert.DeserializeObject<ExternalCompanyResponse>(serializedRequest);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public override string ToString()
		{
			return "ExternalCompanyResponse: " + JsonConvert.SerializeObject(this, Formatting.None);
		}
	}
}