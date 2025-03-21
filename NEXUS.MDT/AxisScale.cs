using NEXUS.MDT.Enums;
using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class AxisScale : KaitaiStruct
{
    public static AxisScale FromFile(string fileName)
    {
        return new AxisScale(new KaitaiStream(fileName));
    }

    public AxisScale(KaitaiStream p__io, KaitaiStruct p__parent = null, NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root;
        _read();
    }

    private void _read()
    {
        _offset = _stream.ReadF4Le();
        _step = _stream.ReadF4Le();
        _unit = ((Unit)_stream.ReadS2Le());
    }

    private float _offset;
    private float _step;
    private Unit _unit;
    private NtMdt m_root;
    private KaitaiStruct m_parent;

    /// <summary>
    /// x_scale-&gt;offset = gwy_get_gfloat_le(&amp;p);# r0 (physical units)
    /// </summary>
    public float Offset
    {
        get { return _offset; }
    }

    /// <summary>
    /// x_scale-&gt;step = gwy_get_gfloat_le(&amp;p); r (physical units) x_scale-&gt;step = fabs(x_scale-&gt;step); if (!x_scale-&gt;step) {
    ///   g_warning(&quot;x_scale.step == 0, changing to 1&quot;);
    ///   x_scale-&gt;step = 1.0;
    /// }
    /// </summary>
    public float Step
    {
        get { return _step; }
    }

    /// <summary>
    /// U
    /// </summary>
    public Unit Unit
    {
        get { return _unit; }
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