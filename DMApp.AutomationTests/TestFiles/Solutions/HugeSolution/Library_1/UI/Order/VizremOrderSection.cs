namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ServiceDefinition;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class VizremOrderSection : OrderSection
	{
		private readonly Label studioTitle = new Label("Studio") { Style = TextStyle.Bold };
		private readonly Label graphicsEngineTitle = new Label("Graphics Engine") { Style = TextStyle.Bold };
		private readonly YleCheckBox useGraphicsEngineAsInputCheckBox = new YleCheckBox("Use graphics engine as a source only, i.e. no input signal needed for an engine");

		public VizremOrderSection(Helpers helpers, Order order, OrderSectionConfiguration configuration, UserInfo userInfo) : base(helpers, order, configuration, userInfo)
		{
			Initialize();
			GenerateUi();
			HandleVisibilityAndEnabledUpdate();
		}

		public event EventHandler<bool> UseGraphicsEngineAsInputChanged;

		private ServiceSelectionSection GraphicsEngineSection => serviceSections.EndpointSections[VirtualPlatformType.VizremFarm].SingleOrDefault()?.DisplayedSection;

		private ServiceSelectionSection StudioDestinationSection => serviceSections.EndpointSections[VirtualPlatformType.VizremStudio].SingleOrDefault(x => x.Contains(StudioDestination?.Id ?? Guid.Empty))?.DisplayedSection;

		private Service StudioDestination => order.AllServices.SingleOrDefault(s => s.Definition.VirtualPlatform == VirtualPlatform.VizremStudio && !order.SourceService.Equals(s));

		/// <summary>
		/// Initializes the widgets within this section and the linking with the underlying model objects.
		/// </summary>
		private void Initialize()
		{
			LogMethodStart(nameof(Initialize));

			EnableLogging();
			InitializeWidgets();
			InitializeSections();
			SubscribeToWidgets();
			SubscribeToOrder();

			LogMethodCompleted(nameof(Initialize));
		}

		protected override void InitializeServiceSections()
		{
			List<Service> childServices = null;

			UpdateSourceServiceSection(this, order.SourceService);

			if (order.SourceService.Definition.VirtualPlatform == VirtualPlatform.VizremStudio)
			{
				childServices = OrderManager.FlattenServices(order.SourceService.Children);
			}
			else
			{
				childServices = order.AllServices;
			}

			foreach (var service in childServices)
			{
				switch (service.Definition.VirtualPlatformServiceType)
				{
					case VirtualPlatformType.VizremStudio:
					case VirtualPlatformType.VizremFarm:
						AddOrReplaceChildServiceSection(service);
						break;

					default:
						// No need to create service sections for other services
						continue;
				}
			}
		}

		protected override void InitializeWidgets()
		{
			base.InitializeWidgets();

			useGraphicsEngineAsInputCheckBox.IsChecked = order.SourceService.Definition.VirtualPlatform == VirtualPlatform.VizremFarm;
		}

		protected override void SubscribeToWidgets()
		{
			base.SubscribeToWidgets();

			useGraphicsEngineAsInputCheckBox.Changed += (s, e) => UseGraphicsEngineAsInputChanged?.Invoke(this, useGraphicsEngineAsInputCheckBox.IsChecked);
		}

		protected override void SubscribeToOrder()
		{
			base.SubscribeToOrder();

			order.UseGraphicsEngineAsInputChanged += Order_UseGraphicsEngineAsInputChanged;
		}

		private void Order_UseGraphicsEngineAsInputChanged(object sender, bool e)
		{
			useGraphicsEngineAsInputCheckBox.IsChecked = e;
		}

		protected override void GenerateUi()
		{
			Clear();

			int row = -1;

			GenerateHeaderUi(ref row);

			AddWidget(studioTitle, ++row, 0, 1, 8);

			GenerateSourceUi(ref row);

			if (StudioDestinationSection != null)
			{
				AddSection(StudioDestinationSection, new SectionLayout(++row, 0));
				row += StudioDestinationSection.RowCount;
			}
			else
			{
				Log(nameof(GenerateUi), $"Studio destination section is null for studio destination service '{StudioDestination?.Name}'");
			}

			AddWidget(graphicsEngineTitle, ++row, 0, 1, 8);
			AddWidget(useGraphicsEngineAsInputCheckBox, ++row, 0, 1, 8);

			if (GraphicsEngineSection != null)
			{
				AddSection(GraphicsEngineSection, new SectionLayout(row + 1, 0));
			}

			ToolTipHandler.AddToolTips(helpers, configuration, GetType(), this);
		}

		protected override void HandleVisibilityAndEnabledUpdate()
		{
			// only show the source section if the source is a vizrem studio
			bool sourceServiceWidgetsVisible = order.SourceService.Definition.VirtualPlatform == VirtualPlatform.VizremStudio;

			HandleMainVisibilityAndEnabledUpdate(sourceServiceWidgetsVisible, sourceServiceWidgetsVisible);

			useGraphicsEngineAsInputCheckBox.IsVisible = !order.AllServices.Exists(s => s.Definition.Description.Contains("ST26"));

			GraphicsEngineSection.IsVisible = IsVisible; // specifically trigger the visibility update on the Graphics Engine Section

			if (StudioDestinationSection != null)
			{
				StudioDestinationSection.IsVisible = order.SourceService.Definition.VirtualPlatform == VirtualPlatform.VizremFarm;
			}

			ToolTipHandler.SetTooltipVisibility(this);
		}

		public override List<Section> GetServiceSections(Service service)
		{
			if (service.Equals(order.SourceService))
			{
				return ((Section)SourceServiceSection).Yield().ToList();
			}
			else
			{
				return serviceSections.EndpointSections[service.Definition.VirtualPlatformServiceType].SingleOrDefault(x => x.Contains(service.Id))?.DisplayedSection?.ServiceSection?.Yield()?.ToList<Section>();
			}
		}
	}
}
