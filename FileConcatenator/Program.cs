﻿namespace FileConcatenator;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal sealed class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            return;
        }

        string destinationFile = $"{args[0]}/Combined.cs";
            
        if (File.Exists(destinationFile))
        {
            File.Delete(destinationFile);
        }

        List<string> filesToParse = GetSourceFileNames(args[0]).ToList();

        List<string> usings = new List<string>();
        List<string> classes = new List<string>();

        foreach (string fileToParse in filesToParse)
        {
            if (Path.GetFileName(fileToParse) == "AssemblyInfo.cs")
                continue;

            (List<string> fileUsings, string fileContents) = GetClassContents(fileToParse);
                
            usings.AddRange(fileUsings);
            classes.Add(fileContents);
        }

        usings = usings.Distinct().ToList();

        CreateFile(destinationFile, usings, classes);
    }
        
    private static IEnumerable<string> GetSourceFileNames(string solutionFilePath)
    {
        string[] files = Directory.GetFiles(solutionFilePath, "*.cs", SearchOption.AllDirectories);

        return files.Where(f => !f.Contains("\\obj\\")); // Exclude anything in obj
    }

    private static string GetNameSpace(string file)
    {
        string text = File.ReadAllText(file);

        int startLocation = text.IndexOf("namespace", StringComparison.Ordinal) + 10;

        int endLocation = text.IndexOf("\r\n", startLocation, StringComparison.Ordinal);

        return text.Substring(startLocation, endLocation-startLocation);
    }

    private static (List<string> usings, string content) GetClassContents(string fileToParse)
    {
        List<string> usings = new List<string>();

        string text = File.ReadAllText(fileToParse);

        int blockIndex = text.IndexOf("class");
        if (blockIndex == -1)
        {
            blockIndex = text.IndexOf("enum");
        }
        if (blockIndex == -1)
        {
            blockIndex = text.IndexOf("struct");
        }
        if (blockIndex == -1)
        {
            blockIndex = text.IndexOf("interface");
        }

        int blockStart = text.Substring(0, blockIndex).LastIndexOf("\n");

        string blockContent = text.Substring(blockStart + 1);

        // Get usings
        int index = 0;
        bool usingsFinished = false;

        while (!usingsFinished)
        {
            int usingStartIndex = text.Substring(0, blockStart).IndexOf("using", index, StringComparison.Ordinal);

            if (usingStartIndex == -1)
            {
                usingsFinished = true;
                continue;
            }
            int usingEndIndex = text.IndexOf(";", usingStartIndex, StringComparison.Ordinal);
            index = usingEndIndex;

            usings.Add(text.Substring(usingStartIndex, usingEndIndex - usingStartIndex + 1));
        }

        return (usings, blockContent);
    }
        
    private static void CreateFile(string destinationFile, List<string> usings, List<string> classes)
    {
        File.Create(destinationFile).Close();

        using StreamWriter textWriter = new StreamWriter(destinationFile);
            
        // Add comments
        textWriter.WriteLine("/**************************************************************");
        textWriter.WriteLine("  This file was generated by FileConcatenator.");
        textWriter.WriteLine("  It combined all classes in the project to work in Codingame.");
        textWriter.WriteLine("***************************************************************/");
        textWriter.WriteLine();

        // Add usings
        foreach (string classUsing in usings)
        {
            textWriter.WriteLine(classUsing);
        }

        textWriter.WriteLine();

        foreach (string projectClass in classes)
        {
            textWriter.Write(projectClass);
            textWriter.WriteLine();
            textWriter.WriteLine();
        }
            
    }
}

