namespace CacheLib;

public enum CacheBlockType : byte
{
    /// <summary>
    /// Raw uncompressed data
    /// </summary>
    Uncompressed = 0,
        
    /// <summary>
    /// BZIP2 compressed data without header (RS classic format)
    /// </summary>
    BZip2 = 1,
        
    /// <summary>
    /// GZIP compressed data
    /// </summary>
    GZip = 2
}