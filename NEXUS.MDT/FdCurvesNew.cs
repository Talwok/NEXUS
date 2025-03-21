using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class FdCurvesNew : KaitaiStruct
    {
        public static FdCurvesNew FromFile(string fileName)
        {
            return new FdCurvesNew(new KaitaiStream(fileName));
        }

        public FdCurvesNew(KaitaiStream p__io, Frame.FrameMain p__parent = null, NtMdt p__root = null) :
            base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root;
            _read();
        }

        private void _read()
        {
            _blockCount = _stream.ReadU4Le();
            _blocksHeaders = new List<BlockDescr>();
            for (var i = 0; i < BlockCount; i++)
            {
                _blocksHeaders.Add(new BlockDescr(_stream, this, m_root));
            }

            _blocksNames = new List<string>();
            for (var i = 0; i < BlockCount; i++)
            {
                _blocksNames.Add(System.Text.Encoding.GetEncoding("UTF-8")
                    .GetString(_stream.ReadBytes(BlocksHeaders[i].NameLen)));
            }

            _blocksData = new List<byte[]>();
            for (var i = 0; i < BlockCount; i++)
            {
                _blocksData.Add(_stream.ReadBytes(BlocksHeaders[i].Len));
            }
        }

        
        private uint _blockCount;
        private List<BlockDescr> _blocksHeaders;
        private List<string> _blocksNames;
        private List<byte[]> _blocksData;
        private NtMdt m_root;
        private Frame.FrameMain m_parent;

        public uint BlockCount
        {
            get { return _blockCount; }
        }

        public List<BlockDescr> BlocksHeaders
        {
            get { return _blocksHeaders; }
        }

        public List<string> BlocksNames
        {
            get { return _blocksNames; }
        }

        public List<byte[]> BlocksData
        {
            get { return _blocksData; }
        }

        public NtMdt M_Root
        {
            get { return m_root; }
        }

        public Frame.FrameMain M_Parent
        {
            get { return m_parent; }
        }
    }