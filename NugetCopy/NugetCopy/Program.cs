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

            string file1Path = @"..\..\..\..\GP.WebAPI.csproj";
            string file2Path = @"..\..\..\..\GP.Baasic.Module.Neccton.WindowsService.Host.csproj";

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
            XDocument sourceDoc = XDocument.Load(sourceFile);
            XDocument targetDoc = XDocument.Load(targetFile);
            
            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

            var sourcePackages = sourceDoc.Descendants("PackageReference").Select(p => new { Id = p.Attribute("Include")?.Value, Version = GetPackageVersion(p) });

            if (!sourcePackages.Any())
            {
                sourcePackages = sourceDoc.Descendants(ns + "PackageReference").Select(p => new { Id = p.Attribute("Include")?.Value, Version = GetPackageVersion(p) });
            }

            foreach (var package in sourcePackages)
            {
                var targetPackage = targetDoc.Descendants("PackageReference").FirstOrDefault(p => p.Attribute("Include")?.Value == package.Id);

                if (targetPackage == null)
                {
                    targetPackage = targetDoc.Descendants(ns + "PackageReference").FirstOrDefault(p => p.Attribute("Include")?.Value == package.Id);
                }

                if (targetPackage != null)
                {
                    var currentVersion = GetPackageVersion(targetPackage);
                    if (currentVersion != package.Version)
                    {
                        SetPackageVersion(targetPackage, package.Version);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Updated version of package {package.Id} TO ------->> {package.Version}");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.WriteLine($"Package {package.Id} NOT FOUND IN TARGETFILE");
                }
            }

            targetDoc.Save(targetFile);
        }

        static string GetPackageVersion(XElement packageReference)
        {
            XElement versionElement = packageReference.Descendants().FirstOrDefault(e => e.Name.LocalName == "Version");
            if (versionElement != null)
            {
                return versionElement.Value;
            }
            else
            {
                XAttribute versionAttribute = packageReference.Attribute("Version") ?? packageReference.Attribute("version");
                if (versionAttribute != null)
                {
                    return versionAttribute.Value;
                }
                else
                {
                    return null;
                }
            }
        }


        static void SetPackageVersion(XElement packageReference, string version)
        {
            XElement versionElement = packageReference.Descendants().FirstOrDefault(e => e.Name.LocalName == "Version");
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
