using NEXUS.MDT.Enums;

namespace NEXUS.MDT;

public partial class Header : KaitaiStruct
{
    public static Header FromFile(string fileName)
    {
        return new Header(new KaitaiStream(fileName));
    }

    public Header(KaitaiStream p__io, Frame.Dots.DotsHeader p__parent = null,
        NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }

    private void _read()
    {
        _coordSize = _stream.ReadS4Le();
        _version = _stream.ReadS4Le();
        _xyunits = ((Unit)_stream.ReadS2Le());
    }

    private int _coordSize;
    private int _version;
    private Unit _xyunits;
    private NtMdt m_root;
    private Frame.Dots.DotsHeader m_parent;

    public int CoordSize
    {
        get { return _coordSize; }
    }

    public int Version
    {
        get { return _version; }
    }

    public Unit Xyunits
    {
        get { return _xyunits; }
    }

    public NtMdt M_Root
    {
        get { return m_root; }
    }

    public Frame.Dots.DotsHeader M_Parent
    {
        get { return m_parent; }
    }
}
