using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ThemeFileModifier
{
    internal class Program
    {
        static string appDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        static void Main(string[] args)
        {
            var userConfig = new IniFile(Path.Combine(appDirectory,"changes.ini"));            
            string currentDir = Environment.CurrentDirectory;
            string[] themeFiles = Directory.GetFiles(currentDir, "*.theme");
            string outputDir = Path.Combine(currentDir, "output");
            Directory.CreateDirectory(outputDir);

            if (themeFiles.Length == 0)
            {
                Console.WriteLine("ERROR: No .theme files found, any key to exit");
                Console.ReadKey();
                return;
            }

            foreach (string file in themeFiles)
            {
                string outputFilePath = Path.Combine(outputDir,Path.GetFileName(file));                                
                File.Copy(file, outputFilePath, true);                
                string fileName = Path.GetFileName(outputFilePath).ToLower();
                var themeFile = new IniFile(outputFilePath);

                if (userConfig.GetSectionNames() == null)
                {
                    Console.WriteLine("ERROR: NO SECTIONS FOUND");
                    Console.ReadKey();
                    return;
                } 

                foreach (var section in userConfig.GetSectionNames())
                {

                    Console.WriteLine($"{section} is being parsed");

                    if (section.StartsWith("-"))
                    {
                        Console.WriteLine($"{section.Substring(1)} gets deleted");
                        themeFile.DeleteSection(section.Substring(1));
                        continue;
                    }                    

                    foreach (string item2 in userConfig.GetSectionData(section))
                    {
                        string pattern = @"^(\w+)\s*=\s*(.*)$";

                        MatchCollection matches = Regex.Matches(item2, pattern, RegexOptions.Multiline);                        

                        foreach (Match match in matches)
                        {
                            string key = match.Groups[1].Value;
                            string value = match.Groups[2].Value;

                            themeFile.Write(key, value, section);

                            Console.WriteLine($"found match, modifying: {key}={value}");
                        }
                    }
                }

                var wallPath = themeFile.Read("Wallpaper", "Control Panel\\Desktop");

                bool darkMode = false;

                if (fileName.Contains("dark") || fileName.Contains("night"))
                {
                    darkMode = true;
                }

                // Regular expression to match the folder name after "Themes"
                string folderNamePattern = @"Resources\\Themes\\([^\\/]+)\\";                               

                // Find the match using Regex.folderNamePattern
                Match folderNameMatch = Regex.Match(wallPath, folderNamePattern);

                if (folderNameMatch.Success)
                {
                    string folderName = folderNameMatch.Groups[1].Value;
                    Console.WriteLine("Folder name: " + folderName);
                    string darkOrLight = darkMode ? "dark" : "light";

                    themeFile.Write("Wallpaper", $"%SystemRoot%\\Resources\\Themes\\{folderName}\\{darkOrLight}.jpg", "Control Panel\\Desktop");
                }
                else
                {
                    Console.WriteLine("Folder name not found.");
                }
            }
            Console.WriteLine("ALL DONE. ANY KEY TO EXIT");
            Console.ReadLine();
        }

    }
}
