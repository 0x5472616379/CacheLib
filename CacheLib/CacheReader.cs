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
            Console.WriteLine(
                $"  Block Type: {nextBlockType} ({(byte)nextBlockType}) - {StringUtil.GetCompressionDescription(nextBlockType)}");

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


    public void WriteFile(int indexId, int fileId, byte[] data)
    {
        WriteFileInternal(indexId, fileId, data);
    }

    private void WriteFileInternal(int indexId, int fileId, byte[] data)
    {
        /* 1. Update the index entry */
        int startBlock = (int)((_dataFile.Length + CacheConstants.BlockSize - 1) / CacheConstants.BlockSize);
        if (startBlock == 0) startBlock = 1; /* First block can't be 0 */

        IndexEntry entry = new IndexEntry(data.Length, startBlock);
        WriteIndexEntry(indexId, fileId, entry);

        /* 2. Write the file blocks */
        WriteFileBlocks(entry, data);
    }

    private void WriteIndexEntry(int indexId, int fileId, IndexEntry entry)
    {
        if (indexId < 0 || indexId >= _indexFiles.Length)
            throw new ArgumentOutOfRangeException(nameof(indexId));

        byte[] buffer = new byte[CacheConstants.IndexEntrySize];
        buffer[0] = (byte)(entry.Size >> 16);
        buffer[1] = (byte)(entry.Size >> 8);
        buffer[2] = (byte)entry.Size;
        buffer[3] = (byte)(entry.StartBlock >> 16);
        buffer[4] = (byte)(entry.StartBlock >> 8);
        buffer[5] = (byte)entry.StartBlock;

        FileStream indexFile = _indexFiles[indexId];
        lock (indexFile)
        {
            long position = fileId * CacheConstants.IndexEntrySize;
            indexFile.Seek(position, SeekOrigin.Begin);
            indexFile.Write(buffer, 0, buffer.Length);
        }
    }

    private void WriteFileBlocks(IndexEntry entry, byte[] data)
    {
        int bytesWritten = 0;
        int currentBlock = entry.StartBlock;
        int part = 0;

        while (bytesWritten < entry.Size)
        {
            /* Calculate next block */
            int nextBlock = 0;
            if (bytesWritten + CacheConstants.ChunkSize < entry.Size)
            {
                nextBlock = (int)((_dataFile.Length + CacheConstants.BlockSize - 1) / CacheConstants.BlockSize);
                if (nextBlock == currentBlock) nextBlock++;
            }

            /* Prepare header */
            byte[] header = new byte[CacheConstants.HeaderSize];
            header[0] = (byte)(0 >> 8); /* File ID high byte (always 0 for archive I believe) */
            header[1] = (byte)0; /* File ID low byte */
            header[2] = (byte)(part >> 8);
            header[3] = (byte)part;
            header[4] = (byte)(nextBlock >> 16);
            header[5] = (byte)(nextBlock >> 8);
            header[6] = (byte)nextBlock;
            header[7] = (byte)CacheBlockType.BZip2; /* Compression type, 1 in this case, just like how we read it */

            /* Write the block */
            lock (_dataFile)
            {
                /* Seek to block position*/
                long blockPosition = currentBlock * CacheConstants.BlockSize;
                _dataFile.Seek(blockPosition, SeekOrigin.Begin);

                /* Write header */
                _dataFile.Write(header, 0, header.Length);

                /* Write data */
                int bytesToWrite = Math.Min(CacheConstants.ChunkSize, data.Length - bytesWritten);
                _dataFile.Write(data, bytesWritten, bytesToWrite);

                /* Pad with zeros if needed */
                if (bytesToWrite < CacheConstants.ChunkSize)
                {
                    byte[] padding = new byte[CacheConstants.ChunkSize - bytesToWrite];
                    _dataFile.Write(padding, 0, padding.Length);
                }
            }

            bytesWritten += CacheConstants.ChunkSize;
            currentBlock = nextBlock;
            part++;
        }
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