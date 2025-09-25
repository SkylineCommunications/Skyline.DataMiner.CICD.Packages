namespace Skyline.DataMiner.CICD.Parsers.Common.XmlEdit
{
    public class TextChange
    {
        public int OldPosition { get; private set; }

        public int OldLength { get; private set; }

        public int NewPosition { get; private set; }

        public string NewText { get; private set; }

        public ChangeType Type { get; }

        private TextChange(ChangeType type)
        {
            Type = type;
        }

        public static TextChange CreateInsert(int position, string text)
        {
            return new TextChange(ChangeType.Insertion)
            {
                NewPosition = position,
                NewText = text,
            };
        }

        public static TextChange CreateDelete(int startPosition, int charsToDelete)
        {
            return new TextChange(ChangeType.Deletion)
            {
                OldPosition = startPosition,
                OldLength = charsToDelete,
            };
        }

        public static TextChange CreateReplace(int startPosition, int charsToReplace, string replaceWith)
        {
            return new TextChange(ChangeType.Replace)
            {
                OldPosition = startPosition,
                OldLength = charsToReplace,
                NewPosition = startPosition,
                NewText = replaceWith,
            };
        }

        public override string ToString()
        {
            switch (Type)
            {
                case ChangeType.Insertion:
                    return "INSERT: " + NewPosition + " - '" + NewText + "'";

                case ChangeType.Deletion:
                    return "DELETE: " + OldPosition + " - length: " + OldLength;

                case ChangeType.Replace:
                    return "REPLACE: " + OldPosition + " - length: " + OldLength + " - '" + NewText + "'";

                default:
                    return base.ToString();
            }
        }
    }

    public enum ChangeType
    {
        Insertion,
        Deletion,
        Replace,
    }
}