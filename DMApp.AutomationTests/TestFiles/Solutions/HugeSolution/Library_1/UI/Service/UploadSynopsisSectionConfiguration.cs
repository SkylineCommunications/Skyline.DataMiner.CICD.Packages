namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Service
{
    public class UploadSynopsisSectionConfiguration
    {
        [IsIsVisibleProperty]
        public bool IsVisible { get; set; } = false;

        [IsIsEnabledProperty]
        public bool IsEnabled { get; set; } = true;

        public int LabelSpan { get; internal set; } = 2;

        public int InputWidgetSpan { get; internal set; } = 2;

        public int InputWidgetColumn { get; internal set; } = 2;
    }
}
