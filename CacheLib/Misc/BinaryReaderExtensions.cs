using System.Text;

namespace CacheLib;

public static class BinaryReaderExtensions
{
    public static int ReadInt16BigEndian(this BinaryReader reader)
    {
        byte[] bytes = reader.ReadBytes(2);
        return (bytes[0] << 8) | bytes[1];
    }

    public static int ReadUnsignedMedium(this BinaryReader reader)
    {
        byte[] bytes = reader.ReadBytes(3);
        return (bytes[0] << 16) | (bytes[1] << 8) | bytes[2];
    }

    public static int ReadInt32BigEndian(this BinaryReader reader)
    {
        byte[] bytes = reader.ReadBytes(4);
        return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
    }
    
    public static ushort ReadUInt16BigEndian(this BinaryReader br)
    {
        byte[] bytes = br.ReadBytes(2);
        return (ushort)((bytes[0] << 8) | bytes[1]);
    }

    public const int StringTerminator = 10;
    public static string ReadSafeString(this BinaryReader reader)
    {
        var builder = new StringBuilder();
        byte b;
        while ((b = reader.ReadByte()) != StringTerminator)
            builder.Append((char)b);
        return builder.ToString();
    }

    public static int ReadUnsignedMedium(this MemoryStream stream)
    {
        return (stream.ReadByte() << 16) | (stream.ReadByte() << 8) | stream.ReadByte();
    }
}