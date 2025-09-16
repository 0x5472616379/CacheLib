namespace CacheLib;

public class IndexManager : IDisposable
{
    private FileStream[] _indexFiles;
    private bool _disposed = false;
    
    public void Initialize(string cacheDirectory)
    {
        _indexFiles = new FileStream[5];
        for (int i = 0; i < 5; i++)
        {
            string indexPath = Path.Combine(cacheDirectory, string.Format(CacheConstants.IndexFilePattern, i));
            _indexFiles[i] = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
    }

    public int GetEntryCount(int idx)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(IndexManager));
        return (int)_indexFiles[idx].Length / CacheConstants.IndexEntrySize;
    }

    public List<IndexEntry> GetEntries(int idx)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(IndexManager));
        
        var count = GetEntryCount(idx);
        byte[] buffer = new byte[CacheConstants.IndexEntrySize];
        List<IndexEntry> entries = new List<IndexEntry>();
        
        for (int fileId = 0; fileId < count; fileId++)
        {
            long position = fileId * CacheConstants.IndexEntrySize;
            _indexFiles[idx].Seek(position, SeekOrigin.Begin);
            int bytesRead = _indexFiles[idx].Read(buffer, 0, buffer.Length);
            
            if (bytesRead != buffer.Length)
                throw new IOException($"Failed to read index entry at position {position}");
            
            int size = (buffer[0] << 16) | (buffer[1] << 8) | buffer[2];
            int block = (buffer[3] << 16) | (buffer[4] << 8) | buffer[5];
            entries.Add(new IndexEntry(size, block, fileId));
        }
        
        return entries;
    }
    
    public void CreateNewIndexFile(string outputPath, List<IndexEntry> entries)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(IndexManager));
        
        var parentDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(parentDirectory))
        {
            Directory.CreateDirectory(parentDirectory);
        }
    
        using (FileStream fs = new FileStream(outputPath, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            foreach (var entry in entries)
            {
                writer.Write((byte)((entry.Size >> 16) & 0xFF));
                writer.Write((byte)((entry.Size >> 8) & 0xFF));
                writer.Write((byte)(entry.Size & 0xFF));
                writer.Write((byte)((entry.StartBlock >> 16) & 0xFF));
                writer.Write((byte)((entry.StartBlock >> 8) & 0xFF));
                writer.Write((byte)(entry.StartBlock & 0xFF));
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var file in _indexFiles)
            {
                file?.Dispose();
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}