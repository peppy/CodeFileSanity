using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeFileSanity
{
    public class CodeSanityValidator
    {

        private readonly string[] ignore_paths = {
            ".git",
            "bin",
            "obj",
            "migrations",
            "packages"
        };

        public bool HasErrors { get; private set; }

        private readonly bool isAppveyorBuild;

        private readonly string rootDirectory;

        public CodeSanityValidator(ValidateCodeSanitySettings settings) 
        {
            isAppveyorBuild = settings.IsAppveyorBuild;
            rootDirectory = settings.RootDirectory;
        }

        public void Validate() 
            => checkDirectory(rootDirectory);

        private string getLicenseHeader(string path)
        {
            string filename;

            while (true)
            {
                filename = Directory.GetFiles(path, "*.licenseheader").FirstOrDefault();
                if (filename != null) break;

                path = Directory.GetParent(path)?.FullName;
                if (path == null) return null;
            }

            bool started = false;
            string licenseHeader = string.Empty;

            foreach (string s in File.ReadAllLines(filename))
            {
                if (started)
                {
                    if (!s.StartsWith("//"))
                        break;
                    licenseHeader += s + "\r\n";
                }

                if (s == "extensions: .cs")
                {
                    started = true;
                }
            }

            return licenseHeader;
        }

        private void checkDirectory(string path)
        {
            if (ignore_paths.Contains(path.Split(Path.DirectorySeparatorChar).Last().ToLower()))
                return;

            foreach (var sub in Directory.GetDirectories(path))
                checkDirectory(sub);

            var license = getLicenseHeader(path);

            foreach (var file in Directory.GetFiles(path, "*.cs"))
                checkFile(file, license);
        }

        private void checkFile(string file, string licenseHeader)
        {
            string text = File.ReadAllText(file);

            List<int> lines = new List<int>();

            if ((lines = findMatchingLines(text, "\r[^\n].", RegexOptions.Multiline)).Count > 0)
            {
                report(file, $"Incorrect line endings", lines);
            }

            if (licenseHeader != null && !text.StartsWith(licenseHeader))
            {
                report(file, $"License header missing");
            }

            if ((lines = findMatchingLines(text, "^((?!///).)* \r\n", RegexOptions.Multiline)).Count > 0)
            {
                report(file, $"White space needs to be trimmed", lines);
            }

            if ((lines = findMatchingLines(text, "\t")).Count > 0)
            {
                report(file, $"Found tab character", lines);
            }

            if (Path.GetFileName(file) == "AssemblyInfo.cs")
                return;

            if (findMatchingLines(text, $"(enum|struct|class|interface) {Path.GetFileNameWithoutExtension(file).Split('_').First()}").Count == 0)
            {
                report(file, $"Filename does not match contained type.");
            }
        }

        private List<int> findMatchingLines(string input, string pattern, RegexOptions options = RegexOptions.None)
        {
            MatchCollection matches = Regex.Matches(input, pattern, options);
            List<int> toReturn = new List<int>();
            foreach (Match match in matches)
            {
                toReturn.Add(getLineNumber(input, match.Index));
            }
            return toReturn;
        }

        private int getLineNumber(string input, int index) => input.Remove(index).Count(c => c == '\n') + 1;

        private void report(string filename, string message, List<int> lines) => lines.ForEach((line) => report(filename, message, line));

        private void report(string filename, string message, int line = 0)
        {
            Console.WriteLine($"{filename}:{line}: {message}");

            HasErrors = true;

            if (isAppveyorBuild)
                runAppveyor($"\"{message}\" -Category Error -FileName \"{filename.Substring(2)}\" -Line {line}");
        }

        private bool runAppveyor(string args)
        {
            if (string.IsNullOrEmpty(args))
                args = "\"\"";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "appveyor",
                    Arguments = $"AddCompilationMessage {args}"
                });

                return true;
            }
            catch
            {
                // we don't have appveyor and don't care
            }

            return false;
        }
    }
}