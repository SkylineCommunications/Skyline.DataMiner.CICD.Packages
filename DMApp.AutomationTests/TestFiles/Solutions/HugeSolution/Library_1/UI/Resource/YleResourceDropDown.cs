namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Resource
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;

	public class YleResourceDropDown : YleDropDown
    {
        public YleResourceDropDown(string functionDefinitionLabel, IEnumerable<string> options, string selected = null) : base(options, selected)
        {
            if (string.IsNullOrWhiteSpace(functionDefinitionLabel)) throw new ArgumentNullException(nameof(functionDefinitionLabel));
            FunctionDefinitionLabel = functionDefinitionLabel;
        }

        public string FunctionDefinitionLabel { get; private set; }
    }
}
