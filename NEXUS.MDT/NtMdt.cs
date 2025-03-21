using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

/// <summary>
/// A native file format of NT-MDT scientific software. Usually contains
/// any of:
/// 
/// * [Scanning probe](https://en.wikipedia.org/wiki/Scanning_probe_microscopy) microscopy scans and spectra
/// * [Raman spectra](https://en.wikipedia.org/wiki/Raman_spectroscopy)
/// * results of their analysis
/// 
/// Some examples of mdt files can be downloaded at:
/// 
/// * &lt;https://www.ntmdt-si.ru/resources/scan-gallery&gt;
/// * &lt;http://callistosoft.narod.ru/Resources/Mdt.zip&gt;
/// </summary>
/// <remarks>
/// Reference: <a href="https://svn.code.sf.net/p/gwyddion/code/trunk/gwyddion/modules/file/nt-mdt.c">Source</a>
/// </remarks>
public partial class NtMdt : KaitaiStruct
{
    public static NtMdt FromFile(string fileName)
    {
        return new NtMdt(new KaitaiStream(fileName));
    }

    public NtMdt(KaitaiStream p__io, KaitaiStruct p__parent = null, NtMdt p__root = null) : base(p__io)
    {
        m_parent = p__parent;
        m_root = p__root ?? this;
        _read();
    }

    private void _read()
    {
        _signature = _stream.ReadBytes(4);
        if (!((KaitaiStream.ByteArrayCompare(Signature, new byte[] { 1, 176, 147, 255 }) == 0)))
        {
            throw new ValidationNotEqualError(new byte[] { 1, 176, 147, 255 }, Signature, M_Io, "/seq/0");
        }

        _size = _stream.ReadU4Le();
        _reserved0 = _stream.ReadBytes(4);
        _lastFrame = _stream.ReadU2Le();
        _reserved1 = _stream.ReadBytes(18);
        _wrondDoc = _stream.ReadBytes(1);
        __raw_frames = _stream.ReadBytes(Size);
        var io___raw_frames = new KaitaiStream(__raw_frames);
        _frames = new Framez(io___raw_frames, this, m_root);
    }

    private byte[] _signature;
    private uint _size;
    private byte[] _reserved0;
    private ushort _lastFrame;
    private byte[] _reserved1;
    private byte[] _wrondDoc;
    private Framez _frames;
    private NtMdt m_root;
    private KaitaiStruct m_parent;
    private byte[] __raw_frames;

    public byte[] Signature
    {
        get { return _signature; }
    }

    /// <summary>
    /// File size (w/o header)
    /// </summary>
    public uint Size
    {
        get { return _size; }
    }

    public byte[] Reserved0
    {
        get { return _reserved0; }
    }

    public ushort LastFrame
    {
        get { return _lastFrame; }
    }

    public byte[] Reserved1
    {
        get { return _reserved1; }
    }

    /// <summary>
    /// documentation specifies 32 bytes long header, but zeroth frame starts at 33th byte in reality
    /// </summary>
    public byte[] WrondDoc
    {
        get { return _wrondDoc; }
    }

    public Framez Frames
    {
        get { return _frames; }
    }

    public NtMdt M_Root
    {
        get { return m_root; }
    }

    public KaitaiStruct M_Parent
    {
        get { return m_parent; }
    }

    public byte[] M_RawFrames
    {
        get { return __raw_frames; }
    }
}