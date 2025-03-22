using Kaitai;

namespace NEXUS.Parsers.MDT;

/// <summary>
/// It is a color scheme for visualising SPM scans.
/// </summary>
public partial class Pallete : KaitaiStruct
{
    public static Pallete FromFile(string fileName)
    {
        return new Pallete(new KaitaiStream(fileName));
    }

    public Pallete(KaitaiStream io, KaitaiStruct parent = null, Pallete root = null) : base(io)
    {
        _parent = parent;
        _root = root ?? this;
        _read();
    }

    private void _read()
    {
        _signature = m_io.ReadBytes(26);
        if (!((KaitaiStream.ByteArrayCompare(Signature,
                new byte[]
                {
                    78, 84, 45, 77, 68, 84, 32, 80, 97, 108, 101, 116, 116, 101, 32, 70, 105, 108, 101, 32, 32, 49, 46,
                    48, 48, 33
                }) == 0)))
        {
            throw new ValidationNotEqualError(
                new byte[]
                {
                    78, 84, 45, 77, 68, 84, 32, 80, 97, 108, 101, 116, 116, 101, 32, 70, 105, 108, 101, 32, 32, 49, 46,
                    48, 48, 33
                }, Signature, M_Io, "/seq/0");
        }

        _count = m_io.ReadU4be();
        _metaValue = new List<Meta>();
        for (var i = 0; i < Count; i++)
        {
            _metaValue.Add(new Meta(m_io, this, _root));
        }

        _something2 = m_io.ReadBytes(1);
        _tables = new List<ColTable>();
        for (ushort i = 0; i < Count; i++)
        {
            _tables.Add(new ColTable(i, m_io, this, _root));
        }
    }

    public partial class Meta : KaitaiStruct
    {
        public static Meta FromFile(string fileName)
        {
            return new Meta(new KaitaiStream(fileName));
        }

        public Meta(KaitaiStream io, Pallete parent = null, Pallete root = null) : base(io)
        {
            _parent = parent;
            _root = root;
            _read();
        }

        private void _read()
        {
            _unknown00 = m_io.ReadBytes(3);
            _unknown01 = m_io.ReadBytes(2);
            _unknown02 = m_io.ReadBytes(1);
            _unknown03 = m_io.ReadBytes(1);
            _colorsCount = m_io.ReadU2le();
            _unknown10 = m_io.ReadBytes(2);
            _unknown11 = m_io.ReadBytes(1);
            _unknown12 = m_io.ReadBytes(2);
            _nameSize = m_io.ReadU2be();
        }

        private byte[] _unknown00;
        private byte[] _unknown01;
        private byte[] _unknown02;
        private byte[] _unknown03;
        private ushort _colorsCount;
        private byte[] _unknown10;
        private byte[] _unknown11;
        private byte[] _unknown12;
        private ushort _nameSize;
        private Pallete _root;
        private Pallete _parent;

        /// <summary>
        /// usually 0s
        /// </summary>
        public byte[] Unknown00
        {
            get { return _unknown00; }
        }

        public byte[] Unknown01
        {
            get { return _unknown01; }
        }

        public byte[] Unknown02
        {
            get { return _unknown02; }
        }

        /// <summary>
        /// usually 0s
        /// </summary>
        public byte[] Unknown03
        {
            get { return _unknown03; }
        }

        public ushort ColorsCount
        {
            get { return _colorsCount; }
        }

        /// <summary>
        /// usually 0s
        /// </summary>
        public byte[] Unknown10
        {
            get { return _unknown10; }
        }

        /// <summary>
        /// usually 4
        /// </summary>
        public byte[] Unknown11
        {
            get { return _unknown11; }
        }

        /// <summary>
        /// usually 0s
        /// </summary>
        public byte[] Unknown12
        {
            get { return _unknown12; }
        }

        public ushort NameSize
        {
            get { return _nameSize; }
        }

        public Pallete Root
        {
            get { return _root; }
        }

        public Pallete Parent
        {
            get { return _parent; }
        }
    }

    public partial class Color : KaitaiStruct
    {
        public static Color FromFile(string fileName)
        {
            return new Color(new KaitaiStream(fileName));
        }

        public Color(KaitaiStream io, ColTable parent = null, Pallete root = null) : base(io)
        {
            _parent = parent;
            _root = root;
            _read();
        }

        private void _read()
        {
            _red = m_io.ReadU1();
            _unknown = m_io.ReadU1();
            _blue = m_io.ReadU1();
            _green = m_io.ReadU1();
        }

        private byte _red;
        private byte _unknown;
        private byte _blue;
        private byte _green;
        private Pallete _root;
        private ColTable _parent;

        public byte Unknown
        {
            get { return _unknown; }
        }

        public byte Red
        {
            get { return _red; }
        }

        public byte Blue
        {
            get { return _blue; }
        }

        public byte Green
        {
            get { return _green; }
        }

        public Pallete Root
        {
            get { return _root; }
        }

        public ColTable Parent
        {
            get { return _parent; }
        }
    }

    public class ColTable : KaitaiStruct
    {
        public ColTable(ushort index, KaitaiStream io, Pallete parent = null, Pallete root = null) : base(io)
        {
            _parent = parent;
            _root = root;
            _index = index;
            _read();
        }

        private void _read()
        {
            _size1 = m_io.ReadU1();
            _unkn = m_io.ReadU1();
            _title = System.Text.Encoding.GetEncoding("UTF-16LE")
                .GetString(m_io.ReadBytes(Root.MetaValue[Index].NameSize));
            _unkn1 = m_io.ReadU2be();
            _colors = new List<Color>();
            for (var i = 0; i < (Root.MetaValue[Index].ColorsCount - 1); i++)
            {
                _colors.Add(new Color(m_io, this, _root));
            }
        }

        private byte _size1;
        private byte _unkn;
        private string _title;
        private ushort _unkn1;
        private List<Color> _colors;
        private ushort _index;
        private Pallete _root;
        private Pallete _parent;

        public byte Size1
        {
            get { return _size1; }
        }

        public byte Unkn
        {
            get { return _unkn; }
        }

        public string Title
        {
            get { return _title; }
        }

        public ushort Unkn1
        {
            get { return _unkn1; }
        }

        public List<Color> Colors
        {
            get { return _colors; }
        }

        public ushort Index
        {
            get { return _index; }
        }

        public Pallete Root
        {
            get { return _root; }
        }

        public Pallete Parent
        {
            get { return _parent; }
        }
    }

    private byte[] _signature;
    private uint _count;
    private List<Meta> _metaValue;
    private byte[] _something2;
    private List<ColTable> _tables;
    private Pallete _root;
    private KaitaiStruct _parent;

    public byte[] Signature
    {
        get { return _signature; }
    }

    public uint Count
    {
        get { return _count; }
    }

    public List<Meta> MetaValue
    {
        get { return _metaValue; }
    }

    public byte[] Something2
    {
        get { return _something2; }
    }

    public List<ColTable> Tables
    {
        get { return _tables; }
    }

    public Pallete Root
    {
        get { return _root; }
    }

    public KaitaiStruct Parent
    {
        get { return _parent; }
    }
}