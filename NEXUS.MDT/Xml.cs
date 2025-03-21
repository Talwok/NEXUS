using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Xml : KaitaiStruct
{
    public static Xml FromFile(string fileName)
    {
        return new Xml(new KaitaiStream(fileName));
    }

    public Xml(KaitaiStream p__io, KaitaiStruct p__parent = null, NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }
    private void _read()
    {
        _xmlLen = _stream.ReadU4Le();
        _xml = System.Text.Encoding.GetEncoding("UTF-16LE").GetString(_stream.ReadBytes(XmlLen));
    }
    private uint _xmlLen;
    private string _xml;
    private NtMdt m_root;
    private KaitaiStruct m_parent;
    public uint XmlLen { get { return _xmlLen; } }
    public string Xml { get { return _xml; } }
    public NtMdt M_Root { get { return m_root; } }
    public KaitaiStruct M_Parent { get { return m_parent; } }
}