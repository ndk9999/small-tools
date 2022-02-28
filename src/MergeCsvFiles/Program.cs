//var arguments = Environment.GetCommandLineArgs();

using System.Diagnostics;

var folderPath = args[0];
var fileExt = args[1];
var resultFileName = args[2];
var noHeaderRow = args.Length > 3 ? bool.Parse(args[3]) : false;

System.Console.WriteLine("Source folder path   : {0}", folderPath);
System.Console.WriteLine("Source file extension: {0}", fileExt);
System.Console.WriteLine("Result file name     : {0}", resultFileName);

// Get list of CSV files in source folder
var listFiles = Directory.EnumerateFiles(folderPath, $"*.{fileExt}");

// Print number of CSV files in source folder
System.Console.WriteLine("Found {0} files", listFiles.Count());

// Merge files if any
if (listFiles.Any())
{
    using (var writer = File.CreateText($"{folderPath}\\{resultFileName}"))
    {
        var writeHeaderRow = true;

        foreach (var path in listFiles)
        {
            using (var reader = new StreamReader(File.OpenRead(path)))
            {
                var content = reader.ReadLine();

                if (writeHeaderRow || noHeaderRow)
                {
                    writer.WriteLine(content);
                    writeHeaderRow = false;
                }

                while (!reader.EndOfStream)
                {
                    content = reader.ReadLine();

                    if (!string.IsNullOrWhiteSpace(content))
                        writer.WriteLine(content);
                }
            }

            writer.Flush();
        }
    }
}

System.Console.WriteLine("DONE !!!");