using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class ScanDir : KaitaiStruct
        {
            public static ScanDir FromFile(string fileName)
            {
                return new ScanDir(new KaitaiStream(fileName));
            }

            public ScanDir(KaitaiStream p__io, Frame.FdScanned.Vars p__parent = null,
                NtMdt p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }

            private void _read()
            {
                _unkn = _stream.ReadBitsIntBe(4);
                _doublePass = _stream.ReadBitsIntBe(1) != 0;
                _bottom = _stream.ReadBitsIntBe(1) != 0;
                _left = _stream.ReadBitsIntBe(1) != 0;
                _horizontal = _stream.ReadBitsIntBe(1) != 0;
            }

            private ulong _unkn;
            private bool _doublePass;
            private bool _bottom;
            private bool _left;
            private bool _horizontal;
            private NtMdt m_root;
            private Frame.FdScanned.Vars m_parent;

            public ulong Unkn
            {
                get { return _unkn; }
            }

            public bool DoublePass
            {
                get { return _doublePass; }
            }

            /// <summary>
            /// Bottom - 1 Top - 0
            /// </summary>
            public bool Bottom
            {
                get { return _bottom; }
            }

            /// <summary>
            /// Left - 1 Right - 0
            /// </summary>
            public bool Left
            {
                get { return _left; }
            }

            /// <summary>
            /// Horizontal - 1 Vertical - 0
            /// </summary>
            public bool Horizontal
            {
                get { return _horizontal; }
            }

            public NtMdt M_Root
            {
                get { return m_root; }
            }

            public Frame.FdScanned.Vars M_Parent
            {
                get { return m_parent; }
            }
        }