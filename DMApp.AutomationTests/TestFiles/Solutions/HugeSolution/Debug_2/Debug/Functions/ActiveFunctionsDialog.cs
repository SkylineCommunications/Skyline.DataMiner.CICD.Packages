namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Debug.Functions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Solutions.SRM;

	public class ActiveFunctionsDialog : DebugDialog
	{
		private readonly Label viewActiveFunctionsLabel = new Label("Active Functions") { Style = TextStyle.Bold };
		private readonly Label duplicateFunctionsLabel = new Label("Duplicate Functions") { Style = TextStyle.Bold };
		private readonly Label functionNameLabel = new Label("Name") { Style = TextStyle.Heading };
		private readonly Label functionIdLabel = new Label("ID") { Style = TextStyle.Heading };
		private readonly List<Tuple<string, Guid, string>> functions = new List<Tuple<string, Guid, string>>();
		//private readonly List<Tuple<string, Guid>> duplicateFunctions = new List<Tuple<string, Guid>>();

		public ActiveFunctionsDialog(Helpers helpers) : base(helpers)
		{
			Title = "View Active Functions";

			Initialize();
			GenerateUi();
		}

		private void Initialize()
		{
			var activeProtocolFunctionVersions = SrmManagers.ProtocolFunctionManager.GetAllProtocolFunctions(true).Select(p => p.ProtocolFunctionVersions.FirstOrDefault());
			foreach (var activeProtocolFunctionVersion in activeProtocolFunctionVersions)
			{
				foreach (var functionDefinition in activeProtocolFunctionVersion.FunctionDefinitions)
				{
					var functionDefinitionId = functionDefinition.FunctionDefinitionID.Id;
					functions.Add(new Tuple<string, Guid, string>(functionDefinition.Name, functionDefinitionId, activeProtocolFunctionVersion.ProtocolName));
				}
			}

			// Check for duplicates
			//for (int i = 0; i < functions.Count; i++)
			//{
			//	var possibleDuplicates = functions.Where(x => x.Item1.Equals(functions[i].Item1));
			//	if (possibleDuplicates.Count() <= 1) continue;

			//	foreach (var duplicateFunction in possibleDuplicates)
			//	{
			//		if (duplicateFunctions.Any(x => x.Item1.Equals(duplicateFunction.Item1) && x.Item2.Equals(duplicateFunction.Item2))) continue;
			//		duplicateFunctions.Add(duplicateFunction);
			//	}
			//}
		}

		private void GenerateUi()
		{
			Clear();

			int row = -1;

			AddWidget(BackButton, ++row, 0, 1, 3);

			AddWidget(viewActiveFunctionsLabel, ++row, 0, 1, 5);

			AddWidget(functionNameLabel, ++row, 0);
			AddWidget(functionIdLabel, row, 1);
			foreach (var kvp in functions.OrderBy(x => x.Item1))
			{
				AddWidget(new Label(kvp.Item1), ++row, 0);
				AddWidget(new Label(kvp.Item2.ToString()), row, 1);
				AddWidget(new Label(kvp.Item3 ?? String.Empty), row, 2);
			}

			//if (duplicateFunctions.Any())
			//{
			//	AddWidget(new WhiteSpace(), ++row, 0);
			//	AddWidget(duplicateFunctionsLabel, ++row, 0, 1, 5);
			//	foreach (var kvp in duplicateFunctions.OrderBy(x => x.Item1))
			//	{
			//		AddWidget(new Label(kvp.Item1), ++row, 0);
			//		AddWidget(new Label(kvp.Item2.ToString()), row, 1);
			//	}
			//}
		}
	}
}
