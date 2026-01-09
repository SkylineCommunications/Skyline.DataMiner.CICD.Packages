namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class GetOrderReservationsSection : Section
	{
		private readonly CheckBox orderIdsCheckbox = new CheckBox("Newline-separated Order IDs");
		private readonly YleTextBox orderIdTextBox = new YleTextBox { IsMultiline = true, ValidationText = "Provide newline-separated guids", ValidationPredicate = text => text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).All(guid => Guid.TryParse(guid, out var parsedGuid)) };
		private readonly Button enterCurrentIdButton = new Button("Add Current ID") { Width = 200 };

		private readonly CheckBox orderStartCheckbox = new CheckBox("Order start");
		private readonly CheckBox orderStartFromCheckbox = new CheckBox("from");
		private readonly CheckBox orderStartUntilCheckbox = new CheckBox("until");
		private readonly DateTimePicker orderStartLowerLimitDateTimePicker = new DateTimePicker(DateTime.Now);
		private readonly DateTimePicker orderStartUpperLimitDateTimePicker = new DateTimePicker(DateTime.Now.AddDays(1));

		private readonly CheckBox orderEndCheckbox = new CheckBox("Order end");
		private readonly CheckBox orderEndFromCheckbox = new CheckBox("from");
		private readonly CheckBox orderEndUntilCheckbox = new CheckBox("until");
		private readonly DateTimePicker orderEndLowerLimitDateTimePicker = new DateTimePicker(DateTime.Now.AddDays(1));
		private readonly DateTimePicker orderEndUpperLimitDateTimePicker = new DateTimePicker(DateTime.Now.AddDays(2));

		private readonly CheckBox limitAmountOfOrdersToCheckBox = new CheckBox("Limit amount of Orders to") { IsChecked = true };
		private readonly Numeric limitAmountOfOrdersToNumeric = new Numeric(100) { Decimals = 0, Minimum = 1 };

		private readonly Helpers helpers;

		public GetOrderReservationsSection(Helpers helpers)
		{
			this.helpers = helpers;

			Initialize();
			GenerateUi();
		}

		public bool IsValid
		{
			get
			{
				return !orderIdsCheckbox.IsChecked || orderIdTextBox.IsValid;
			}
		}

		public List<FilterElement<ReservationInstance>> AdditionalFilters { get; } = new List<FilterElement<ReservationInstance>>();

		public List<ServiceReservationInstance> GetOrderReservations()
		{
			var orderStartFilter = GetOrderStartFilter();

			var orderEndFilter = GetOrderEndFilter();

			var orderIdFilter = GetOrderIdFilter();

			List<FilterElement<ReservationInstance>> filters = new List<FilterElement<ReservationInstance>>()
			{
				orderStartFilter,
				orderEndFilter,
				orderIdFilter
			};

			filters.AddRange(AdditionalFilters);

			var allCustomFilters = filters.Where(filter => filter != null).ToArray();
			if (!allCustomFilters.Any())
			{
				return new List<ServiceReservationInstance>();
			}

			var orderTypeFilter = ReservationInstanceExposers.Properties.DictStringField("Type").Equal("Video");

			var fullFilter = orderTypeFilter.AND(allCustomFilters);

			var orderReservations = DataMinerInterface.ResourceManager.GetReservationInstances(helpers, fullFilter).Cast<ServiceReservationInstance>().ToList();

			orderReservations = orderReservations.OrderBy(r => r.Start).ToList();

			if (limitAmountOfOrdersToCheckBox.IsChecked)
			{
				orderReservations = orderReservations.Take((int)limitAmountOfOrdersToNumeric.Value).ToList();
			}

			return orderReservations;
		}

		private FilterElement<ReservationInstance> GetOrderIdFilter()
		{
			if (!orderIdsCheckbox.IsChecked) return null;

			List<Guid> orderIds = new List<Guid>();
			foreach(var line in orderIdTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (Guid.TryParse(line.Trim(), out Guid result)) orderIds.Add(result);
			}

			if (!orderIds.Any()) return null;

			FilterElement<ReservationInstance> filter = null;

			foreach (var orderId in orderIds)
			{
				filter = filter is null ? ReservationInstanceExposers.ID.Equal(orderId) : filter.OR(ReservationInstanceExposers.ID.Equal(orderId));
			}

			return filter;
		}


		private FilterElement<ReservationInstance> GetOrderStartFilter()
		{
			FilterElement<ReservationInstance> orderStartFilter = null;

			if (!orderStartCheckbox.IsChecked) return null;

			if (orderStartFromCheckbox.IsChecked)
			{
				orderStartFilter = ReservationInstanceExposers.Start.GreaterThanOrEqual(orderStartLowerLimitDateTimePicker.DateTime);
			}

			if (orderStartUntilCheckbox.IsChecked)
			{
				var orderStartUpperLimitFilter = ReservationInstanceExposers.Start.LessThanOrEqual(orderStartUpperLimitDateTimePicker.DateTime);

				orderStartFilter = orderStartFilter is null ? orderStartUpperLimitFilter : orderStartFilter.AND(orderStartUpperLimitFilter);
			}

			return orderStartFilter;
		}

		private FilterElement<ReservationInstance> GetOrderEndFilter()
		{
			if (!orderEndCheckbox.IsChecked) return null;

			FilterElement<ReservationInstance> orderEndFilter = null;
			if (orderEndFromCheckbox.IsChecked)
			{
				orderEndFilter = ReservationInstanceExposers.End.GreaterThanOrEqual(orderEndLowerLimitDateTimePicker.DateTime);
			}

			if (orderEndUntilCheckbox.IsChecked)
			{
				var orderEndUpperLimitFilter = ReservationInstanceExposers.End.LessThanOrEqual(orderEndUpperLimitDateTimePicker.DateTime);

				orderEndFilter = orderEndFilter is null ? orderEndUpperLimitFilter : orderEndFilter.AND(orderEndUpperLimitFilter);
			}

			return orderEndFilter;
		}

		private void Initialize()
		{
			enterCurrentIdButton.Pressed += (s, e) =>
			{
				List<string> lines = orderIdTextBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
				lines.Add(helpers.Engine.GetScriptParam(1)?.Value);
				orderIdTextBox.Text = String.Join(Environment.NewLine, lines);
				orderIdsCheckbox.IsChecked = true;
			};
		}

		private void GenerateUi()
		{
			int row = -1;

			AddWidget(orderIdsCheckbox, ++row, 0, 1, 2, verticalAlignment: VerticalAlignment.Top);
			AddWidget(orderIdTextBox, row, 2, 1, 2);
			AddWidget(enterCurrentIdButton, row, 4, verticalAlignment: VerticalAlignment.Top);

			AddWidget(orderStartCheckbox, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(orderStartFromCheckbox, row, 1);
			AddWidget(orderStartLowerLimitDateTimePicker, row, 2);
			AddWidget(orderStartUntilCheckbox, ++row, 1);
			AddWidget(orderStartUpperLimitDateTimePicker, row, 2);

			AddWidget(orderEndCheckbox, ++row, 0, verticalAlignment: VerticalAlignment.Top);
			AddWidget(orderEndFromCheckbox, row, 1);
			AddWidget(orderEndLowerLimitDateTimePicker, row, 2);
			AddWidget(orderEndUntilCheckbox, ++row, 1);
			AddWidget(orderEndUpperLimitDateTimePicker, row, 2);

			AddWidget(limitAmountOfOrdersToCheckBox, ++row, 0, 1, 2);
			AddWidget(limitAmountOfOrdersToNumeric, row, 2, 1, 2);
		}
	}
}
