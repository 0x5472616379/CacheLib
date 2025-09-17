namespace CacheLib;

public class ArchiveManager : IDisposable
{
    private IndexManager _indexManager;
    private FileStream _dataFile;
    private bool _disposed = false;
    private bool _ownsIndexManager;

    public List<ArchiveFile> Files { get; set; } = new();
    
    public void Initialize(string cacheDirectory, IndexManager indexManager = null)
    {
        _indexManager = indexManager ?? new IndexManager();
        _ownsIndexManager = indexManager == null;

        if (_ownsIndexManager)
        {
            _indexManager.Initialize(cacheDirectory);
        }

        string dataFilePath = Path.Combine(cacheDirectory, CacheConstants.DataFile);
        _dataFile = new FileStream(dataFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    }

    public List<byte[]> ReadArchiveFile(int idx)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ArchiveManager));

        var entries = _indexManager.GetEntries(idx);
        var idxFiles = new List<byte[]>();

        foreach (var entry in entries)
        {
            var fileBuffer = ReadFileBlocks(entry);
            idxFiles.Add(fileBuffer);
        }

        return idxFiles;
    }

    public byte[] ReadFileBlocks(IndexEntry entry)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ArchiveManager));

        byte[] output = new byte[entry.Size];
        int bytesRead = 0;
        int currentBlock = entry.StartBlock;

        while (bytesRead < entry.Size)
        {
            byte[] header = new byte[CacheConstants.HeaderSize];
            long blockPosition = currentBlock * CacheConstants.BlockSize;

            _dataFile.Seek(blockPosition, SeekOrigin.Begin);
            int headerBytesRead = _dataFile.Read(header, 0, header.Length);

            if (headerBytesRead != header.Length)
                throw new IOException($"Failed to read header at block {currentBlock}");

            int nextBlock = (header[4] << 16) | (header[5] << 8) | header[6];
            int remainingBytes = entry.Size - bytesRead;
            int bytesToRead = Math.Min(remainingBytes, CacheConstants.ChunkSize);

            int dataBytesRead = _dataFile.Read(output, bytesRead, bytesToRead);
            if (dataBytesRead != bytesToRead)
                throw new IOException($"Failed to read data at block {currentBlock}");

            bytesRead += bytesToRead;
            currentBlock = nextBlock;

            if (currentBlock == 0 && bytesRead < entry.Size)
                throw new IOException($"Unexpected end of file chain at block {currentBlock}");
        }

        return output;
    }

    public bool IsCompressed(byte[] data)
    {
        if (data.Length < 6) return false;

        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        int decompressedSize = (br.ReadByte() << 16) | (br.ReadByte() << 8) | br.ReadByte();
        int compressedSize = (br.ReadByte() << 16) | (br.ReadByte() << 8) | br.ReadByte();

        return decompressedSize != compressedSize;
    }

    public byte[] Decompress(byte[] archiveBuffer)
    {
        if (archiveBuffer == null || archiveBuffer.Length < 6) return [];
        
        byte[] data = [];
        
        try
        {
            using var ms = new MemoryStream(archiveBuffer);
            using var br = new BinaryReader(ms);

            int decompressedSize = br.ReadUnsignedMedium();
            int compressedSize = br.ReadUnsignedMedium();
            
            if (decompressedSize != compressedSize)
            {
                byte[] compressed = br.ReadBytes(compressedSize);
                data = BZip2Helper.Decompress(compressed);

                if (data.Length != decompressedSize)
                {
                    Console.WriteLine($"Warning: Decompressed size mismatch. Expected: {decompressedSize}, Got: {data.Length}");
                }
            }
            else
            {
                data = br.ReadBytes(decompressedSize);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading archive: {ex.Message}");
        }

        return data;
    }
    
    public void ParseArchiveData(byte[] data)
    {
        if (data == null || data.Length == 0) return;

        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        try
        {
            int fileCount = br.ReadUInt16BigEndian();
            Console.WriteLine($"Found {fileCount} files in archive");

            var entries = new List<(int id, int decompressedSize, int compressedSize, bool isCompressed)>();

            for (int i = 0; i < fileCount; i++)
            {
                int id = br.ReadInt32BigEndian();
                int decompressedSize = br.ReadUnsignedMedium();
                int compressedSize = br.ReadUnsignedMedium();
                bool isCompressed = decompressedSize != compressedSize;

                entries.Add((id, decompressedSize, compressedSize, isCompressed));
            }

            foreach (var (id, decompressedSize, compressedSize, isCompressed) in entries)
            {
                try
                {
                    byte[] fileData = br.ReadBytes(compressedSize);

                    if (isCompressed)
                    {
                        try
                        {
                            fileData = BZip2Helper.Decompress(fileData);
                            if (fileData.Length != decompressedSize)
                            {
                                Console.WriteLine(
                                    $"Size mismatch for {id}: Expected {decompressedSize}, got {fileData.Length}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Decompression failed for {id}: {ex.Message}");
                            continue;
                        }
                    }

                    Files.Add(new ArchiveFile(id, fileData, compressedSize, decompressedSize, isCompressed));
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
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _dataFile?.Dispose();
            if (_ownsIndexManager)
            {
                _indexManager?.Dispose();
            }

            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}

public class ArchiveFile
{
    public int Id { get; }
    public byte[] Data { get; set; }
    public int CompressedSize { get; set; }
    public int DecompressedSize { get; set; }
    public bool WasCompressed { get; set; }

    public ArchiveFile(int id, byte[] data, int compressedSize, int decompressedSize, bool wasCompressed)
    {
        Id = id;
        Data = data;
        CompressedSize = compressedSize;
        DecompressedSize = decompressedSize;
        WasCompressed = wasCompressed;
    }
}