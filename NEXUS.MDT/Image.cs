using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Image : KaitaiStruct
        {
            public static Image FromFile(string fileName)
            {
                return new Image(new KaitaiStream(fileName));
            }

            public Image(KaitaiStream p__io, Frame.FdMetaData p__parent = null, NtMdt p__root = null) :
                base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }

            private void _read()
            {
                _image = new List<Vec>();
                {
                    var i = 0;
                    while (!_stream.IsEof)
                    {
                        _image.Add(new Vec(_stream, this, m_root));
                        i++;
                    }
                }
            }

            
            private List<Vec> _image;
            private NtMdt m_root;
            private Frame.FdMetaData m_parent;

            public List<Vec> Image
            {
                get { return _image; }
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