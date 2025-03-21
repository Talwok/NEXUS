using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class FdMetaData : KaitaiStruct
    {
        public static FdMetaData FromFile(string fileName)
        {
            return new FdMetaData(new KaitaiStream(fileName));
        }

        public FdMetaData(KaitaiStream p__io, Frame.FrameMain p__parent = null, NtMdt p__root = null) :
            base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root;
            f_image = false;
            _read();
        }

        private void _read()
        {
            _headSize = _stream.ReadU4Le();
            _totLen = _stream.ReadU4Le();
            _guids = new List<Uuid>();
            for (var i = 0; i < 2; i++)
            {
                _guids.Add(new Uuid(_stream, this, m_root));
            }

            _frameStatus = _stream.ReadBytes(4);
            _nameSize = _stream.ReadU4Le();
            _commSize = _stream.ReadU4Le();
            _viewInfoSize = _stream.ReadU4Le();
            _specSize = _stream.ReadU4Le();
            _sourceInfoSize = _stream.ReadU4Le();
            _varSize = _stream.ReadU4Le();
            _dataOffset = _stream.ReadU4Le();
            _dataSize = _stream.ReadU4Le();
            _title = System.Text.Encoding.GetEncoding("UTF-8").GetString(_stream.ReadBytes(NameSize));
            _xml = System.Text.Encoding.GetEncoding("UTF-8").GetString(_stream.ReadBytes(CommSize));
            _structLen = _stream.ReadU4Le();
            _arraySize = _stream.ReadU8le();
            _cellSize = _stream.ReadU4Le();
            _nDimensions = _stream.ReadU4Le();
            _nMesurands = _stream.ReadU4Le();
            _dimensions = new List<Calibration>();
            for (var i = 0; i < NDimensions; i++)
            {
                _dimensions.Add(new Calibration(_stream, this, m_root));
            }

            _mesurands = new List<Calibration>();
            for (var i = 0; i < NMesurands; i++)
            {
                _mesurands.Add(new Calibration(_stream, this, m_root));
            }
        }

        

        

        private bool f_image;
        private Image _image;

        public Image Image
        {
            get
            {
                if (f_image)
                    return _image;
                long _pos = _stream.Pos;
                _stream.Seek(DataOffset);
                __raw_image = _stream.ReadBytes(DataSize);
                var io___raw_image = new KaitaiStream(__raw_image);
                _image = new Image(io___raw_image, this, m_root);
                _stream.Seek(_pos);
                f_image = true;
                return _image;
            }
        }

        private uint _headSize;
        private uint _totLen;
        private List<Uuid> _guids;
        private byte[] _frameStatus;
        private uint _nameSize;
        private uint _commSize;
        private uint _viewInfoSize;
        private uint _specSize;
        private uint _sourceInfoSize;
        private uint _varSize;
        private uint _dataOffset;
        private uint _dataSize;
        private string _title;
        private string _xml;
        private uint _structLen;
        private ulong _arraySize;
        private uint _cellSize;
        private uint _nDimensions;
        private uint _nMesurands;
        private List<Calibration> _dimensions;
        private List<Calibration> _mesurands;
        private NtMdt m_root;
        private Frame.FrameMain m_parent;
        private byte[] __raw_image;

        public uint HeadSize
        {
            get { return _headSize; }
        }

        public uint TotLen
        {
            get { return _totLen; }
        }

        public List<Uuid> Guids
        {
            get { return _guids; }
        }

        public byte[] FrameStatus
        {
            get { return _frameStatus; }
        }

        public uint NameSize
        {
            get { return _nameSize; }
        }

        public uint CommSize
        {
            get { return _commSize; }
        }

        public uint ViewInfoSize
        {
            get { return _viewInfoSize; }
        }

        public uint SpecSize
        {
            get { return _specSize; }
        }

        public uint SourceInfoSize
        {
            get { return _sourceInfoSize; }
        }

        public uint VarSize
        {
            get { return _varSize; }
        }

        public uint DataOffset
        {
            get { return _dataOffset; }
        }

        public uint DataSize
        {
            get { return _dataSize; }
        }

        public string Title
        {
            get { return _title; }
        }

        public string Xml
        {
            get { return _xml; }
        }

        public uint StructLen
        {
            get { return _structLen; }
        }

        public ulong ArraySize
        {
            get { return _arraySize; }
        }

        public uint CellSize
        {
            get { return _cellSize; }
        }

        public uint NDimensions
        {
            get { return _nDimensions; }
        }

        public uint NMesurands
        {
            get { return _nMesurands; }
        }

        public List<Calibration> Dimensions
        {
            get { return _dimensions; }
        }

        public List<Calibration> Mesurands
        {
            get { return _mesurands; }
        }

        public NtMdt M_Root
        {
            get { return m_root; }
        }

        public Frame.FrameMain M_Parent
        {
            get { return m_parent; }
        }

        public byte[] M_RawImage
        {
            get { return __raw_image; }
        }
    }
