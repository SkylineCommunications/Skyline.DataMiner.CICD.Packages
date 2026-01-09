using Newtonsoft.Json;

namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UserTasks;
    using System.Collections.Generic;
	using System.Linq;

	public class UserInfo
	{
        public UserInfo()
        {

        }

		public UserInfo(Contract contract, UserGroup[] userGroups, Contract[] allUserContracts, UserGroup[] allUserGroups, int mcrViewId, Contract[] allContracts, User user = null)
		{
			Contract = contract;
			AllUserContracts = allUserContracts ?? new Contract[0];
			UserGroups = userGroups ?? new UserGroup[0];
			AllUserGroups = allUserGroups ?? new UserGroup[0];
			McrSecurityViewId = mcrViewId;
			AllContracts = allContracts ?? new Contract[0];
            User = user;
		}

		/// <summary>
		/// Contains the specific contract selected in the event or the base contract in case not event has been created.
		/// </summary>
		public Contract Contract { get; private set; }

		/// <summary>
		/// Contains all contracts for this user.
		/// </summary>
		public Contract[] AllUserContracts { get; set; } = new Contract[0];

		/// <summary>
		/// Contains the user groups this user is part of.
		/// </summary>
		public UserGroup[] UserGroups { get; private set; } = new UserGroup[0];

        /// <summary>
        /// Contains all user groups on the DMS.
        /// </summary>
        public UserGroup[] AllUserGroups { get; private set; } = new UserGroup[0];

		/// <summary>
		/// Contains all contracts on the DMS.
		/// </summary>
		public Contract[] AllContracts { get; set; } = new Contract[0];

        /// <summary>
        /// Contains the user that is currently retrieving the userinfo.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the ID of the View that is associated with MCR users.
        /// </summary>
        public int McrSecurityViewId { get; set; }

		public bool IsMcrUser
		{
			get
			{
				return UserGroups != null && UserGroups.Any(x => x.IsMcr);
			}
		}

		public bool CanSwapResources
        {
			get
            {
				return IsMcrUser || (UserGroups != null && UserGroups.Any(x => x.CanSwapResources));
            }
        }

		public bool IsSportUser
		{
			get
			{
				return UserGroups != null && UserGroups.Any(x => x.IsSport);
			}
		}

        public bool IsNewsUser
        {
            get
            {
                return UserGroups != null && UserGroups.Any(x => x.IsNews);
            }
        }

        public bool IsInternalUser
		{
			get
			{
				return UserGroups != null && UserGroups.Any(x => x.Company.Equals("YLE"));
			}
		}

		public bool CanConfigureTemplate
		{
			get
			{
				return UserGroups != null && UserGroups.Any(x => x.CanConfigureTemplate);
			}
		}

		public string[] GetOrderTemplates()
		{
			if (UserGroups == null)
			{
				return new string[0];
			}
			else
			{
				HashSet<string> distinctTemplateNames = new HashSet<string>();
				foreach (var usergroup in UserGroups)
				{
					foreach (string templateName in usergroup.OrderTemplates)
					{
						distinctTemplateNames.Add(templateName);
					}
				}

				return distinctTemplateNames.ToArray();
			}
		}

		public string[] GetEventTemplates()
		{
			if (UserGroups == null)
			{
				return new string[0];
			}
			else
			{
				HashSet<string> distinctTemplateNames = new HashSet<string>();
				foreach (var usergroup in UserGroups)
				{
					foreach (string templateName in usergroup.EventTemplates)
					{
						distinctTemplateNames.Add(templateName);
					}
				}

				return distinctTemplateNames.ToArray();
			}
		}

		/// <summary>
		/// Contains the companies this user is part of.
		/// </summary>
		[JsonIgnore]
		public IEnumerable<string> AllUserCompanies
		{
			get
			{
				if (UserGroups == null) return new List<string>();

				List<string> allUserCompanies = new List<string>();
				foreach (var userGroup in UserGroups)
				{
					if (userGroup != null && !string.IsNullOrWhiteSpace(userGroup.Company) && !userGroup.Company.Equals("-1")) allUserCompanies.Add(userGroup.Company);
				}

				return allUserCompanies;
			}
		}

		[JsonIgnore]
		public IEnumerable<string> AllCompanies
		{
			get
			{
				return AllUserGroups.Select(x => x.Company);
			}
		}

		[JsonIgnore]
		public bool CanPromoteToSharedSource
        {
			get
            {
				return AllUserContracts.Any(x => x != null && x.ContractGlobalEventLevelReceptionConfiguration.HasFlag(GlobalEventLevelReceptionConfigurations.PromoteGlobalEventLevelReceptionAllowed));
			}
        }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}
	}
}