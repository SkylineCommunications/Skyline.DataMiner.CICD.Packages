namespace DiamondReferences
{
    public static class LibraryA
    {
        public static string GetMessage()
        {
            return $"{LibraryB.GetMessage()} | {LibraryC.GetMessage()}";
        }
    }
}