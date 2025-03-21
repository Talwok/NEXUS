using NEXUS.MDT.Enums;
using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Calibration : KaitaiStruct
        {
            public static Calibration FromFile(string fileName)
            {
                return new Calibration(new KaitaiStream(fileName));
            }

            public Calibration(KaitaiStream p__io, Frame.FdMetaData p__parent = null, NtMdt p__root = null) :
                base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_count = false;
                _read();
            }

            private void _read()
            {
                _lenTot = _stream.ReadU4Le();
                _lenStruct = _stream.ReadU4Le();
                _lenName = _stream.ReadU4Le();
                _lenComment = _stream.ReadU4Le();
                _lenUnit = _stream.ReadU4Le();
                _siUnit = _stream.ReadU8le();
                _accuracy = _stream.ReadF8Le();
                _functionIdAndDimensions = _stream.ReadU8le();
                _bias = _stream.ReadF8Le();
                _scale = _stream.ReadF8Le();
                _minIndex = _stream.ReadU8le();
                _maxIndex = _stream.ReadU8le();
                _dataType = ((DataType)_stream.ReadS4Le());
                _lenAuthor = _stream.ReadU4Le();
                _name = System.Text.Encoding.GetEncoding("utf-8").GetString(_stream.ReadBytes(LenName));
                _comment = System.Text.Encoding.GetEncoding("utf-8").GetString(_stream.ReadBytes(LenComment));
                _unit = System.Text.Encoding.GetEncoding("utf-8").GetString(_stream.ReadBytes(LenUnit));
                _author = System.Text.Encoding.GetEncoding("utf-8").GetString(_stream.ReadBytes(LenAuthor));
            }

            private bool f_count;
            private int _count;

            public int Count
            {
                get
                {
                    if (f_count)
                        return _count;
                    _count = (int)(((MaxIndex - MinIndex) + 1));
                    f_count = true;
                    return _count;
                }
            }

            private uint _lenTot;
            private uint _lenStruct;
            private uint _lenName;
            private uint _lenComment;
            private uint _lenUnit;
            private ulong _siUnit;
            private double _accuracy;
            private ulong _functionIdAndDimensions;
            private double _bias;
            private double _scale;
            private ulong _minIndex;
            private ulong _maxIndex;
            private DataType _dataType;
            private uint _lenAuthor;
            private string _name;
            private string _comment;
            private string _unit;
            private string _author;
            private NtMdt m_root;
            private Frame.FdMetaData m_parent;

            public uint LenTot
            {
                get { return _lenTot; }
            }

            public uint LenStruct
            {
                get { return _lenStruct; }
            }

            public uint LenName
            {
                get { return _lenName; }
            }

            public uint LenComment
            {
                get { return _lenComment; }
            }

            public uint LenUnit
            {
                get { return _lenUnit; }
            }

            public ulong SiUnit
            {
                get { return _siUnit; }
            }

            public double Accuracy
            {
                get { return _accuracy; }
            }

            public ulong FunctionIdAndDimensions
            {
                get { return _functionIdAndDimensions; }
            }

            public double Bias
            {
                get { return _bias; }
            }

            public double Scale
            {
                get { return _scale; }
            }

            public ulong MinIndex
            {
                get { return _minIndex; }
            }

            public ulong MaxIndex
            {
                get { return _maxIndex; }
            }

            public DataType DataType
            {
                get { return _dataType; }
            }

            public uint LenAuthor
            {
                get { return _lenAuthor; }
            }

            public string Name
            {
                get { return _name; }
            }

            public string Comment
            {
                get { return _comment; }
            }

            public string Unit
            {
                get { return _unit; }
            }

            public string Author
            {
                get { return _author; }
            }

            public NtMdt M_Root
            {
                get { return m_root; }
            }

            public Frame.FdMetaData M_Parent
            {
                get { return m_parent; }
            }
        }