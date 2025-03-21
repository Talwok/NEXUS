using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class FdSpectroscopy : KaitaiStruct
    {
        public static FdSpectroscopy FromFile(string fileName)
        {
            return new FdSpectroscopy(new KaitaiStream(fileName));
        }

        public FdSpectroscopy(KaitaiStream p__io, Frame.FrameMain p__parent = null, NtMdt p__root = null) :
            base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root;
            _read();
        }

        private void _read()
        {
            __raw_vars = _stream.ReadBytes(M_Parent.VarSize);
            var io___raw_vars = new KaitaiStream(__raw_vars);
            _vars = new Vars(io___raw_vars, this, m_root);
            _fmMode = _stream.ReadU2Le();
            _fmXres = _stream.ReadU2Le();
            _fmYres = _stream.ReadU2Le();
            _dots = new Dots(_stream, this, m_root);
            _data = new List<short>();
            for (var i = 0; i < (FmXres * FmYres); i++)
            {
                _data.Add(_stream.ReadS2Le());
            }

            _title = new Title(_stream, this, m_root);
            _xml = new Xml(_stream, this, m_root);
        }

        public partial class Vars : KaitaiStruct
        {
            public static Vars FromFile(string fileName)
            {
                return new Vars(new KaitaiStream(fileName));
            }

            public Vars(KaitaiStream p__io, Frame.FdSpectroscopy p__parent = null, NtMdt p__root = null) :
                base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }

            private void _read()
            {
                _xScale = new Frame.AxisScale(_stream, this, m_root);
                _yScale = new Frame.AxisScale(_stream, this, m_root);
                _zScale = new Frame.AxisScale(_stream, this, m_root);
                _spMode = _stream.ReadU2Le();
                _spFilter = _stream.ReadU2Le();
                _uBegin = _stream.ReadF4Le();
                _uEnd = _stream.ReadF4Le();
                _zUp = _stream.ReadS2Le();
                _zDown = _stream.ReadS2Le();
                _spAveraging = _stream.ReadU2Le();
                _spRepeat = _stream.ReadU1();
                _spBack = _stream.ReadU1();
                _sp4nx = _stream.ReadS2Le();
                _spOsc = _stream.ReadU1();
                _spN4 = _stream.ReadU1();
                _sp4x0 = _stream.ReadF4Le();
                _sp4xr = _stream.ReadF4Le();
                _sp4u = _stream.ReadS2Le();
                _sp4i = _stream.ReadS2Le();
                _spNx = _stream.ReadS2Le();
            }

            private Frame.AxisScale _xScale;
            private Frame.AxisScale _yScale;
            private Frame.AxisScale _zScale;
            private ushort _spMode;
            private ushort _spFilter;
            private float _uBegin;
            private float _uEnd;
            private short _zUp;
            private short _zDown;
            private ushort _spAveraging;
            private byte _spRepeat;
            private byte _spBack;
            private short _sp4nx;
            private byte _spOsc;
            private byte _spN4;
            private float _sp4x0;
            private float _sp4xr;
            private short _sp4u;
            private short _sp4i;
            private short _spNx;
            private NtMdt m_root;
            private Frame.FdSpectroscopy m_parent;

            public Frame.AxisScale XScale
            {
                get { return _xScale; }
            }

            public Frame.AxisScale YScale
            {
                get { return _yScale; }
            }

            public Frame.AxisScale ZScale
            {
                get { return _zScale; }
            }

            public ushort SpMode
            {
                get { return _spMode; }
            }

            public ushort SpFilter
            {
                get { return _spFilter; }
            }

            public float UBegin
            {
                get { return _uBegin; }
            }

            public float UEnd
            {
                get { return _uEnd; }
            }

            public short ZUp
            {
                get { return _zUp; }
            }

            public short ZDown
            {
                get { return _zDown; }
            }

            public ushort SpAveraging
            {
                get { return _spAveraging; }
            }

            public byte SpRepeat
            {
                get { return _spRepeat; }
            }

            public byte SpBack
            {
                get { return _spBack; }
            }

            public short Sp4nx
            {
                get { return _sp4nx; }
            }

            public byte SpOsc
            {
                get { return _spOsc; }
            }

            public byte SpN4
            {
                get { return _spN4; }
            }

            public float Sp4x0
            {
                get { return _sp4x0; }
            }

            public float Sp4xr
            {
                get { return _sp4xr; }
            }

            public short Sp4u
            {
                get { return _sp4u; }
            }

            public short Sp4i
            {
                get { return _sp4i; }
            }

            public short SpNx
            {
                get { return _spNx; }
            }

            public NtMdt M_Root
            {
                get { return m_root; }
            }

            public Frame.FdSpectroscopy M_Parent
            {
                get { return m_parent; }
            }
        }

        private Vars _vars;
        private ushort _fmMode;
        private ushort _fmXres;
        private ushort _fmYres;
        private Dots _dots;
        private List<short> _data;
        private Title _title;
        private Xml _xml;
        private NtMdt m_root;
        private Frame.FrameMain m_parent;
        private byte[] __raw_vars;

        public Vars Vars
        {
            get { return _vars; }
        }

        public ushort FmMode
        {
            get { return _fmMode; }
        }

        public ushort FmXres
        {
            get { return _fmXres; }
        }

        public ushort FmYres
        {
            get { return _fmYres; }
        }

        public Dots Dots
        {
            get { return _dots; }
        }

        public List<short> Data
        {
            get { return _data; }
        }

        public Title Title
        {
            get { return _title; }
        }

        public Xml Xml
        {
            get { return _xml; }
        }

        public NtMdt M_Root
        {
            get { return m_root; }
        }

        public Frame.FrameMain M_Parent
        {
            get { return m_parent; }
        }

        public byte[] M_RawVars
        {
            get { return __raw_vars; }
        }
    }