namespace CacheLib;

public class Printer
{
    public static void PrintArchive(Dictionary<int, ArchiveFile>  files)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"Archive contains {files.Count} files:");
        Console.ResetColor();
    
        Console.WriteLine("┌──────────────────────────┬─────────────┬────────────┬─────────────┐");
        Console.WriteLine("│ Filename                 │ ID          │ Compressed │ Decompressed│");
        Console.WriteLine("├──────────────────────────┼─────────────┼────────────┼─────────────┤");

        foreach (var (id, file) in files.OrderBy(x => x.Key))
        {
            string filename = EntryDictionary.Lookup(id).PadRight(24);
            string compressed = StringUtil.FormatSize(file.CompressedSize).PadLeft(10);
            string decompressed = StringUtil.FormatSize(file.DecompressedSize).PadLeft(10);
        
            Console.WriteLine($"│ {filename} │ {id,-11} │ {compressed} │ {decompressed}  │");
        }

        Console.WriteLine("└──────────────────────────┴─────────────┴────────────┴─────────────┘");
    
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"\nCompression Summary:");
        Console.ResetColor();
        Console.WriteLine($"Total compressed size:   {StringUtil.FormatSize(files.Sum(f => f.Value.CompressedSize))}");
        Console.WriteLine($"Total decompressed size: {StringUtil.FormatSize(files.Sum(f => f.Value.DecompressedSize))}");
        Console.WriteLine($"Compression ratio:       {files.Sum(f => f.Value.CompressedSize) * 100.0 / files.Sum(f => f.Value.DecompressedSize):F2}%");
}
    
    public static void PrintFileInfo(Dictionary<int, ArchiveFile>  files, int fileId)
    {
        if (files.TryGetValue(fileId, out var file))
        {
            Console.WriteLine($"File: {EntryDictionary.Lookup(fileId)} (ID: {fileId})");
            Console.WriteLine($"Compressed size: {file.CompressedSize} bytes");
            Console.WriteLine($"Decompressed size: {file.DecompressedSize} bytes");
            Console.WriteLine($"Ratio: {file.CompressedSize/(double)file.DecompressedSize*100:F2}%");
        }
        else
        {
            Console.WriteLine($"File with ID {fileId} not found in archive.");
        }
    }
}