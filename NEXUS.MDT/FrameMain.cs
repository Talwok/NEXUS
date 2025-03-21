using NEXUS.MDT.Enums;
using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class FrameMain : KaitaiStruct
    {
        public static FrameMain FromFile(string fileName)
        {
            return new FrameMain(new KaitaiStream(fileName));
        }

        public FrameMain(KaitaiStream p__io, Frame p__parent = null, NtMdt p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root;
            _read();
        }

        private void _read()
        {
            _type = ((Frame.FrameType)_stream.ReadU2Le());
            _version = new Version(_stream, this, m_root);
            _dateTime = new DateTime(_stream, this, m_root);
            _varSize = _stream.ReadU2Le();
            switch (Type)
            {
                case Frame.FrameType.Mda:
                {
                    __raw_frameData = _stream.ReadBytesFull();
                    var io___raw_frameData = new KaitaiStream(__raw_frameData);
                    _frameData = new Frame.FdMetaData(io___raw_frameData, this, m_root);
                    break;
                }
                case Frame.FrameType.CurvesNew:
                {
                    __raw_frameData = _stream.ReadBytesFull();
                    var io___raw_frameData = new KaitaiStream(__raw_frameData);
                    _frameData = new Frame.FdCurvesNew(io___raw_frameData, this, m_root);
                    break;
                }
                case Frame.FrameType.Curves:
                {
                    __raw_frameData = _stream.ReadBytesFull();
                    var io___raw_frameData = new KaitaiStream(__raw_frameData);
                    _frameData = new Frame.FdSpectroscopy(io___raw_frameData, this, m_root);
                    break;
                }
                case Frame.FrameType.Spectroscopy:
                {
                    __raw_frameData = _stream.ReadBytesFull();
                    var io___raw_frameData = new KaitaiStream(__raw_frameData);
                    _frameData = new Frame.FdSpectroscopy(io___raw_frameData, this, m_root);
                    break;
                }
                case Frame.FrameType.Scanned:
                {
                    __raw_frameData = _stream.ReadBytesFull();
                    var io___raw_frameData = new KaitaiStream(__raw_frameData);
                    _frameData = new Frame.FdScanned(io___raw_frameData, this, m_root);
                    break;
                }
                default:
                {
                    _frameData = _stream.ReadBytesFull();
                    break;
                }
            }
        }

        private FrameType _type;
        private Version _version;
        private DateTime _dateTime;
        private ushort _varSize;
        private object _frameData;
        private NtMdt m_root;
        private Frame m_parent;
        private byte[] __raw_frameData;

        /// <summary>
        /// h_what
        /// </summary>
        public FrameType Type
        {
            get { return _type; }
        }

        public Version Version
        {
            get { return _version; }
        }

        public DateTime DateTime
        {
            get { return _dateTime; }
        }

        /// <summary>
        /// h_am, v6 and older only
        /// </summary>
        public ushort VarSize
        {
            get { return _varSize; }
        }

        /// <summary>
        /// 
        /// </summary>
        public object FrameData
        {
            get { return _frameData; }
        }

        public NtMdt M_Root
        {
            get { return m_root; }
        }

        public Frame M_Parent
        {
            get { return m_parent; }
        }

        public byte[] M_RawFrameData
        {
            get { return __raw_frameData; }
        }
    }