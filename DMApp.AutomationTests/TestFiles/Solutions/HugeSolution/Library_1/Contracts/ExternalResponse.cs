namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts
{
    using System;
    using System.Collections.Generic;
	using System.Linq;

	public abstract class ExternalResponse
	{
        protected ExternalResponse()
        {
            ID = String.Empty;
            UserGroups = new UserGroup[0];
            Contracts = new Contract[0];
            AllUsers = new User[0];
            AllUserGroups = new UserGroup[0];
        }

        /// <summary>
        /// Unique Id of the response. This matches the Id of the request.
        /// </summary>
        public string ID { get; set; }

		/// <summary>
		/// All user groups for the requested Company or User.
		/// </summary>
		public UserGroup[] UserGroups { get; set; }

        /// <summary>
        /// All contracts for the requested Company or User.
        /// </summary>
        public Contract[] Contracts { get; set; }

		/// <summary>
		/// A list containing all Users from the Contract Manager. Used to send emails when certain triggers trigger.
		/// </summary>
		public User[] AllUsers { get; set; }

		/// <summary>
		/// A list containing all User Groups from the Contract Manager. Used as dropdown options in Customer UI for MCR user.
		/// </summary>
		public UserGroup[] AllUserGroups { get; set; }

		public Contract[] AllContracts { get; set; }

		/// <summary>
		/// Gets or sets the ID of the View that is associated with MCR users.
		/// </summary>
		public int McrSecurityViewId { get; set; }

		public IEnumerable<User> AllMcrUsers
		{
			get
			{
				return AllUsers.Where(user => AllUserGroups.Any(userGroup => user.UsergroupIds.Contains(userGroup.ID) && userGroup.IsMcr));
			}
		}
	}
}