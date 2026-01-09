namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    /// <summary>
    /// Indicates the type of a parsed Token: text/whitespace, normal tag or special tag.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Represents either whitespace between tags, or the inner text between an opening and closing tag.
        /// </summary>
        Text,

        /// <summary>
        /// Represents an invalid XML tag, for example unclosed tags like "&lt;xxx" or "xxx&gt;", or a tag with invalid name characters like "&lt;@?*#&gt;". 
        /// </summary>
        Invalid,

        /// <summary>
        /// Represents a valid XML element tag.
        /// </summary>
        Element,

        /// <summary>
        /// Represents a valid XML processing tag: &lt;? ... ?&gt; &lt;% ... %&gt; &lt;! ... &gt;
        /// </summary>
        Special
    }

    /// <summary>
    /// Indicates the type of an XML element tag: opening, closing or self-contained.
    /// </summary>
    public enum ElementTagType
    {
        /// <summary>
        /// Represents an opening tag, eg. &lt;Grid width=&quot;123&quot;&gt;
        /// </summary>
        Opening,

        /// <summary>
        /// Represents an opening tag, eg. &lt;/Grid&gt;
        /// </summary>
        Closing,

        /// <summary>
        /// Represents a self-closing tag, eg. &lt;Grid /&gt;
        /// </summary>
        SelfContained
    }
}