using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepRecursiveReferences
{
    public static class LibraryB
    {
        public static string GetMessage()
        {
            return "B -> " + LibraryC.GetMessage();
        }
    }
}
