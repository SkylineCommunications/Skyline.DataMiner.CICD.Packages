namespace DiamondReferences
{
    public static class LibraryB
    {
        public static string GetMessage()
        {
            return "B -> " + LibraryD.GetMessage();
        }
    }
}