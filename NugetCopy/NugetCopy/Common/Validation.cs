using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetCopy.Common
{
    public class Validation
    {
        public static bool SubmitBool(string s)
        {
            bool isBool;
            if (!bool.TryParse(s, out isBool))
            {
                do
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Please enter (true/false):");
                    Console.ResetColor();
                    s = Console.ReadLine();
                }
                while (!bool.TryParse(s, out isBool));
            }

            return isBool;
        }
    }
}
