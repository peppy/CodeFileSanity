using System;

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
