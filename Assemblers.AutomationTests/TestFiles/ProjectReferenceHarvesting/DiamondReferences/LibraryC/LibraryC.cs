namespace DiamondReferences
{
    public static class LibraryC
    {
        public static string GetMessage()
        {
            return "C -> " + LibraryD.GetMessage();
        }
    }
}