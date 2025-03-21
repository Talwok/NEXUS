using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class FdScanned : KaitaiStruct
    {
        public static FdScanned FromFile(string fileName)
        {
            return new FdScanned(new KaitaiStream(fileName));
        }
        
        public FdScanned(KaitaiStream p__io, Frame.FrameMain p__parent = null, NtMdt p__root = null) : base(p__io)
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
            if (false)
            {
                _origFormat = _stream.ReadU4Le();
            }

            if (false)
            {
                _tune = ((LiftMode)_stream.ReadU4Le());
            }

            if (false)
            {
                _feedbackGain = _stream.ReadF8Le();
            }

            if (false)
            {
                _dacScale = _stream.ReadS4Le();
            }

            if (false)
            {
                _overscan = _stream.ReadS4Le();
            }

            _fmMode = _stream.ReadU2Le();
            _fmXres = _stream.ReadU2Le();
            _fmYres = _stream.ReadU2Le();
            _dots = new Dots(_stream, this, m_root);
            _image = new List<short>();
            for (var i = 0; i < (FmXres * FmYres); i++)
            {
                _image.Add(_stream.ReadS2Le());
            }

            _title = new Title(_stream, this, m_root);
            _xml = new Xml(_stream, this, m_root);
        }

        

        

        

        private Vars _vars;
        private uint? _origFormat;
        private LiftMode _tune;
        private double? _feedbackGain;
        private int? _dacScale;
        private int? _overscan;
        private ushort _fmMode;
        private ushort _fmXres;
        private ushort _fmYres;
        private Dots _dots;
        private List<short> _image;
        private Title _title;
        private Xml _xml;
        private NtMdt m_root;
        private Frame.FrameMain m_parent;
        private byte[] __raw_vars;

        public Vars Vars
        {
            get { return _vars; }
        }

        /// <summary>
        /// s_oem
        /// </summary>
        public uint? OrigFormat
        {
            get { return _origFormat; }
        }

        /// <summary>
        /// z_tune
        /// </summary>
        public LiftMode Tune
        {
            get { return _tune; }
        }

        /// <summary>
        /// s_fbg
        /// </summary>
        public double? FeedbackGain
        {
            get { return _feedbackGain; }
        }

        /// <summary>
        /// s_s
        /// </summary>
        public int? DacScale
        {
            get { return _dacScale; }
        }

        /// <summary>
        /// s_xov (in %)
        /// </summary>
        public int? Overscan
        {
            get { return _overscan; }
        }

        /// <summary>
        /// m_mode
        /// </summary>
        public ushort FmMode
        {
            get { return _fmMode; }
        }

        /// <summary>
        /// m_nx
        /// </summary>
        public ushort FmXres
        {
            get { return _fmXres; }
        }

        /// <summary>
        /// m_ny
        /// </summary>
        public ushort FmYres
        {
            get { return _fmYres; }
        }

        public Dots Dots
        {
            get { return _dots; }
        }

        public List<short> Image
        {
            get { return _image; }
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