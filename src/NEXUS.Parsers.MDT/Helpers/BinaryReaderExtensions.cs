namespace NEXUS.Parsers.MDT.Helpers;

public static class BinaryReaderExtensions
{
    public static short ReadInt16Le(this BinaryReader reader)
    {
        var bytes = reader.ReadBytes(2);
        return BitConverter.ToInt16(bytes.Reverse().ToArray(), 0);
    }

    public static ushort ReadUInt16Le(this BinaryReader reader)
    {
        var bytes = reader.ReadBytes(2);
        return BitConverter.ToUInt16(bytes.Reverse().ToArray(), 0);
    }

    public static int ReadInt32Le(this BinaryReader reader)
    {
        var bytes = reader.ReadBytes(4);
        return BitConverter.ToInt32(bytes.Reverse().ToArray(), 0);
    }

    public static uint ReadUInt32Le(this BinaryReader reader)
    {
        var bytes = reader.ReadBytes(4);
        return BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0);
    }
    
    public static long ReadInt64Le(this BinaryReader reader)
    {
        var bytes = reader.ReadBytes(8);
        return BitConverter.ToInt64(bytes.Reverse().ToArray(), 0);
    }
    
    public static ulong ReadUInt64Le(this BinaryReader reader)
    {
        var bytes = reader.ReadBytes(8);
        return BitConverter.ToUInt64(bytes.Reverse().ToArray(), 0);
    }

    public static float ReadSingleLe(this BinaryReader reader)
    {
        var bytes = reader.ReadBytes(4);
        return BitConverter.ToSingle(bytes.Reverse().ToArray(), 0);
    }

    public static double ReadDoubleLe(this BinaryReader reader)
    {
        var bytes = reader.ReadBytes(8);
        return BitConverter.ToDouble(bytes.Reverse().ToArray(), 0);
    }
}