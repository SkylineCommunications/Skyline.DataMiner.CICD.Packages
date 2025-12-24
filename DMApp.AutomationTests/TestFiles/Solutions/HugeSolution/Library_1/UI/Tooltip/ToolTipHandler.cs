namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.ToolTip
{
    using Newtonsoft.Json;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reflection;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using System;
    using System.Linq;

    public static class ToolTipHandler
    {
        private const int TooltipColumn = 200;

        public static void AddToolTips(Helpers helpers, ISectionConfiguration configuration, Type type, Section section)
        {
            try
            {
                AddTooltipsForWidgetsInSectionProperties(helpers, type, configuration, section);

                AddTooltipsForWidgetsInSectionFields(helpers, type, configuration, section);
            }
            catch (Exception e)
            {
                helpers?.Log(nameof(ToolTipHandler), nameof(AddToolTip), $"Exception occurred: {e}");
            }
        }

        private static void AddTooltipsForWidgetsInSectionFields(Helpers helpers, Type type, ISectionConfiguration configuration, Section section)
        {
            var fieldsInfo = ReflectionHandler.GetFieldsInfo(type);

            if (fieldsInfo == null || !fieldsInfo.Any())
            {
                helpers.Log(nameof(ToolTipHandler), nameof(AddTooltipsForWidgetsInSectionFields), $"fieldsInfo Null or Empty");
                return;
            }

            foreach (var sectionField in fieldsInfo)
            {
                var sectionPropertyCurrentValue = sectionField.GetValue(section);

                try
                {
                    if (sectionPropertyCurrentValue is Widget widgetObj && widgetObj.GetType() != typeof(Label))
                    {
                        AddToolTip(helpers, configuration, type.Name, sectionField.Name, widgetObj, section);
                    }
                }
                catch (Exception e)
                {
                    helpers.Log(nameof(ToolTipHandler), nameof(AddTooltipsForWidgetsInSectionFields), $"Error Widget widgetObj {e}");
                }
            }
        }

        private static void AddTooltipsForWidgetsInSectionProperties(Helpers helpers, Type type, ISectionConfiguration configuration, Section section)
        {
            var propertiesInfo = ReflectionHandler.GetPropertiesInfo(type);

            if (propertiesInfo == null || !propertiesInfo.Any())
            {
                helpers.Log(nameof(ToolTipHandler), nameof(AddTooltipsForWidgetsInSectionProperties), $"propertiesInfo Null or Empty");
                return;
            }

            foreach (var sectionProperty in propertiesInfo)
            {
                var sectionPropertyCurrentValue = sectionProperty.GetValue(section);

                try
                {
                    if (sectionPropertyCurrentValue is Widget widgetObj)
                    {
                        AddToolTip(helpers, configuration, type.Name, sectionProperty.Name, widgetObj, section);
                    }
                }
                catch (Exception e)
                {
                    helpers.Log(nameof(ToolTipHandler), nameof(AddTooltipsForWidgetsInSectionProperties), $"Error Widget widgetObj {e}");
                }
            }
        }

        private static void AddToolTip(Helpers helpers, ISectionConfiguration configuration, string sectionName, string field_property_name, Widget widgetObj, Section section)
        {
			if (!section.Widgets.Contains(widgetObj))
			{
                helpers.Log(nameof(ToolTipHandler), nameof(AddToolTip), $"Widget is not part of the section");
                return;
            }

            string toolTipKey = $"{sectionName}_{field_property_name}";

            if (configuration.ToolTip.TryGetValue(toolTipKey, out var tooltipValue) && !String.IsNullOrWhiteSpace(tooltipValue))
            {
                var widgetObjLayout = section.GetWidgetLayout(widgetObj);

                var existingTooltip = section.Widgets.SingleOrDefault(w => section.GetWidgetLayout(w).Row == widgetObjLayout.Row && section.GetWidgetLayout(w).Column == 200);

                if (existingTooltip != null)
                {
                    ((Label)existingTooltip).Tooltip += "\n" + tooltipValue;
                }
                else
                {
                    Label tooltip_Info = new Label("ⓘ") { IsVisible = section.IsVisible, Tooltip = tooltipValue, Style = TextStyle.Title };
                    section.AddWidget(tooltip_Info, widgetObjLayout.Row, TooltipColumn, verticalAlignment: VerticalAlignment.Top);
                }
            }
        }

        public static void SetTooltipVisibility(Section section)
        {
            for (int i = 0; i < section.RowCount; i++)
            {
                var existingTooltip = (Label)section.Widgets.FirstOrDefault(w => section.GetWidgetLayout(w).Row == i && section.GetWidgetLayout(w).Column >= 100);
                
                if (existingTooltip == null) continue;

                var visibleWidgetsOnRow = section.Widgets.Where(w => section.GetWidgetLayout(w).Row == i && section.GetWidgetLayout(w).Column <= 100 && w.IsVisible).Except(existingTooltip.Yield());

                existingTooltip.IsVisible = visibleWidgetsOnRow.Any();
            }
        }
    }
}