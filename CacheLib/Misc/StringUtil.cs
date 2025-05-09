namespace CacheLib;

public class StringUtil
{
    public static string FormatSize(int bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes/1024.0:F1} KB";
        return $"{bytes/(1024.0*1024.0):F1} MB";
    }
    
    public static string GetCompressionDescription(CacheBlockType type) => type switch
    {
        CacheBlockType.Uncompressed => "Raw data",
        CacheBlockType.BZip2 => "Headerless BZIP2",
        CacheBlockType.GZip => "GZIP compressed",
        _ => "Unknown compression"
    };
}