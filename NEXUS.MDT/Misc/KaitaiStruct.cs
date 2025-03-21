
namespace NEXUS.MDT.Misc;

public class KaitaiStruct
{
    protected KaitaiStream _stream;
    protected KaitaiStruct? _parent;
    protected KaitaiStruct _root;

    public KaitaiStruct(
        KaitaiStream stream, 
        KaitaiStruct? parent = null, 
        KaitaiStruct? root = null)
    {
        _stream = stream;
        _parent = parent;
        _root = root ?? this;
    }

    public static byte[]? ByteArrayCompare(byte[]? firstArray, byte[]? secondArray)
    {
        if (firstArray == null || secondArray == null)
            return null;
        
        if (firstArray.Length != secondArray.Length)
            return null;

        for (var i = 0; i < firstArray.Length; i++)
            if (firstArray[i] != secondArray[i])
                return null;

        return firstArray;
    }
}