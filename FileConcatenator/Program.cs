﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileConcatenator
{
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                return;
            }
            
            var destinationFile = $"{args[0]}/Combined.cs";
            
            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }

            var filesToParse = GetSourceFileNames(args[0]).ToList();

            var projectNamespace = GetNameSpace(filesToParse[0]);
            var usings = new List<string>();
            var classes = new List<string>();

            foreach (var fileToParse in filesToParse)
            {
                var (fileUsings, fileContents) = GetClassContents(fileToParse);
                
                usings.AddRange(fileUsings);
                classes.Add(fileContents);
            }

            usings = usings.Distinct().ToList();

            CreateFile(destinationFile, usings, projectNamespace, classes);
        }
        
        private static IEnumerable<string> GetSourceFileNames(string solutionFilePath)
        {
            var files = Directory.GetFiles(solutionFilePath, "*.cs", SearchOption.AllDirectories);

            return files.Where(f => !f.Contains("\\obj\\")); // Exclude anything in obj
        }

        private static string GetNameSpace(string file)
        {
            var text = File.ReadAllText(file);

            var startLocation = text.IndexOf("namespace", StringComparison.Ordinal) + 10;

            var endLocation = text.IndexOf("\r\n", startLocation, StringComparison.Ordinal);

            return text.Substring(startLocation, endLocation-startLocation);
        }

        private static (List<string> usings, string contents) GetClassContents(string fileToParse)
        {
            var usings = new List<string>();
            var contents = string.Empty;

            var text = File.ReadAllText(fileToParse);

            var index = 0;
            var found = true;

            while (found)
            {
                var usingStartIndex = text.IndexOf("using", index, StringComparison.Ordinal);

                if (usingStartIndex == -1)
                {
                    found = false;
                    continue;
                }
                var usingEndIndex = text.IndexOf(";", usingStartIndex, StringComparison.Ordinal);
                
                usings.Add(text.Substring(usingStartIndex, usingEndIndex-usingStartIndex+1));

                index = usingEndIndex;
            }

            var contentStart = text.IndexOf("{", StringComparison.Ordinal);

            var lastClosingBraceLocation = 0;
            index = 0;

            while (true)
            {
                var closingBraceLocation = text.IndexOf("}", index, StringComparison.Ordinal);

                if (closingBraceLocation != -1)
                {
                    lastClosingBraceLocation = closingBraceLocation;
                }
                else
                {
                    break;
                }

                index = closingBraceLocation + 1;
            }

            contents = text.Substring(contentStart+1, lastClosingBraceLocation - contentStart-1);

            return (usings, contents);
        }
        
        private static void CreateFile(string destinationFile, List<string> usings, string projectNamespace, List<string> classes)
        {
            File.Create(destinationFile).Close();

            using var textWriter = new StreamWriter(destinationFile);
            
            // Add usings
            foreach (var classUsing in usings)
            {
                textWriter.WriteLine(classUsing);
            }
            
            // Add namespace
            textWriter.WriteLine("");
            textWriter.WriteLine($"namespace {projectNamespace}Combined");
            textWriter.Write("{");
            
            foreach (var projectClass in classes)
            {
                textWriter.WriteLine(projectClass);
            }
            
            textWriter.WriteLine("}");
            
        }
    }
}