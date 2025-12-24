namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    internal interface IErrorLogger
    {
        void Log(int offset, string message, params object[] formatParams);

        void Log(int offset, int length, string message, params object[] formatParams);
    }
}