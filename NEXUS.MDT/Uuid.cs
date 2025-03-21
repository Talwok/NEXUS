using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Uuid : KaitaiStruct
{
    public static Uuid FromFile(string fileName)
    {
        return new Uuid(new KaitaiStream(fileName));
    }

    public Uuid(KaitaiStream p__io, Frame.FdMetaData p__parent = null, NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }
    private void _read()
    {
        _data = new List<byte>();
        for (var i = 0; i < 16; i++)
        {
            _data.Add(_stream.ReadU1());
        }
    }
    private List<byte> _data;
    private NtMdt m_root;
    private Frame.FdMetaData m_parent;
    public List<byte> Data { get { return _data; } }
    public NtMdt M_Root { get { return m_root; } }
    public Frame.FdMetaData M_Parent { get { return m_parent; } }
}