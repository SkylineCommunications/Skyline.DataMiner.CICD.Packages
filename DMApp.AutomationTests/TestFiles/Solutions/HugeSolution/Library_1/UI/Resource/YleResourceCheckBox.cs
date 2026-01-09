namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Resource
{
	using System;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

	public sealed class YleResourceCheckBox : YleCheckBox
	{
        public YleResourceCheckBox(string functionDefinitionLabel, string text) : base(text)
        {
            if (string.IsNullOrWhiteSpace(functionDefinitionLabel)) throw new ArgumentNullException(nameof(functionDefinitionLabel));
            FunctionDefinitionLabel = functionDefinitionLabel;
        }

        public string FunctionDefinitionLabel { get; private set; }
    }
}
