using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Framez : KaitaiStruct
{
    public static Framez FromFile(string fileName)
    {
        return new Framez(new KaitaiStream(fileName));
    }

    public Framez(KaitaiStream p__io, NtMdt p__parent = null, NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }
    private void _read()
    {
        _frames = new List<Frame>();
        for (var i = 0; i < (M_Root.LastFrame + 1); i++)
        {
            _frames.Add(new Frame(_stream, this, m_root));
        }
    }
    private List<Frame> _frames;
    private NtMdt m_root;
    private NtMdt m_parent;
    public List<Frame> Frames { get { return _frames; } }
    public NtMdt M_Root { get { return m_root; } }
    public NtMdt M_Parent { get { return m_parent; } }
}