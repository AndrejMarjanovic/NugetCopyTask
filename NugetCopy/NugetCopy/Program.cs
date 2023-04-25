using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NuGetVersionUpdater
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("NuGetCopy Task: ");

            string file1Path = @"C:\Users\Andrej\NugetCopyTask\NugetCopy\GP.WebAPI.csproj";
            string file2Path = @"C:\Users\Andrej\NugetCopyTask\NugetCopy\GP.Baasic.Module.Neccton.WindowsService.Host2.csproj";

            if (!File.Exists(file1Path))
            {
                Console.WriteLine($"File not found: {file1Path}");
                return;
            }

            if (!File.Exists(file2Path))
            {
                Console.WriteLine($"File not found: {file2Path}");
                return;
            }

            UpdatePackageVersions(file1Path, file2Path);

            Console.WriteLine("Update complete.");
        }

        static void UpdatePackageVersions(string sourceFile, string targetFile)
        {
            var sourceDoc = XDocument.Load(sourceFile);
            var targetDoc = XDocument.Load(targetFile);

            //foreach (XElement element in sourceDoc.Descendants("PackageReference"))
            //{
            //    Console.WriteLine(element.Element);
            //}

            //Console.WriteLine(targetDoc);
            //Console.Writeline(sourceDoc):

            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            var sourcePackages = sourceDoc.Descendants(ns + "PackageReference").Select(p => new { Id = p.Attribute("Include")?.Value, Version = GetPackageVersion(p) });

            foreach (var package in sourcePackages)
            {
                var targetPackage = targetDoc.Descendants("PackageReference").FirstOrDefault(p => p.Attribute("Include")?.Value == package.Id);

                if (targetPackage != null)
                {
                    var currentVersion = GetPackageVersion(targetPackage);
                    if (currentVersion != null && currentVersion != package.Version)
                    {
                        SetPackageVersion(targetPackage, package.Version);
                        Console.WriteLine($"Updated version of package {package.Id} TO ------->> {package.Version}");
                    }
                }
                else
                {
                    Console.WriteLine($"Package {package.Id} not found in {targetFile}");
                }
            }

            targetDoc.Save(targetFile);
        }

        static string GetPackageVersion(XElement packageReference)
        {

            var versionAttribute = packageReference.Attributes()
                .FirstOrDefault(x => string.Equals(x.Name.LocalName, "version", StringComparison.OrdinalIgnoreCase));

            if (versionAttribute != null)
            {
                return versionAttribute.Value;
            }

            var versionElement = packageReference.Element("Version");

            if (versionElement != null)
            {
                return versionElement.Value;
            }

            return null;
        }


        static void SetPackageVersion(XElement packageReference, string version)
        {
            var versionElement = packageReference.Element("Version");
            if (versionElement != null)
            {
                versionElement.Value = version;
            }
            else
            {
                XAttribute versionAttribute = packageReference.Attribute("Version") ?? packageReference.Attribute("version");
                versionAttribute.Value = version;
            }
        }
    }
}
