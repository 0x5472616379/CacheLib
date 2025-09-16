namespace CacheLib;

public static class CacheConstants
{
    public const string DataFile = "main_file_cache.dat";
    public const string IndexFilePattern = "main_file_cache.idx{0}";

    public const int ChunkSize = 512;
    public const int HeaderSize = 8;
    public const int BlockSize = HeaderSize + ChunkSize;
    public const int IndexEntrySize = 6;

    public const int ArchiveIndex = 0;
    public const int ModelIndex = 1;
    public const int AnimationIndex = 2;
    public const int MusicIndex = 3;
    public const int MapIndex = 4;
}

public class IndexEntry
{
    public int Size { get; private set; }
    public int StartBlock { get; private set; }
    public int FileId { get; set; }

    public IndexEntry(int size, int startBlock, int fileId = -1)
    {
        Size = size;
        StartBlock = startBlock;
        FileId = fileId;
    }
}

public enum ArchiveType
{
    ArchiveIndex = 0,
    ModelIndex = 1,
    AnimationIndex = 2,
    MusicIndex = 3,
    MapIndex = 4,
}