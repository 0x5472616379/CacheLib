namespace CacheLib;

public class CacheReader
{
    private readonly FileStream _dataFile;
    private readonly FileStream[] _indexFiles;

    public CacheReader(string cacheDirectory)
    {
        // Open data file
        string dataFilePath = Path.Combine(cacheDirectory, CacheConstants.DataFile);
        _dataFile = new FileStream(dataFilePath, FileMode.Open, FileAccess.ReadWrite);

        // Open index files (idx0..4)
        _indexFiles = new FileStream[5];
        for (int i = 0; i < 5; i++)
        {
            string indexPath = Path.Combine(cacheDirectory, string.Format(CacheConstants.IndexFilePattern, i));
            _indexFiles[i] = new FileStream(indexPath, FileMode.Open, FileAccess.ReadWrite);
        }
    }

    public byte[] ReadFile(int indexId, int fileId)
    {
        byte[] data = ReadFileInternal(indexId, fileId);

        if (IsCompressed(data))
        {
            return BZip2Helper.Decompress(data);
        }

        return data;
    }

    private byte[] ReadFileInternal(int indexId, int fileId)
    {
        IndexEntry entry = ReadIndexEntry(indexId, fileId);
        return ReadFileBlocks(entry);
    }

    private bool IsCompressed(byte[] data)
    {
        if (data.Length < 6) return false;

        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        int decompressedSize = (br.ReadByte() << 16) | (br.ReadByte() << 8) | br.ReadByte();
        int compressedSize = (br.ReadByte() << 16) | (br.ReadByte() << 8) | br.ReadByte();

        return decompressedSize != compressedSize;
    }

    /// <summary>
    /// Gets the entry data based on fileId
    /// </summary>
    /// <param name="indexId"></param>
    /// <param name="fileId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private IndexEntry ReadIndexEntry(int indexId, int fileId)
    {
        if (indexId < 0 || indexId >= _indexFiles.Length)
            throw new ArgumentOutOfRangeException(nameof(indexId));

        FileStream indexFile = _indexFiles[indexId];
        byte[] buffer = new byte[CacheConstants.IndexEntrySize];

        lock (indexFile)
        {
            long position = fileId * CacheConstants.IndexEntrySize;
            if (position < 0 || position >= indexFile.Length)
                throw new ArgumentOutOfRangeException(nameof(fileId));

            indexFile.Seek(position, SeekOrigin.Begin);
            indexFile.Read(buffer, 0, buffer.Length);
        }

        // Parse the 6 byte index entry
        int size = (buffer[0] << 16) | (buffer[1] << 8) | buffer[2];
        int block = (buffer[3] << 16) | (buffer[4] << 8) | buffer[5];

        return new IndexEntry(size, block);
    }

    private byte[] ReadFileBlocks(IndexEntry entry)
    {
        Console.WriteLine($"\n=== Reading File Blocks ===");
        Console.WriteLine($"Total Size: {entry.Size} bytes");
        Console.WriteLine($"Starting Block: {entry.StartBlock}");

        byte[] output = new byte[entry.Size];
        int bytesRead = 0;
        int currentBlock = entry.StartBlock;
        int chunkCounter = 0;
        CacheBlockType compressionType = CacheBlockType.Uncompressed;
        
        while (bytesRead < entry.Size)
        {
            byte[] header = new byte[CacheConstants.HeaderSize];
            lock (_dataFile)
            {
                long blockPosition = currentBlock * CacheConstants.BlockSize;
                Console.WriteLine($"\n[Block {currentBlock}] Seeking to position: {blockPosition}");
            
                _dataFile.Seek(blockPosition, SeekOrigin.Begin);
                _dataFile.Read(header, 0, header.Length);
            }

            // Parse header with debug info
            int fileId = (header[0] << 8) | header[1];
            int currentChunk = (header[2] << 8) | header[3];
            int nextBlock = (header[4] << 16) | (header[5] << 8) | header[6];
            var nextBlockType = (CacheBlockType)header[7];
            
            Console.WriteLine($"  Header Data: {BitConverter.ToString(header)}");
            Console.WriteLine($"  File ID: {fileId} (0x{fileId:X4})");
            Console.WriteLine($"  Current Chunk: {currentChunk}");
            Console.WriteLine($"  Next Block: {nextBlock}");
            Console.WriteLine($"  Block Type: {nextBlockType} ({(byte)nextBlockType}) - {StringUtil.GetCompressionDescription(nextBlockType)}");

            int remainingBytes = entry.Size - bytesRead;
            int bytesToRead = Math.Min(remainingBytes, CacheConstants.ChunkSize);
        
            Console.WriteLine($"  Reading {bytesToRead} bytes (chunk {chunkCounter++})");

            lock (_dataFile)
            {
                _dataFile.ReadExactly(output, bytesRead, bytesToRead);
            }

            bytesRead += bytesToRead;
            currentBlock = nextBlock;
        }

        Console.WriteLine($"\n=== Finished Reading {bytesRead} bytes ===");
        return output;
    }

    public void Close()
    {
        _dataFile.Close();
        foreach (var indexFile in _indexFiles)
        {
            indexFile.Close();
        }
    }
}