namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml.QActions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Skyline.DataMiner.CICD.Parsers.Common.Xml;

    public class QAction
    {
        public static readonly Regex RegexExtractQActionRef = new Regex(@"\[ProtocolName\]\.\[ProtocolVersion\]\.(?<dll>.+\.dll)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly Regex RegexExtractQActionID = new Regex(@"QAction[_\.](?<id>[0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public QAction(XmlElement node)
        {
            NodeQAction = node;
            NodeCDATA = node.Children.OfType<XmlCDATA>()
                                     .FirstOrDefault();

            Int32.TryParse(node.GetAttributeValue("id"), out int id);
            Id = id;

            IsRow = String.Equals(node.GetAttributeValue("row"), "true", StringComparison.OrdinalIgnoreCase);
            Encoding = node.GetAttributeValue("encoding");
            Options = node.GetAttributeValue("options");
            Name = node.GetAttributeValue("name");

            string code = NodeCDATA?.InnerText;
            if (code != null)
            {
                files.Add(new QActionCodeFile(code));
            }

            var items = node.GetAttributeValue("triggers");
            if (!String.IsNullOrEmpty(items))
            {
                var y = items.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var z in y)
                {
                    if (Int32.TryParse(z, out int x))
                    {
                        triggers.Add(x);
                    }
                }
            }

            items = node.GetAttributeValue("inputParameters");
            if (!String.IsNullOrEmpty(items))
            {
                var y = items.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var z in y)
                {
                    if (Int32.TryParse(z, out int x))
                    {
                        inputParameters.Add(x);
                    }
                }
            }

            items = node.GetAttributeValue("dllImport");
            if (!String.IsNullOrEmpty(items))
            {
                var y = items.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                dllImports.AddRange(y);
            }

            ParseOptions();
        }

        public QAction(int id, string name, ICollection<QActionCodeFile> files)
        {
            Id = id;
            Name = name;
            this.files.AddRange(files);
        }

        public QAction(int id, IList<QActionCodeFile> files, IList<string> dllImports)
        {
            Id = id;
            this.dllImports.AddRange(dllImports);
            this.files.AddRange(files);
        }

        public QAction(int id, string name, ICollection<QActionCodeFile> files, ICollection<string> dllImports)
        {
            Id = id;
            Name = name;
            this.dllImports.AddRange(dllImports);
            this.files.AddRange(files);
        }

        private readonly List<QActionCodeFile> files = new List<QActionCodeFile>();
        private readonly List<string> dllImports = new List<string>();
        private readonly List<int> triggers = new List<int>();
        private readonly List<int> inputParameters = new List<int>();

        public IList<QActionCodeFile> Files => files;

        public IList<string> DllImports => dllImports;

        public IList<int> Triggers => triggers;

        public IList<int> InputParameters => inputParameters;

        public int Id { get; private set; }

        public bool IsRow { get; private set; }

        public string Name { get; private set; }

        public string Encoding { get; private set; }

        public string Options { get; private set; }

        public bool IsPrecompile { get; private set; }

        public string CustomDllName { get; private set; }

        public string Code
        {
            get
            {
                if (files.Count < 1)
                {
                    return null;
                }

                return files[0].Code;
            }
        }

        public XmlElement NodeQAction { get; private set; }

        public XmlCDATA NodeCDATA { get; private set; }

        private void ParseOptions()
        {
            string[] options = (Options ?? "").Split(';');

            foreach (string option in options)
            {
                var parts = option.Split(new[] { '=' }, 2);

                string optName = parts[0].Trim();

                if (String.Equals(optName, "precompile", StringComparison.OrdinalIgnoreCase))
                {
                    IsPrecompile = true;
                }
                else if (String.Equals(optName, "dllname", StringComparison.OrdinalIgnoreCase) && parts.Length == 2)
                {
                    string dllName = parts[1].Trim();

                    if (!dllName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        dllName += ".dll";
                    }

                    CustomDllName = dllName;
                }
            }
        }

        public static bool IsReferencedQActionDllImport(QAction qa, string dllImport, ICollection<QAction> allQActions, out QAction referencedQAction)
        {
            referencedQAction = null;

            var qactionRefMatch = RegexExtractQActionRef.Match(dllImport);
            if (!qactionRefMatch.Success)
            {
                return false;
            }

            string dll = qactionRefMatch.Groups["dll"].Value;

            var qactionIdMatch = RegexExtractQActionID.Match(dll);
            if (qactionIdMatch.Success)
            {
                var qactionId = Convert.ToInt32(qactionIdMatch.Groups["id"].Value);
                referencedQAction = allQActions.FirstOrDefault(qAction => qAction.Id == qactionId);
            }
            else
            {
                foreach (var q in allQActions)
                {
                    if (String.Equals(q.CustomDllName, dll, StringComparison.OrdinalIgnoreCase))
                    {
                        referencedQAction = q;
                        break;
                    }
                }
            }

            return referencedQAction != null;
        }
    }
}
