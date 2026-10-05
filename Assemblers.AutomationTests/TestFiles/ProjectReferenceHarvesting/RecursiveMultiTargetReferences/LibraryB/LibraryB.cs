namespace RecursiveMultiTargetReferences
{
    public static class LibraryB
    {
        public static string GetMessage()
        {
            return "B -> " + LibraryC.GetMessage();
        }
    }
}