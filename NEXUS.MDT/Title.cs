using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Title : KaitaiStruct
{
    public static Title FromFile(string fileName)
    {
        return new Title(new KaitaiStream(fileName));
    }

    public Title(KaitaiStream p__io, KaitaiStruct p__parent = null, NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }
    private void _read()
    {
        _titleLen = _stream.ReadU4Le();
        _title = System.Text.Encoding.GetEncoding("cp1251").GetString(_stream.ReadBytes(TitleLen));
    }
    private uint _titleLen;
    private string _title;
    private NtMdt m_root;
    private KaitaiStruct m_parent;
    public uint TitleLen { get { return _titleLen; } }
    public string Title { get { return _title; } }
    public NtMdt M_Root { get { return m_root; } }
    public KaitaiStruct M_Parent { get { return m_parent; } }
}