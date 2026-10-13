namespace RecursiveMultiTargetReferences
{
    public static class LibraryA
    {
        public static string GetMessage()
        {
            return "A -> " + LibraryB.GetMessage();
        }
    }
}