namespace CacheLib;

public static class CacheConstants
{
    // File names
    public const string DataFile = "main_file_cache.dat";
    public const string IndexFilePattern = "main_file_cache.idx{0}";
    
    // Sizes
    public const int ChunkSize = 512;
    public const int HeaderSize = 8;
    public const int BlockSize = HeaderSize + ChunkSize;
    public const int IndexEntrySize = 6;
    
    // Cache types
    public const int ArchiveIndex = 0;
    public const int ModelIndex = 1;
    public const int AnimationIndex = 2;
    public const int MusicIndex = 3;
    public const int MapIndex = 4;
}

public struct IndexEntry
{
    public int Size { get; }
    public int StartBlock { get; }
    
    public IndexEntry(int size, int startBlock)
    {
        Size = size;
        StartBlock = startBlock;
    }
}