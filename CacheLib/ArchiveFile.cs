namespace CacheLib;

public class ArchiveFile
{
    public int Id { get; }
    public byte[] Data { get; set; }
    public int CompressedSize { get; set; }
    public int DecompressedSize { get; set; }

    public ArchiveFile(int id, byte[] data, int compressedSize, int decompressedSize)
    {
        Id = id;
        Data = data;
        CompressedSize = compressedSize;
        DecompressedSize = decompressedSize;
    }
}