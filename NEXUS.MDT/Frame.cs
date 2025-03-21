using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Frame : KaitaiStruct
{
    public static Frame FromFile(string fileName)
    {
        return new Frame(new KaitaiStream(fileName));
    }


    

    public Frame(KaitaiStream p__io, Framez p__parent = null, NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }

    private void _read()
    {
        _size = _stream.ReadU4Le();
        __raw_main = _stream.ReadBytes((Size - 4));
        var io___raw_main = new KaitaiStream(__raw_main);
        _main = new FrameMain(io___raw_main, this, m_root);
    }
    

    

    
    

    

    

    

    private uint _size;
    private FrameMain _main;
    private NtMdt m_root;
    private Framez m_parent;
    private byte[] __raw_main;

    /// <summary>
    /// h_sz
    /// </summary>
    public uint Size
    {
        get { return _size; }
    }

    public FrameMain Main
    {
        get { return _main; }
    }

    public NtMdt M_Root
    {
        get { return m_root; }
    }

    public Framez M_Parent
    {
        get { return m_parent; }
    }

    public byte[] M_RawMain
    {
        get { return __raw_main; }
    }
}