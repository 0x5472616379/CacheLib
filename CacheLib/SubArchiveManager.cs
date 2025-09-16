// namespace CacheLib;
//
// public class SubArchiveManager
// {
//     string cachePath = "cache";
//     private Cache _cache = new();
//
//     public SubArchiveManager()
//     {
//         _cache.Load(cachePath);
//     }
//
//     public byte[] LoadArchive(ArchiveType archiveIndex, SubArchiveType archiveId)
//     {
//         var indexEntry = _cache.ReadIndexEntry((int)archiveIndex, (int)archiveId); //i.e: idx0, archiveId 2 (config)
//         var archiveBuffer = _cache.ReadFileBlocks(indexEntry);
//         return archiveBuffer;
//     }
//
//     public Dictionary<int, ArchiveFile> LoadArchiveFiles(byte[] archiveBuffer)
//     {
//         Dictionary<int, ArchiveFile> files = new();
//         var subArchive = new SubArchive(_cache.IsCompressed(archiveBuffer), archiveBuffer);
//
//         if (subArchive.IsCompressed)
//             subArchive.Data = BZip2Helper.Decompress(subArchive.Data.Skip(6).ToArray());
//
//         using var ms = new MemoryStream(subArchive.Data);
//         using var br = new BinaryReader(ms);
//
//         //Only do this if the subArchiveData was not compressed to begin with, if we've had to decompress it we don't need to check this. (not sure why it's like this)
//         if (!subArchive.IsCompressed)
//         {
//             int decompressedSize = br.ReadUnsignedMedium();
//             int compressedSize = br.ReadUnsignedMedium();
//
//             //Doesn't seem to ever hit this..?
//             if (decompressedSize != compressedSize)
//             {
//                 throw new Exception("Compressed size mismatch. Compress it (I think..)");
//             }
//
//             // byte[] data;
//             //
//             // // First decompression handles the archive container compression
//             // if (decompressedSize != compressedSize)
//             // {
//             //     // Handle compressed data
//             //     byte[] compressed = br.ReadBytes(compressedSize); //Has already skipped the first 6 bytes because the BR increments on read.
//             //     data = BZip2Helper.Decompress(compressed);
//             //     
//             //     if (data.Length != decompressedSize)
//             //     {
//             //         Console.WriteLine($"Warning: Decompressed size mismatch. Expected: {decompressedSize}, Got: {data.Length}");
//             //     }
//             // }
//             // else
//             // {
//             //     // Uncompressed data
//             //     data = br.ReadBytes(decompressedSize);
//             // }
//         }
//
//         int fileCount = br.ReadUInt16BigEndian();
//         Console.WriteLine($"Found {fileCount} files in archive");
//
//         var entries = new List<(int id, int decompressedSize, int compressedSize, bool isCompressed)>();
//         for (int i = 0; i < fileCount; i++)
//         {
//             int id = br.ReadInt32BigEndian();
//             int decompressedSize = br.ReadUnsignedMedium();
//             int compressedSize = br.ReadUnsignedMedium();
//             bool isCompressed = decompressedSize != compressedSize;
//
//             Console.WriteLine($"FileId: {i} - Compressed: {isCompressed}.");
//
//             entries.Add((id, decompressedSize, compressedSize, isCompressed));
//         }
//
//         foreach (var (id, decompressedSize, compressedSize, isCompressed) in entries)
//         {
//             try
//             {
//                 byte[] fileData = br.ReadBytes(compressedSize);
//
//                 // Second decompression handles individual file compression within the archive
//                 if (isCompressed)
//                 {
//                     try
//                     {
//                         fileData = BZip2Helper.Decompress(fileData);
//                         if (fileData.Length != decompressedSize)
//                         {
//                             Console.WriteLine($"Size mismatch for {id}: Expected {decompressedSize}, got {fileData.Length}");
//                         }
//                     }
//                     catch (Exception ex)
//                     {
//                         Console.WriteLine($"Decompression failed for {id}: {ex.Message}");
//                         continue;
//                     }
//                 }
//
//                 files[id] = new ArchiveFile(id, fileData, compressedSize, decompressedSize);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Failed to read file {id}: {ex.Message}");
//             }
//         }
//
//         return files;
//     }
// }
//
// public enum SubArchiveType
// {
//     Title = 1,
//     Config = 2,
//     Interface = 3,
//     Media = 4,
//     Versionlist = 5,
//     Texture = 6,
//     Wordenc = 7,
//     Sound = 8,
// }
//
// public class SubArchive(bool isIsCompressed, byte[] data)
// {
//     public bool IsCompressed { get; set; } = isIsCompressed;
//     public byte[] Data { get; set; } = data;
// }