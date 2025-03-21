using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Date : KaitaiStruct
{
    public static Date FromFile(string fileName)
    {
        return new Date(new KaitaiStream(fileName));
    }

    public Date(KaitaiStream p__io, Frame.DateTime p__parent = null, NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }

    private void _read()
    {
        _year = _stream.ReadU2Le();
        _month = _stream.ReadU2Le();
        _day = _stream.ReadU2Le();
    }

    private ushort _year;
    private ushort _month;
    private ushort _day;
    private NtMdt m_root;
    private Frame.DateTime m_parent;

    /// <summary>
    /// h_yea
    /// </summary>
    public ushort Year
    {
        get { return _year; }
    }

    /// <summary>
    /// h_mon
    /// </summary>
    public ushort Month
    {
        get { return _month; }
    }

    /// <summary>
    /// h_day
    /// </summary>
    public ushort Day
    {
        get { return _day; }
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
