using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Version : KaitaiStruct
{
    public static Version FromFile(string fileName)
    {
        return new Version(new KaitaiStream(fileName));
    }

    public Version(KaitaiStream p__io, Frame.FrameMain p__parent = null, NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }
    private void _read()
    {
        _minor = _stream.ReadU1();
        _major = _stream.ReadU1();
    }
    private byte _minor;
    private byte _major;
    private NtMdt m_root;
    private Frame.FrameMain m_parent;
    public byte Minor { get { return _minor; } }
    public byte Major { get { return _major; } }
    public NtMdt M_Root { get { return m_root; } }
    public Frame.FrameMain M_Parent { get { return m_parent; } }
}