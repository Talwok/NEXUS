using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class DotsHeader : KaitaiStruct
{
    public static DotsHeader FromFile(string fileName)
    {
        return new DotsHeader(new KaitaiStream(fileName));
    }

    public DotsHeader(KaitaiStream p__io, Frame.Dots p__parent = null, NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }

    private void _read()
    {
        _headerSize = _stream.ReadS4Le();
        __raw_header = _stream.ReadBytes(HeaderSize);
        var io___raw_header = new KaitaiStream(__raw_header);
        _header = new Header(io___raw_header, this, m_root);
    }

    private int _headerSize;
    private Header _header;
    private NtMdt m_root;
    private Frame.Dots m_parent;
    private byte[] __raw_header;

    public int HeaderSize
    {
        get { return _headerSize; }
    }

    public Header Header
    {
        get { return _header; }
    }

    public NtMdt M_Root
    {
        get { return m_root; }
    }

    public Frame.Dots M_Parent
    {
        get { return m_parent; }
    }

    public byte[] M_RawHeader
    {
        get { return __raw_header; }
    }
}