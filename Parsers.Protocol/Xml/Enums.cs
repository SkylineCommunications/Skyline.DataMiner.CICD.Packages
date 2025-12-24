namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml
{
    using System.ComponentModel;

    public enum ColumnSortType
    {
        Integer,
        String,
    }

    public enum ColumnSortAutomatic
    {
        None,
        Ascending,
        Descending,
    }

    public enum ColumnHeaderIndication
    {
        None,
        Sum,
        Avg,
        Min,
        Max,
    }

    public enum TriggerType
    {
        Action,
        Trigger,
    }

    public enum GroupType
    {
        [Description("Poll")]
        Poll,
        [Description("Action")]
        Action,
        [Description("Trigger")]
        Trigger,
        [Description("Poll Action")]
        PollAction,
        [Description("Poll Trigger")]
        PollTrigger
    }

    public enum ContentType
    {
        Group,
        Parameter,
        Trigger,
        Action,
        Pair,
        Session,
    }
}
