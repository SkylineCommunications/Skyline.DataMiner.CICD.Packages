namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition
{
	using System;
	using Library_1.Utilities;

	public class ContributingConfig : ICloneable
	{
		public ContributingConfig()
		{

		}

		private ContributingConfig(ContributingConfig other)
		{
			CloneHelper.CloneProperties(other, this);
		}

		public string ParentSystemFunction { get; set; }

		public string ResourcePool { get; set; }

		/// <summary>
		/// The appendix name for the contributed resources created for each service
		/// Only required when action is "NEW" or "EDIT" and only for a service (not for the order itself) (can be found in Contributing Config property of SD)
		/// </summary>
		public string ReservationAppendixName { get; set; }

		public string LifeCycle { get; set; }

		public bool ConvertToContributing { get; set; }

		public object Clone()
		{
			return new ContributingConfig(this);
		}
	}
}