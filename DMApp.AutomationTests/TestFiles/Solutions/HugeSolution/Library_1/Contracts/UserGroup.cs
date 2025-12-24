namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
	using Newtonsoft.Json;

	public class UserGroup
	{
		public UserGroup()
		{
			ID = default(string);
			Name = default(string);
			Company = default(string);
			CompanySecurityViewId = default(int);
			IsSport = default(bool);
			IsNews = default(bool);
			IsMcr = default(bool);
			CanConfigureTemplate = default(bool);
			OrderTemplates = new string[0];
			EventTemplates = new string[0];
			LinkedCompanies = new Company[0];
		}

		public string ID { get; set; }

		public string Name { get; set; }

		public string Company { get; set; }

		/// <summary>
		/// Gets or sets the ID of the view associated with the company to which this User Group belongs.
		/// 0 if the User Group is not linked to a Company.
		/// </summary>
		public int CompanySecurityViewId { get; set; }

		/// <summary>
		/// Gets or sets the list of companies that are linked to the company of this contract.
		/// </summary>
		public Company[] LinkedCompanies { get; set; }

		public bool IsSport { get; set; }

        public bool IsNews { get; set; }

        public bool IsMcr { get; set; }

		public bool CanSwapResources { get; set; }

		public bool CanConfigureTemplate { get; set; }

		public string[] OrderTemplates { get; set; }

		public string[] EventTemplates { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}
	}
}