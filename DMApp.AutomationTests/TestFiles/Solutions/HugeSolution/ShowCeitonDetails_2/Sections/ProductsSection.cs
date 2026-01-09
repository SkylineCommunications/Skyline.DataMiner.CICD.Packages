namespace ShowCeitonDetails_2.Sections
{
	using System.Collections.Generic;
	using System.Linq;
	using ShowCeitonDetails_2.Ceiton;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ProductsSection : Section
	{
		private readonly Label productsTitle = new Label("Products") { Style = TextStyle.Bold };
		private readonly CollapseButton collapseButton;
		private readonly Label productNumberLabel = new Label("PRODUCT NUMBER");
		private readonly Label productNameLabel = new Label("PRODUCT NAME");
		private readonly Label productDescriptionLabel = new Label("PRODUCT DESCRIPTION");
		private readonly Label noProductsAvailableLabel = new Label("No Ceiton product details available for this event");

		private Widget[] productDetailsWidgets;

		public ProductsSection(Engine engine, IEnumerable<Product> products)
		{
			CreateWidgets(products);
			collapseButton = new CollapseButton(new[] { productNumberLabel, productNameLabel, productDescriptionLabel, noProductsAvailableLabel }.Concat(productDetailsWidgets), isCollapsed: false) { CollapseText = "-", ExpandText = "+", Width = 44 };
			GenerateUI();
		}

		private void CreateWidgets(IEnumerable<Product> products)
		{
			List<Widget> createdWidgets = new List<Widget>();

			foreach (Product product in products)
			{
				createdWidgets.Add(new Label(product.Number));
				createdWidgets.Add(new Label(product.Name));
				createdWidgets.Add(new Label(product.Description));
			}

			productDetailsWidgets = createdWidgets.ToArray();
		}

		private void GenerateUI()
		{
			int row = 0;

			AddWidget(collapseButton, new WidgetLayout(row, 0));
			AddWidget(productsTitle, new WidgetLayout(row, 1, 1, 5));

			if (productDetailsWidgets.Any())
			{
				AddWidget(productNumberLabel, new WidgetLayout(++row, 1, 1, 2));
				AddWidget(productNameLabel, new WidgetLayout(row, 3, 1, 2));
				AddWidget(productDescriptionLabel, new WidgetLayout(row, 5, 1, 2));

				int rowCounter = 0;
				int columnCounter = 1;
				foreach (Widget widget in productDetailsWidgets)
				{
					AddWidget(widget, new WidgetLayout(rowCounter % 3 == 0 ? ++row : row, columnCounter, 1, 2));
					rowCounter++;
					columnCounter = (columnCounter + 2) % 6;
				}
			}
			else
			{
				AddWidget(noProductsAvailableLabel, new WidgetLayout(++row, 1, 1, 4));
			}
		}
	}

}