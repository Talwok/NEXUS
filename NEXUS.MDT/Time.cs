using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Time : KaitaiStruct
{
    public static Time FromFile(string fileName)
    {
        return new Time(new KaitaiStream(fileName));
    }

    public Time(KaitaiStream p__io, Frame.DateTime p__parent = null, NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }

    private void _read()
    {
        _hour = _stream.ReadU2Le();
        _min = _stream.ReadU2Le();
        _sec = _stream.ReadU2Le();
    }

    private ushort _hour;
    private ushort _min;
    private ushort _sec;
    private NtMdt m_root;
    private Frame.DateTime m_parent;

    /// <summary>
    /// h_h
    /// </summary>
    public ushort Hour
    {
        get { return _hour; }
    }

    /// <summary>
    /// h_m
    /// </summary>
    public ushort Min
    {
        get { return _min; }
    }

    /// <summary>
    /// h_s
    /// </summary>
    public ushort Sec
    {
        get { return _sec; }
    }

    public NtMdt M_Root
    {
        get { return m_root; }
    }

    public Frame.DateTime M_Parent
    {
        get { return m_parent; }
    }
}