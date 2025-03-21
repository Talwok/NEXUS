using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Dots : KaitaiStruct
    {
        public static Dots FromFile(string fileName)
        {
            return new Dots(new KaitaiStream(fileName));
        }

        public Dots(KaitaiStream p__io, KaitaiStruct p__parent = null, NtMdt p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root;
            _read();
        }

        private void _read()
        {
            _fmNdots = _stream.ReadU2Le();
            if (FmNdots > 0)
            {
                _coordHeader = new DotsHeader(_stream, this, m_root);
            }

            _coordinates = new List<DotsData>();
            for (var i = 0; i < FmNdots; i++)
            {
                _coordinates.Add(new DotsData(_stream, this, m_root));
            }

            _data = new List<DataLinez>();
            for (var i = 0; i < FmNdots; i++)
            {
                _data.Add(new DataLinez(i, _stream, this, m_root));
            }
        }

        
        public partial class DotsData : KaitaiStruct
        {
            public static DotsData FromFile(string fileName)
            {
                return new DotsData(new KaitaiStream(fileName));
            }

            public DotsData(KaitaiStream p__io, Frame.Dots p__parent = null, NtMdt p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }

            private void _read()
            {
                _coordX = _stream.ReadF4Le();
                _coordY = _stream.ReadF4Le();
                _forwardSize = _stream.ReadS4Le();
                _backwardSize = _stream.ReadS4Le();
            }

            private float _coordX;
            private float _coordY;
            private int _forwardSize;
            private int _backwardSize;
            private NtMdt m_root;
            private Frame.Dots m_parent;

            public float CoordX
            {
                get { return _coordX; }
            }

            public float CoordY
            {
                get { return _coordY; }
            }

            public int ForwardSize
            {
                get { return _forwardSize; }
            }

            public int BackwardSize
            {
                get { return _backwardSize; }
            }

            public NtMdt M_Root
            {
                get { return m_root; }
            }

            public Frame.Dots M_Parent
            {
                get { return m_parent; }
            }
        }

        public partial class DataLinez : KaitaiStruct
        {
            public DataLinez(ushort p_index, KaitaiStream p__io, Frame.Dots p__parent = null,
                NtMdt p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _index = p_index;
                _read();
            }

            private void _read()
            {
                _forward = new List<short>();
                for (var i = 0; i < M_Parent.Coordinates[Index].ForwardSize; i++)
                {
                    _forward.Add(_stream.ReadS2Le());
                }

                _backward = new List<short>();
                for (var i = 0; i < M_Parent.Coordinates[Index].BackwardSize; i++)
                {
                    _backward.Add(_stream.ReadS2Le());
                }
            }

            private List<short> _forward;
            private List<short> _backward;
            private ushort _index;
            private NtMdt m_root;
            private Frame.Dots m_parent;

            public List<short> Forward
            {
                get { return _forward; }
            }

            public List<short> Backward
            {
                get { return _backward; }
            }

            public ushort Index
            {
                get { return _index; }
            }

            public NtMdt M_Root
            {
                get { return m_root; }
            }

            public Frame.Dots M_Parent
            {
                get { return m_parent; }
            }
        }

        private ushort _fmNdots;
        private DotsHeader _coordHeader;
        private List<DotsData> _coordinates;
        private List<DataLinez> _data;
        private NtMdt m_root;
        private KaitaiStruct m_parent;

        public ushort FmNdots
        {
            get { return _fmNdots; }
        }

        public DotsHeader CoordHeader
        {
            get { return _coordHeader; }
        }

        public List<DotsData> Coordinates
        {
            get { return _coordinates; }
        }

        public List<DataLinez> Data
        {
            get { return _data; }
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
