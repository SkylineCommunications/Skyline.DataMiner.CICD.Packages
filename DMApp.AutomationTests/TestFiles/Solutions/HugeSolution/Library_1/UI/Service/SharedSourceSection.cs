namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.UI.YleWidgets;
	using NPOI.SS.Formula.Functions;

	/// <summary>
	/// This section is used to visualize an Event Level Reception service.
	/// </summary>
	public class SharedSourceSection : YleSection
	{
		private readonly ServiceSectionConfiguration configuration;
		private readonly IEnumerable<Order> ordersThatShareTheSource;

		private readonly Label serviceNameLabel = new Label { Style = TextStyle.Bold };
		private readonly Label warningTitle = new Label("Warning: You are editing a Shared Source! Note that this can affect multiple orders.") { Style = TextStyle.Heading };
		private readonly Label ordersTitle = new Label("Orders that use this Shared Source") { Style = TextStyle.Heading };

		private readonly List<Label> orderDescriptionLabels = new List<Label>();

		private readonly DropDown serviceDefinitionTypeSelectionDropDown;
		private readonly UserInfo userInfo;

		/// <summary>
		/// Initializes a new instance of the <see cref="SharedSourceSection"/> class.
		/// </summary>
		/// <param name="sharedSource">Event Level Reception Service that is displayed by this section.</param>
		/// <param name="ordersThatShareTheSource">List of Orders that use the Event Level Reception service.</param>
		/// <param name="configuration"></param>
		/// <param name="userInfo"></param>
		/// <param name="serviceDefinitionTypeSelectionDropDown"></param>
		/// <param name="helpers"></param>
		public SharedSourceSection(Helpers helpers, Service sharedSource, IEnumerable<Order> ordersThatShareTheSource, ServiceSectionConfiguration configuration, UserInfo userInfo, DropDown serviceDefinitionTypeSelectionDropDown) : base(helpers)
		{
			this.ordersThatShareTheSource = ordersThatShareTheSource ?? throw new ArgumentNullException(nameof(ordersThatShareTheSource));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			Service = sharedSource;
			this.serviceDefinitionTypeSelectionDropDown = serviceDefinitionTypeSelectionDropDown ?? throw new ArgumentNullException(nameof(serviceDefinitionTypeSelectionDropDown));
			this.userInfo = userInfo;

			Initialize();
			GenerateUi(out int _);
			HandleVisibilityAndEnabledUpdate();
		}

		/// <summary>
		/// Gets the Shared Source Service that is displayed by this section.
		/// </summary>
		public Service Service { get; private set; }

		/// <summary>
		/// Gets the Service section.
		/// </summary>
		public ServiceSection ServiceSection { get; private set; }

		/// <summary>
		/// Initializes the widgets within this section and the linking with the underlying model objects.
		/// </summary>
		private void Initialize()
		{
			serviceNameLabel.Text = Service.Name;
			ServiceSection = new ServiceSection(helpers, Service as DisplayedService, configuration, userInfo, serviceDefinitionTypeSelectionDropDown);

			foreach (var order in ordersThatShareTheSource)
			{
				orderDescriptionLabels.Add(new Label($"- {order.Name} [{order.Start} - {order.End}]"));
			}
		}

		/// <summary>
		/// Adds the widgets to this section.
		/// </summary>
		protected override void GenerateUi(out int row)
		{
			row = -1;

			AddWidget(serviceNameLabel, ++row, 0, 1, 5);
			AddWidget(warningTitle, ++row, 0, 1, 5);
			AddWidget(new WhiteSpace(), ++row, 0, 1, 5);

			AddWidget(ordersTitle, ++row, 0, 1, 5);
			foreach (var label in orderDescriptionLabels)
			{
				AddWidget(label, ++row, 1, 1, 4);
			}

			AddWidget(new WhiteSpace(), ++row, 0, 1, 5);

			AddSection(ServiceSection, new SectionLayout(++row, 0));

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}

		public override void RegenerateUi()
		{
			Clear();
			ServiceSection.RegenerateUi();
			GenerateUi(out int _);
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			ordersTitle.IsVisible = IsVisible && orderDescriptionLabels.Any();
			foreach (var orderDescriptionLabel in orderDescriptionLabels) orderDescriptionLabel.IsVisible = IsVisible;
		}
	}
}