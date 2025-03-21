using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Dot : KaitaiStruct
{
    public static Dot FromFile(string fileName)
    {
        return new Dot(new KaitaiStream(fileName));
    }

    public Dot(KaitaiStream p__io, KaitaiStruct p__parent = null, NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }

    private void _read()
    {
        _x = _stream.ReadS2Le();
        _y = _stream.ReadS2Le();
    }

    private short _x;
    private short _y;
    private NtMdt m_root;
    private KaitaiStruct m_parent;

    public short X
    {
        get { return _x; }
    }

    public short Y
    {
        get { return _y; }
    }

    public NtMdt M_Root
    {
        get { return m_root; }
    }

    public KaitaiStruct M_Parent
    {
        get { return m_parent; }
    }
}