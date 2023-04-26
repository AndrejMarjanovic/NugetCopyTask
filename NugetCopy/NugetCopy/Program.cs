using NugetCopy.Service;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
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

            string sourcePath = @"..\..\..\..\GP.WebAPI.csproj";

            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"File not found: {sourcePath}");
                return;
            }

            var CsprojFiles = NugetCopyService.GetCsprojFiles(@"..\..\..\..\Solution");

            Console.WriteLine();
            Console.WriteLine("Total files to update: {0}", CsprojFiles.Count);
            Console.WriteLine();

            foreach (var targetPath in CsprojFiles)
            {            
                Console.WriteLine("Updatig file at path: {0}", targetPath);

                NugetCopyService.UpdatePackageVersions(sourcePath, targetPath);

                Console.WriteLine();
            }

            Console.WriteLine("Update of files complete.");
        }
    }
}
