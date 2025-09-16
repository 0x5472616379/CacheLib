namespace CacheLib;

public class DatFileReconstructor : IDisposable
{
    private IndexManager _indexManager;
    private FileStream _originalDataFile;
    private string _cacheDirectory;
    private bool _disposed;
    private bool _ownsIndexManager;

    public void Initialize(string cacheDirectory, IndexManager indexManager = null)
    {
        _cacheDirectory = cacheDirectory;
        _indexManager = indexManager ?? new IndexManager();
        _ownsIndexManager = indexManager == null;

        if (_ownsIndexManager)
        {
            _indexManager.Initialize(cacheDirectory);
        }

        string dataFilePath = Path.Combine(cacheDirectory, CacheConstants.DataFile);
        _originalDataFile = new FileStream(dataFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    }

    public void ReconstructComplete(string outputDirectory)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DatFileReconstructor));

        Directory.CreateDirectory(outputDirectory);

        string outputDatPath = Path.Combine(outputDirectory, CacheConstants.DataFile);
        ReconstructDatFile(outputDatPath);

        ReconstructIndexFiles(outputDirectory);

        Console.WriteLine("Complete reconstruction finished!");
    }

    private void ReconstructDatFile(string outputDatPath)
    {
        using var outputDat = new FileStream(outputDatPath, FileMode.Create);
        var blockMap = CreateBlockMap();
        var allBlocks = blockMap.Keys.OrderBy(k => k).ToList();

        Console.WriteLine($"Reconstructing DAT file with {allBlocks.Count} blocks...");

        for (int i = 0; i < allBlocks.Count; i++)
        {
            int blockNumber = allBlocks[i];
            bool isLastBlock = i == allBlocks.Count - 1;
            WriteBlock(outputDat, blockMap[blockNumber], blockNumber, isLastBlock);
        }

        Console.WriteLine("DAT reconstruction finished!");
    }


    private Dictionary<int, BlockInfo> CreateBlockMap()
    {
        var blockMap = new Dictionary<int, BlockInfo>();

        for (int idx = 0; idx < 5; idx++)
        {
            var entries = _indexManager.GetEntries(idx);

            foreach (var entry in entries)
            {
                if (entry.StartBlock == 0 && entry.Size == 0) continue;

                int currentBlock = entry.StartBlock;
                int bytesRemaining = entry.Size;
                int chunkIndex = 0;

                while (bytesRemaining > 0 && currentBlock != 0)
                {
                    long blockPosition = currentBlock * CacheConstants.BlockSize;

                    byte[] header = new byte[CacheConstants.HeaderSize];
                    _originalDataFile.Seek(blockPosition, SeekOrigin.Begin);
                    ReadFully(_originalDataFile, header, 0, header.Length);

                    int fileId = (header[0] << 8) | header[1];
                    int currentChunk = (header[2] << 8) | header[3];
                    int nextBlock = (header[4] << 16) | (header[5] << 8) | header[6];
                    CacheBlockType blockType = (CacheBlockType)header[7];

                    int bytesToRead = Math.Min(bytesRemaining, CacheConstants.ChunkSize);
                    byte[] data = new byte[bytesToRead];
                    _originalDataFile.Seek(blockPosition + CacheConstants.HeaderSize, SeekOrigin.Begin);
                    ReadFully(_originalDataFile, data, 0, bytesToRead);

                    blockMap[currentBlock] = new BlockInfo
                    {
                        FileId = fileId,
                        ChunkIndex = currentChunk,
                        NextBlock = nextBlock,
                        BlockType = blockType,
                        Data = data
                    };

                    bytesRemaining -= bytesToRead;
                    currentBlock = nextBlock;
                    chunkIndex++;
                }
            }
        }

        return blockMap;
    }

    private void ReadFully(Stream stream, byte[] buffer, int offset, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = stream.Read(buffer, offset + totalRead, count - totalRead);
            if (read == 0)
                throw new EndOfStreamException("Unexpected end of stream while reading DAT file.");
            totalRead += read;
        }
    }

    private void WriteBlock(FileStream output, BlockInfo blockInfo, int blockNumber, bool isLastBlock = false)
    {
        long position = blockNumber * CacheConstants.BlockSize;
        output.Seek(position, SeekOrigin.Begin);

        byte[] header = new byte[CacheConstants.HeaderSize];
        header[0] = (byte)((blockInfo.FileId >> 8) & 0xFF);
        header[1] = (byte)(blockInfo.FileId & 0xFF);
        header[2] = (byte)((blockInfo.ChunkIndex >> 8) & 0xFF);
        header[3] = (byte)(blockInfo.ChunkIndex & 0xFF);
        header[4] = (byte)((blockInfo.NextBlock >> 16) & 0xFF);
        header[5] = (byte)((blockInfo.NextBlock >> 8) & 0xFF);
        header[6] = (byte)(blockInfo.NextBlock & 0xFF);
        header[7] = (byte)blockInfo.BlockType;
        output.Write(header, 0, header.Length);

        output.Write(blockInfo.Data, 0, blockInfo.Data.Length);

        // Only pad if this is not the last block to prevent adding empty bytes at the end
        if (!isLastBlock)
        {
            int padding = CacheConstants.BlockSize - CacheConstants.HeaderSize - blockInfo.Data.Length;
            if (padding > 0)
                output.Write(new byte[padding], 0, padding);
        }
    }

    private void ReconstructIndexFiles(string outputDirectory)
    {
        for (int idx = 0; idx < 5; idx++)
        {
            var entries = _indexManager.GetEntries(idx);
            string outputPath = Path.Combine(outputDirectory, string.Format(CacheConstants.IndexFilePattern, idx));
            _indexManager.CreateNewIndexFile(outputPath, entries);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _originalDataFile?.Dispose();
            if (_ownsIndexManager)
            {
                _indexManager?.Dispose();
            }

            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    private class BlockInfo
    {
        public int FileId { get; set; }
        public int ChunkIndex { get; set; }
        public int NextBlock { get; set; }
        public CacheBlockType BlockType { get; set; }
        public byte[] Data { get; set; }
    }
}