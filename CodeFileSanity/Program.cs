using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeFileSanity
{
    class Program
    {
        static void Main(string[] args)
        {
            CodeSanityValidator.hasAppveyor = Environment.GetEnvironmentVariable("APPVEYOR")?.ToLower().Equals("true") ?? false;

            CodeSanityValidator.checkDirectory(".");

            if (CodeSanityValidator.hasErrors)
                Environment.ExitCode = -1;
        }
    }
}
