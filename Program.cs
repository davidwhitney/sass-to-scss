using System;
using System.Diagnostics;
using System.IO;

namespace sass_to_scss
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var searchRoot = args[0];

            var allSassFiles = Directory.GetFiles(searchRoot, "*.sass", SearchOption.AllDirectories);

            foreach (var file in allSassFiles)
            {
                var scssFile = ConvertToScss(file);
                PatchInternalReferences(scssFile);
                CorrectBadlyConstructedComments(scssFile);
                File.Delete(file);
            }

            Console.WriteLine("Done. Press ANY key to exit");
            Console.ReadLine();
        }

        private static void CorrectBadlyConstructedComments(string file)
        {
            var lines = File.ReadAllLines(file);
            var requiresUpdate = false;
            
            for (var index = 0; index < lines.Length; index++)
            {
                var line = lines[index];
                var trimmed = line.Trim();

                if (trimmed.Contains("//") && trimmed.EndsWith("{"))
                {
                    line = line.Replace("//", " { //");
                    line = line.TrimEnd('{');
                    lines[index] = line;
                    requiresUpdate = true;
                }
            }

            if (requiresUpdate)
            {
                File.WriteAllLines(file, lines);
                Console.WriteLine("Patched { comment endings" + file);
            }
        }

        private static void PatchInternalReferences(string file)
        {
            var scssFile = file.Replace(".sass", ".scss");
            var contents = File.ReadAllText(file);
            contents = contents.Replace(".sass", ".scss");
            File.WriteAllText(scssFile, contents);
            Console.WriteLine("Patched " + scssFile);
        }

        private static string ConvertToScss(string file)
        {
            var extensionless = file.Replace(".sass", "");
            var param = string.Format("/C sass-convert --from sass --to scss {0}.sass {0}.scss", extensionless);
            var info = new ProcessStartInfo("cmd", param) {UseShellExecute = false, CreateNoWindow = true};
            var process = Process.Start(info);
            process.WaitForExit();

            Console.WriteLine("Converted " + extensionless);

            return extensionless + ".scss";
        }
    }
}
