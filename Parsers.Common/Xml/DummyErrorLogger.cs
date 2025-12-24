namespace Skyline.DataMiner.CICD.Parsers.Common.Xml
{
    internal class DummyErrorLogger : IErrorLogger
    {
        private DummyErrorLogger() { }

        #region SingleTon

        public static DummyErrorLogger Instance { get; } = new DummyErrorLogger();

        #endregion

        public void Log(int offset, string message, params object[] formatParams)
        {
            (this as IErrorLogger).Log(offset, message, formatParams);
        }

        void IErrorLogger.Log(int offset, string message, params object[] formatParams)
        {
            Log(offset, 0, message, formatParams);
        }

        public void Log(int offset, int length, string message, params object[] formatParams)
        {
            (this as IErrorLogger).Log(offset, length, message, formatParams);
        }

        void IErrorLogger.Log(int offset, int length, string message, params object[] formatParams)
        {

        }
    }
}