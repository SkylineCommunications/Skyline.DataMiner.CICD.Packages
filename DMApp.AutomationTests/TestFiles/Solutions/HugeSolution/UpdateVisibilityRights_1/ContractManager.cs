namespace UpdateVisibilityRights_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;

	public class ContractManager
	{
		private readonly Element element;

		public ContractManager(IEngine engine)
		{
			element = engine.FindElementsByProtocol("Finnish Broadcasting Company Contract Manager").FirstOrDefault();
		}

		public Dictionary<string, int> ViewIds
		{
			get
			{
				Dictionary<string, int> viewIds = new Dictionary<string, int>();
		
				int mcrViewId = Convert.ToInt32(element.GetParameter(1300));
				if (mcrViewId != 0) viewIds.Add("MCR", mcrViewId);

				string[] companies = element.GetTablePrimaryKeys(1000);
				foreach (string company in companies)
				{
					int companyViewId = Convert.ToInt32(element.GetParameterByPrimaryKey(1003, company));
					if (companyViewId != 0) viewIds.Add(company, companyViewId);
				}

				return viewIds;
			}
		}
	}
}