namespace Skyline.DataMiner.CICD.Parsers.Protocol.Xml.QActions
{
    public class QActionCodeFile
    {
        public QActionCodeFile(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public QActionCodeFile(string code)
        {
            Code = code;
        }

        public string Name { get; private set; }

        public string Code { get; private set; }
    }
}
