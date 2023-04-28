using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using NugetCopy.Common;

namespace NugetCopy.Service
{
    public class NugetCopyService
    {
        public static List<string> GetCsprojFiles(string rootDirectory)
        {
            var csProjFiles = new List<string>();

            foreach (var file in Directory.GetFiles(rootDirectory, "*.csproj", SearchOption.AllDirectories))
            {
                csProjFiles.Add(file);
            }

            return csProjFiles;
        }

        public static void UpdatePackageVersions(string sourceFile, string targetFile, bool packsToUpdate)
        {
            XDocument sourceDoc = XDocument.Load(sourceFile);
            XDocument targetDoc = XDocument.Load(targetFile, LoadOptions.PreserveWhitespace);

            int counter = 0;

            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

            var sourcePackages = sourceDoc.Descendants("PackageReference").Select(p => new { Id = p.Attribute("Include")?.Value, Version = GetPackageVersion(p) })
                         .Concat(sourceDoc.Descendants(ns + "PackageReference").Select(p => new { Id = p.Attribute("Include")?.Value, Version = GetPackageVersion(p) }));

            foreach (var package in sourcePackages)
            {
                IEnumerable<XElement> targetPackages = targetDoc.Descendants("PackageReference").Where(x => x.Attribute("Include")?.Value == package.Id)
                                               .Concat(targetDoc.Descendants(ns + "PackageReference").Where(p => p.Attribute("Include")?.Value == package.Id));

                if (packsToUpdate)
                {
                    targetPackages = FilterPackages(targetPackages);
                }

                if (targetPackages.Any())
                {
                    foreach (var targetPackage in targetPackages)
                    {
                        var currentVersion = GetPackageVersion(targetPackage);
                        if (currentVersion != package.Version)
                        {
                            SetPackageVersion(targetPackage, package.Version);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Updated version of package {package.Id} FROM: {currentVersion} TO ------->> {package.Version}");
                            Console.ResetColor();
                            counter += 1;
                        }
                    }
                }
            }

            Console.WriteLine("NUMBER OF UPDATES: {0}", counter);

            if (counter > 0)
                SaveFile(targetDoc, targetFile);
        }

        static IEnumerable<XElement> FilterPackages (IEnumerable<XElement> elements)
        {
            elements = elements.Where(e => e.Attribute("Include").Value.StartsWith("GP.") || 
                                           e.Attribute("Include").Value.StartsWith("Baasic.") ||
                                           e.Attribute("Include").Value.StartsWith("MonoSoftware."));
            
            return elements;
        }

        static void SaveFile(XDocument targetDoc, string path)
        {
            var stringEncoding = targetDoc.Declaration?.Encoding;

            XmlWriterSettings xws = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true, NewLineChars = "\r\n" };
            using (XmlWriter xw = XmlWriter.Create(path, stringEncoding == null ? xws : null))
                targetDoc.Save(xw);
        }


        static string GetPackageVersion(XElement packageReference)
        {
            return packageReference.Descendants().FirstOrDefault(e => e.Name.LocalName == "Version")?.Value
                   ?? (packageReference.Attribute("Version") ?? packageReference.Attribute("version"))?.Value;
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
