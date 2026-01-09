namespace Skyline.DataMiner.CICD.Parsers.Automation.Xml
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    /// <summary>
    /// Represents an Exe block of an Automation script.
    /// </summary>
    public class ScriptExe
    {
        private readonly List<string> _dllImports = new List<string>();
        private readonly List<string> _scriptReferences = new List<string>();
        private readonly List<string> _namespaces = new List<string>();
        private IList<string> _codeLines = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptExe"/> class.
        /// </summary>
        /// <param name="node">The node that represents the Exe block.</param>
        public ScriptExe(XmlElement node)
        {
            Node = node;
            Id = Convert.ToInt32(node.GetAttributeValue("id"));
            Type = node.GetAttributeValue("type");

            ParseValue(node);

            foreach (var param in node.Elements["param"])
            {
                ParseParam(param);
            }
        }
        
        /// <summary>
        /// Gets the DLL imports.
        /// </summary>
        /// <value>The DLL imports.</value>
        public IEnumerable<string> DllImports => _dllImports;

        /// <summary>
        /// Gets the script references.
        /// </summary>
        /// <value>The script references.</value>
        public IEnumerable<string> ScriptReferences => _scriptReferences;

        /// <summary>
        /// Gets the namespaces.
        /// </summary>
        /// <value>The namespaces.</value>
        public IEnumerable<string> Namespaces => _namespaces;

        /// <summary>
        /// Gets the ID.
        /// </summary>
        /// <value>The ID.</value>
        public int Id { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; }

        /// <summary>
        /// Gets a value indicating whether this is compiled in debug mode.
        /// </summary>
        /// <value><c>true</c> if compiled in debug mode; otherwise, <c>false</c>.</value>
        public bool IsDebug { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is precompiled.
        /// </summary>
        /// <value><c>true</c> if precompiled; otherwise, <c>false</c>.</value>
        public bool IsPrecompile { get; private set; }

        /// <summary>
        /// Gets the library name.
        /// </summary>
        /// <value>The library name.</value>
        public string LibraryName { get; private set; }

        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>The code.</value>
        public string Code { get; private set; }

        /// <summary>
        /// Gets the code lines.
        /// </summary>
        /// <value>The code lines.</value>
        public IEnumerable<string> CodeLines => _codeLines;

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <value>The node.</value>
        public XmlElement Node { get; }

        /// <summary>
        /// Gets the node CDATA.
        /// </summary>
        /// <value>The node CDATA.</value>
        public XmlCDATA NodeCDATA { get; private set; }

        private void ParseParam(XmlElement param)
        {
            string type = param.GetAttributeValue("type");

            if (String.Equals(type, "using", StringComparison.OrdinalIgnoreCase))
            {
                _namespaces.Add(param.InnerText);
            }
            else if (String.Equals(type, "ref", StringComparison.OrdinalIgnoreCase))
            {
                _dllImports.Add(param.InnerText);
            }
            else if (String.Equals(type, "scriptRef", StringComparison.OrdinalIgnoreCase))
            {
                _scriptReferences.Add(param.InnerText);
            }
            else if (String.Equals(type, "debug", StringComparison.OrdinalIgnoreCase))
            {
                if (String.Equals(param.InnerText, "true", StringComparison.OrdinalIgnoreCase))
                {
                    IsDebug = true;
                }
            }
            else if (String.Equals(type, "preCompile", StringComparison.OrdinalIgnoreCase))
            {
                if (String.Equals(param.InnerText, "true", StringComparison.OrdinalIgnoreCase))
                {
                    IsPrecompile = true;
                }
            }
            else if (String.Equals(type, "libraryName", StringComparison.OrdinalIgnoreCase))
            {
                LibraryName = param.InnerText;
            }
            else
            {
                // Do nothing.
            }
        }

        private void ParseValue(XmlElement node)
        {
            var valueNode = node.Element["Value"];
            if (valueNode == null)
            {
                return;
            }

            NodeCDATA = valueNode
                        .Children.OfType<XmlCDATA>()
                        .FirstOrDefault();

            Code = NodeCDATA != null ? NodeCDATA.InnerText : valueNode.InnerText;
            _codeLines = Code.Split(new[] { "\r\n" }, StringSplitOptions.None);
            for (int i = 0; i < _codeLines.Count; i++)
            {
                _codeLines[i] = _codeLines[i].Replace("\t", "    ");
            }
        }
    }
}