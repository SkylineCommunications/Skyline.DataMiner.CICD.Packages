using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Net.Messages.SLDataGateway;
using Skyline.DataMiner.Net.ResourceManager.Objects;

namespace Library.UI.Filters
{
	public class ResourcePoolFilterSection : FilterSectionOneInput<FunctionResource>, IFilter<FunctionResource>
	{
		private IEnumerable<ResourcePool> resourcePools;
		private readonly DropDown resourcePoolDropDown;

		public ResourcePoolFilterSection(string filterName, Func<object, FilterElement<FunctionResource>> emptyFilter, IEnumerable<ResourcePool> resourcePools) : base(filterName, emptyFilter)
		{
			this.resourcePools = resourcePools;

			var resourcePoolOptions = resourcePools.Select(p => p.Name).OrderBy(name => name).ToList();
			resourcePoolDropDown = new DropDown(resourcePoolOptions, resourcePoolOptions[0]) { IsDisplayFilterShown = true };

			GenerateUi();
		}

		public override bool IsValid => true;

		public override object Value
		{
			get => SelectedResourcePool.ID;
			set => resourcePoolDropDown.Selected = (string)value;
		}

		private ResourcePool SelectedResourcePool => resourcePools.SingleOrDefault(pool => pool.Name == resourcePoolDropDown.Selected);

		protected override void GenerateUi()
		{
			base.GenerateUi();

			AddWidget(resourcePoolDropDown, 0, 1);
		}

		protected override void HandleDefaultUpdate()
		{
			filterNameCheckBox.IsChecked = IsDefault;
			filterNameCheckBox.IsEnabled = !IsDefault;
			resourcePoolDropDown.IsEnabled = !IsDefault;
		}
	}
}
