namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.ExternalJson
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities.Attributes;
	using Service = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Service;

	public static class ExternalJsonOrderCreator
	{
		public static Order CreateOrder(Helpers helpers, YLE.Order.ExternalJson.ExternalJson externalJson)
		{
			if (helpers == null) throw new ArgumentNullException(nameof(helpers));
			if (externalJson == null) throw new ArgumentNullException(nameof(externalJson));

			var order = new Order();

			FillOrderProperties(helpers, externalJson.Order, order);

			CleanOrderName(order);

			order.AcceptChanges();

			var sourceService = Service.FromExternalJson(helpers, externalJson.Order.MainSignal.Source);
			sourceService.Start = order.Start;
			sourceService.End = order.End;
			sourceService.AcceptChanges();

			order.SourceService = sourceService;

			return order;
		}

		private static void FillOrderProperties(Helpers helpers, JsonOrder jsonOrder, Order order)
		{
			var orderProperties = typeof(Order).GetProperties();

			foreach (var jsonProperty in typeof(JsonOrder).GetProperties())
			{
				var jsonPropertyValue = jsonProperty.GetValue(jsonOrder);
				if (jsonProperty.PropertyType == typeof(string))
				{
					// Temporary workaround to avoid that reservation properties don't include any escaped characters.
					string jsonPropertyStringValue = Convert.ToString(jsonPropertyValue);
					jsonPropertyStringValue = Regex.Unescape(jsonPropertyStringValue);
					jsonPropertyValue = jsonPropertyStringValue;
				}

				var attributes = jsonProperty.GetCustomAttributes(true);
				var matchingOrderPropertyNameAttribute = attributes.SingleOrDefault(a => a is MatchingOrderPropertyAttribute) as MatchingOrderPropertyAttribute;

				helpers?.Log(nameof(Order), nameof(FillOrderProperties), $"External JSON Order property {jsonProperty.Name} has value '{jsonPropertyValue?.ToString()}' and matching order property attribute value '{matchingOrderPropertyNameAttribute?.MatchingOrderPropertyName}'");

				bool jsonPropertyRepresentsOrderProperty = matchingOrderPropertyNameAttribute != null;
				if (!jsonPropertyRepresentsOrderProperty) continue;

				var matchingOrderProperty = orderProperties.SingleOrDefault(p => p.Name == matchingOrderPropertyNameAttribute.MatchingOrderPropertyName);
				if (matchingOrderProperty == null) continue;

				matchingOrderProperty.SetValue(order, Convert.ChangeType(jsonPropertyValue, jsonProperty.PropertyType, CultureInfo.InvariantCulture));

				helpers?.Log(nameof(Order), nameof(FillOrderProperties), $"Set order property {matchingOrderProperty.Name} to value {matchingOrderProperty.GetValue(order).ToString()}");
			}
		}

		private static void CleanOrderName(LiteOrder order)
		{
			foreach (var disallowedCharacter in LiteOrder.OrderNameDisallowedCharacters)
			{
				order.ManualName = order.Name?.Replace($" {disallowedCharacter} ", " ");
				order.ManualName = order.Name?.Replace($" {disallowedCharacter}", " ");
				order.ManualName = order.Name?.Replace($"{disallowedCharacter} ", " ");
				order.ManualName = order.Name?.Replace($"{disallowedCharacter}", " ");
			}
		}
	}
}
