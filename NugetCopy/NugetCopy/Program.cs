using NugetCopy.Common;
using NugetCopy.Service;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NugetCopy
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

            bool packsToUpdate = PacksToUpdateInput();

            foreach (var targetPath in CsprojFiles)
            {            
                Console.WriteLine("Updatig file: {0}", Path.GetFileName(targetPath));

                NugetCopyService.UpdatePackageVersions(sourcePath, targetPath, packsToUpdate);

                Console.WriteLine();
            }

            Console.WriteLine("Update of files complete.");
        }

        private static bool PacksToUpdateInput()
        {
            Console.WriteLine("Do you wish to update only packages starting with 'GP.', 'Baasic.' and 'MonoSoftware.'?");
            Console.Write("If yes type 'true' if not type 'false' to update all packages: ");
            bool input = Validation.SubmitBool(Console.ReadLine());
            Console.WriteLine();
            return input;
        }
    }
}
