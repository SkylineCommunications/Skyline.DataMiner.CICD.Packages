namespace RecursiveSharedProjectReference
{
    using RecursiveSharedProjectReference.Shared;

    public static class LibraryB
    {
        public static string GetMessage()
        {
            return "B -> " + SharedMarker.GetMessage();
        }
    }
}