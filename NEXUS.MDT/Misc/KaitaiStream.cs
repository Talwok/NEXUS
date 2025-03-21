namespace NEXUS.MDT.Misc;

public class KaitaiStream
{
    private Stream _stream;
    private BinaryReader _reader;

    public KaitaiStream(string fileName)
    {
        _stream = File.OpenRead(fileName);
        _reader = new BinaryReader(_stream);
    }

    public KaitaiStream(byte[] data)
    {
        _stream = new MemoryStream(data);
        _reader = new BinaryReader(_stream);
    }

    public byte[] ReadBytes(int count)
    {
        return _reader.ReadBytes(count);
    }

    public byte[] ReadBytesFull()
    {
        return _reader.ReadBytes((int)(_stream.Length - _stream.Position));
    }

    public uint ReadU4Le()
    {
        return _reader.ReadUInt32();
    }

    public ushort ReadU2Le()
    {
        return _reader.ReadUInt16();
    }

    public byte ReadU1()
    {
        return _reader.ReadByte();
    }

    public float ReadF4Le()
    {
        return _reader.ReadSingle();
    }

    public double ReadF8Le()
    {
        return _reader.ReadDouble();
    }

    public short ReadS2Le()
    {
        return _reader.ReadInt16();
    }

    public int ReadS4Le()
    {
        return _reader.ReadInt32();
    }

    public long ReadS8Le()
    {
        return _reader.ReadInt64();
    }

    public ulong ReadBitsIntBe(int n)
    {
        throw new NotImplementedException();
    }

    public bool IsEof => _stream.Position >= _stream.Length;

    public long Pos => _stream.Position;

    public void Seek(long position)
    {
        _stream.Seek(position, SeekOrigin.Begin);
    }
}