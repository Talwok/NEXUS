using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class BlockDescr : KaitaiStruct
{
    public static BlockDescr FromFile(string fileName)
    {
        return new BlockDescr(new KaitaiStream(fileName));
    }

    public BlockDescr(KaitaiStream p__io, Frame.FdCurvesNew p__parent = null, NtMdt p__root = null) :
        base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }

    private void _read()
    {
        _nameLen = _stream.ReadU4Le();
        _len = _stream.ReadU4Le();
    }

    private uint _nameLen;
    private uint _len;
    private NtMdt m_root;
    private Frame.FdCurvesNew m_parent;

    public uint NameLen
    {
        get { return _nameLen; }
    }

    public uint Len
    {
        get { return _len; }
    }

    public NtMdt M_Root
    {
        get { return m_root; }
    }

    public Frame.FdCurvesNew M_Parent
    {
        get { return m_parent; }
    }
}
