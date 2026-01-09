namespace ShowCeitonDetails_2.Dialogs
{
	using System;
	using System.Collections.Generic;
	using ShowCeitonDetails_2.Ceiton;
	using ShowCeitonDetails_2.Sections;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Product = Ceiton.Product;

	public class DetailsDialog : Dialog
	{
		private readonly ProjectSection projectSection;
		private readonly ProductsSection productsSection;
		private readonly TasksTableSection projectTasksSection;
		private readonly TasksTableSection productTasksSection;
		private readonly Button okButton = new Button("OK") { Width = 100, Style = ButtonStyle.CallToAction };

		private readonly IEnumerable<Product> products;

		public DetailsDialog(Engine engine, CeitonManager ceitonManager, string projectNumber) : base(engine)
		{
			this.Title = "Event Details";

			AllowOverlappingWidgets = true;

			products = ceitonManager.GetProducts(projectNumber);

			projectSection = new ProjectSection(engine, ceitonManager.GetProject(projectNumber));

			productsSection = new ProductsSection(engine, products);

			projectTasksSection = new TasksTableSection(engine, ceitonManager.GetProjectTasks(projectNumber), "Project Tasks");
			projectTasksSection.RegenerateUiRequired += (sender, args) => GenerateUI();

			productTasksSection = new TasksTableSection(engine, ceitonManager.GetProductTasks(projectNumber), "Product Tasks", products);
			productTasksSection.RegenerateUiRequired += (sender, args) => GenerateUI();

			okButton.Pressed += (sender, args) => engine.ExitSuccess(String.Empty);

			GenerateUI();
		}

		private void GenerateUI()
		{
			Clear();

			int row = 0;

			AddSection(projectSection, new SectionLayout(row, 0));
			row += projectSection.RowCount;

			AddSection(productsSection, new SectionLayout(++row, 0));
			row += productsSection.RowCount;

			AddSection(projectTasksSection, new SectionLayout(++row, 0));
			row += projectTasksSection.RowCount;

			AddSection(productTasksSection, new SectionLayout(++row, 0));
			row += productTasksSection.RowCount;

			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));
			AddWidget(new WhiteSpace(), new WidgetLayout(++row, 0));
			AddWidget(okButton, new WidgetLayout(++row, 0, 1, 3));

			for (int i = 2; i < 12; i++)
			{
				SetColumnWidth(i, 170);
			}

			SetColumnWidth(12, 250);
			SetColumnWidth(13, 1000);
		}
	}
}