using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenMakaiRanch.Tools
{
    /// <summary>
    /// Simple content validation command used in CI.
    /// Checks for unique IDs, missing references and image existence.
    /// This is a placeholder implementation – extend as needed.
    /// </summary>
    public static class ContentValidator
    {
        public static int Main(string[] args)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", ".."));
            Console.WriteLine($"Running content validation in {projectRoot}");

            // Example: validate .tres files in resources folder
            var tresFiles = Directory.GetFiles(Path.Combine(projectRoot, "resources"), "*.tres", SearchOption.AllDirectories);
            var ids = new HashSet<string>();
            var duplicateIds = new List<string>();
            foreach (var file in tresFiles)
            {
                // Very naive: look for a line like "id = \"some_id\""
                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    if (line.TrimStart().StartsWith("id = \""))
                    {
                        var id = line.Split('"')[1];
                        if (!ids.Add(id))
                        {
                            duplicateIds.Add(id);
                        }
                    }
                }
            }

            if (duplicateIds.Any())
            {
                Console.Error.WriteLine("Duplicate IDs found:");
                foreach (var dup in duplicateIds.Distinct())
                {
                    Console.Error.WriteLine($"  {dup}");
                }
                return 1;
            }

            Console.WriteLine("No duplicate IDs detected.");
            // Additional checks (reference existence, image paths) can be added here.
            return 0;
        }
    }
}