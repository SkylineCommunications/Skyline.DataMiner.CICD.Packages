using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DeepRecursiveReferences
{
    public static class LibraryC
    {
        public static string GetMessage()
        {
            return "C -> " + LibraryD.GetMessage();
        }
    }
}
