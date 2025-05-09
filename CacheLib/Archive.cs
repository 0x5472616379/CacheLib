namespace CacheLib;

public class Archive
{
    private readonly Dictionary<int, ArchiveFile> _files = new();
    public Dictionary<int, ArchiveFile> GetFiles() => _files;
    public static Archive Load(byte[] archiveData)
    {
        var archive = new Archive();
        if (archiveData == null || archiveData.Length < 6) return archive;

        try
        {
            using var ms = new MemoryStream(archiveData);
            using var br = new BinaryReader(ms);

            // Read compression header (6 bytes)
            int decompressedSize = br.ReadUnsignedMedium();
            int compressedSize = br.ReadUnsignedMedium();

            byte[] data;
            
            // First decompression handles the archive container compression
            if (decompressedSize != compressedSize)
            {
                // Handle compressed data
                byte[] compressed = br.ReadBytes(compressedSize);
                data = BZip2Helper.Decompress(compressed);

                if (data.Length != decompressedSize)
                {
                    Console.WriteLine($"Warning: Decompressed size mismatch. Expected: {decompressedSize}, Got: {data.Length}");
                }
            }
            else
            {
                // Uncompressed data
                data = br.ReadBytes(decompressedSize);
            }

            return ParseArchiveData(data, archive);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading archive: {ex.Message}");
            return archive;
        }
    }

    private static Archive ParseArchiveData(byte[] data, Archive archive)
    {
        if (data == null || data.Length == 0) return archive;

        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        try
        {
            int fileCount = br.ReadUInt16BigEndian();
            Console.WriteLine($"Found {fileCount} files in archive");

            // Read ALL index entries first
            var entries = new List<(int id, int decompressedSize, int compressedSize, bool isCompressed)>();
            
            for (int i = 0; i < fileCount; i++)
            {
                int id = br.ReadInt32BigEndian();
                int decompressedSize = br.ReadUnsignedMedium();
                int compressedSize = br.ReadUnsignedMedium();
                bool isCompressed = decompressedSize != compressedSize;
                
                entries.Add((id, decompressedSize, compressedSize, isCompressed));
            }

            // Read file data
            foreach (var (id, decompressedSize, compressedSize, isCompressed) in entries)
            {
                try
                {
                    byte[] fileData = br.ReadBytes(compressedSize);

                    // Second decompression handles individual file compression within the archive
                    if (isCompressed)
                    {
                        try
                        {
                            fileData = BZip2Helper.Decompress(fileData);
                            if (fileData.Length != decompressedSize)
                            {
                                Console.WriteLine($"Size mismatch for {id}: Expected {decompressedSize}, got {fileData.Length}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Decompression failed for {id}: {ex.Message}");
                            continue;
                        }
                    }

                    archive._files[id] = new ArchiveFile(id, fileData, compressedSize, decompressedSize);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to read file {id}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing archive data: {ex.Message}");
        }

        return archive;
    }
    
    public static int Hash(string name)
    {
        return name.ToUpper().ToCharArray().Aggregate(0, (hash, character) => hash * 61 + character - 32);
    }
}