namespace UnitTestProject.Package
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    [TestClass]
    public class PackageTests
    {
        [TestMethod]
        public void VerifyScriptVersions()
        {
            string version = String.Empty;
            foreach (string file in GetScriptFiles())
            {
                XDocument document;
                try
                {
                    document = XDocument.Load(file);
                }
                catch (Exception)
                {
                    continue;
                }

                if (!document.Root.Name.LocalName.Equals("DMSScript")) continue; // Skip non script files

                var descriptionElement = document.Root.Elements().FirstOrDefault(x => String.Equals(x.Name.LocalName, "Description"));
                Assert.IsNotNull(descriptionElement, $"Script: {file}");

                if (String.IsNullOrEmpty(version))
                {
                    version = descriptionElement.Value;
                    continue;
                }

                Assert.AreEqual(version, descriptionElement.Value, false, $"Script: {Path.GetFileName(file)}");
            }
        }

        [TestMethod]
        public void VerifyScriptNames()
        {
            foreach (string file in GetScriptFiles())
            {
                XDocument document;
                try
                {
                    document = XDocument.Load(file);
                }
                catch (Exception)
                {
                    continue;
                }

                if (!document.Root.Name.LocalName.Equals("DMSScript")) continue; // Skip non script files

                var nameElement = document.Root.Elements().FirstOrDefault(x => String.Equals(x.Name.LocalName, "Name"));
                Assert.IsNotNull(nameElement, $"Script: {file}");

                if (nameElement.Value.StartsWith("PLS")) continue;
                Assert.IsFalse(nameElement.Value.Contains('_'), $"Script: {Path.GetFileName(file)}");
            }
        }

        private string[] GetScriptFiles()
        {
            string mediaServicesDirectory = GetMediaServicesDirectory();
            return Directory.EnumerateFiles(mediaServicesDirectory, "*.xml", SearchOption.AllDirectories).ToArray();
        }

        private string GetMediaServicesDirectory()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string[] splitCurrentDirectory = currentDirectory.Split(Path.DirectorySeparatorChar);
            int mediaServicesIndex = Array.IndexOf(splitCurrentDirectory, splitCurrentDirectory.FirstOrDefault(x => x.Contains("Media") && x.Contains("Services")));
            string[] splitMediaServicesDirectory = new string[mediaServicesIndex + 1];
            Array.Copy(splitCurrentDirectory, splitMediaServicesDirectory, mediaServicesIndex + 1);
            string mediaServicesFolder = String.Join(Path.DirectorySeparatorChar.ToString(), splitMediaServicesDirectory);

            return mediaServicesFolder;
        }
    }
}
