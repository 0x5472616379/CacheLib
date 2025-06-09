using ICSharpCode.SharpZipLib.BZip2;

namespace CacheLib;

/* This works perfectly */

public static class BZip2Helper
{
    // RuneScape BZip2 files lack headers, so we need to add them
    private static readonly byte[] BZip2Header = "BZh1"u8.ToArray();

    public static byte[] Decompress(byte[] compressedData)
    {
        // Add standard BZIP2 header since Jagex strips it
        byte[] withHeader = new byte[compressedData.Length + BZip2Header.Length];
        Buffer.BlockCopy(BZip2Header, 0, withHeader, 0, BZip2Header.Length);
        Buffer.BlockCopy(compressedData, 0, withHeader, BZip2Header.Length, compressedData.Length);

        using var input = new MemoryStream(withHeader);
        using var output = new MemoryStream();
        using var decompressor = new BZip2InputStream(input);

        decompressor.CopyTo(output);
        return output.ToArray();
    }

    public static byte[] Compress(byte[] data)
    {
        using var output = new MemoryStream();
        using var compressor = new BZip2OutputStream(output);

        compressor.Write(data, 0, data.Length);
        compressor.Close();

        // The client expects the data without BZIP2 header (I think)
        byte[] compressed = output.ToArray();
        byte[] withoutHeader = new byte[compressed.Length - 4];
        Buffer.BlockCopy(compressed, 4, withoutHeader, 0, withoutHeader.Length);

        return withoutHeader;
    }
}