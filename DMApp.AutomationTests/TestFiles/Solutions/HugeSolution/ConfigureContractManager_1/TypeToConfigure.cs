using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigureContractManager_1
{
	public enum TypeToConfigure
	{
		[Description("Users")]
		User,
		[Description("User Groups")]
		UserGroup,
	}
}
