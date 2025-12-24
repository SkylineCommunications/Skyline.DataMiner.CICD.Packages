namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Events
{
    using System.ComponentModel;

    public enum EventSelection
    {
        [Description("Other event")]
        OtherExistingEvent,

        [Description("New event")]
        NewEvent,

        [Description("Same event")]
        SameEvent
    }
}