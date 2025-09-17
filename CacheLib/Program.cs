using System.Text;
using CacheLib;

var cachePath = "cache";
string outputPath = "output_cache";

ItemEditor editor = new ItemEditor();

// try
// {
//     // Use a single IndexManager instance for all operations
//     using var idxManager = new IndexManager();
//     idxManager.Initialize(cachePath);
//     
//     // Example 1: Read and display index information
//     Console.WriteLine("Reading index files...");
//     ReadIndexFiles(idxManager);
//     
//     // Example 2: Reconstruct complete cache
//     Console.WriteLine("\nReconstructing cache files...");
//     ReconstructCache(cachePath, outputPath, idxManager);
//     
//     // Example 3: Verify reconstruction
//     Console.WriteLine("\nVerifying reconstruction...");
//     VerifyReconstruction(cachePath, outputPath);
//     
//     Console.WriteLine("All operations completed successfully!");
// }
// catch (Exception ex)
// {
//     Console.WriteLine($"Error: {ex.Message}");
//     Console.WriteLine($"Stack Trace: {ex.StackTrace}");
// }
//
// static void ReadIndexFiles(IndexManager idxManager)
// {
//     for (int x = 0; x < 5; x++)
//     {
//         var count = idxManager.GetEntryCount(x);
//         Console.WriteLine($"Idx = {x}, Entry Count = {count} (0 - {count - 1})");
//
//         var entries = idxManager.GetEntries(x);
//         for (int y = 0; y < Math.Min(5, entries.Count); y++) // Show first 5 only
//         {
//             Console.WriteLine($"[{y}] Size: {entries[y].Size} - Start Block: {entries[y].StartBlock}");
//         }
//         if (entries.Count > 5)
//             Console.WriteLine($"... and {entries.Count - 5} more entries");
//     }
// }
//
// static void ReconstructCache(string cachePath, string outputPath, IndexManager idxManager)
// {
//     using var reconstructor = new DatFileReconstructor();
//     reconstructor.Initialize(cachePath, idxManager);
//     reconstructor.ReconstructComplete(outputPath);
// }
//
// static void VerifyReconstruction(string originalPath, string reconstructedPath)
// {
//     try
//     {
//         // Compare file sizes
//         var originalDat = new FileInfo(Path.Combine(originalPath, CacheConstants.DataFile));
//         var reconstructedDat = new FileInfo(Path.Combine(reconstructedPath, CacheConstants.DataFile));
//         
//         Console.WriteLine($"Original DAT size: {originalDat.Length} bytes");
//         Console.WriteLine($"Reconstructed DAT size: {reconstructedDat.Length} bytes");
//         Console.WriteLine($"DAT size match: {originalDat.Length == reconstructedDat.Length}");
//         
//         // Compare index files
//         for (int i = 0; i < 5; i++)
//         {
//             var originalIdx = new FileInfo(Path.Combine(originalPath, 
//                 string.Format(CacheConstants.IndexFilePattern, i)));
//             var reconstructedIdx = new FileInfo(Path.Combine(reconstructedPath, 
//                 string.Format(CacheConstants.IndexFilePattern, i)));
//             
//             if (originalIdx.Exists && reconstructedIdx.Exists)
//             {
//                 Console.WriteLine($"Idx{i} size match: {originalIdx.Length == reconstructedIdx.Length}");
//             }
//             else
//             {
//                 Console.WriteLine($"Idx{i}: One or both files don't exist");
//             }
//         }
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Verification error: {ex.Message}");
//     }
// }