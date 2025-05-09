namespace CacheLib;


public static class BinaryWriterExtensions
{
    public static void WriteInt16BigEndian(this BinaryWriter writer, short value)
    {
        writer.Write((byte)(value >> 8));
        writer.Write((byte)value);
    }

    public static void WriteInt32BigEndian(this BinaryWriter writer, int value)
    {
        writer.Write((byte)(value >> 24));
        writer.Write((byte)(value >> 16));
        writer.Write((byte)(value >> 8));
        writer.Write((byte)value);
    }

    public static void WriteMedium(this BinaryWriter writer, int value)
    {
        if (value < 0 || value > 0xFFFFFF)
            throw new ArgumentOutOfRangeException(nameof(value), "Medium must be between 0 and 16,777,215");

        writer.Write((byte)(value >> 16));
        writer.Write((byte)(value >> 8));
        writer.Write((byte)value);
    }
    
    public static void WriteSafeString(this BinaryWriter writer, string str)
    {
        foreach (char c in str)
        {
            writer.Write((byte)c);
        }
        writer.Write((byte)10);
    }
    

    public static void WriteUInt16BigEndian(this BinaryWriter writer, ushort value)
    {
        writer.Write((byte)(value >> 8));
        writer.Write((byte)value);
    }
}