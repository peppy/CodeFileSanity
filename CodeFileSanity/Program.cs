using System;

namespace CodeFileSanity
{
    static class Program
    {
        static void Main()
        {
            var codeSanityValidator = new CodeSanityValidator(new ValidateCodeSanitySettings {
                IsAppveyorBuild = Environment.GetEnvironmentVariable("APPVEYOR")?.ToLower().Equals("true") ?? false,
                RootDirectory = "."
            });

            codeSanityValidator.Validate();

            if (codeSanityValidator.HasErrors)
                Environment.ExitCode = -1;
        }
    }
}
