using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class DateTime : KaitaiStruct
    {
        public static DateTime FromFile(string fileName)
        {
            return new DateTime(new KaitaiStream(fileName));
        }

        public DateTime(KaitaiStream p__io, Frame.FrameMain p__parent = null, NtMdt p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root;
            _read();
        }

        private void _read()
        {
            _date = new Date(_stream, this, m_root);
            _time = new Time(_stream, this, m_root);
        }

        private Date _date;
        private Time _time;
        private NtMdt m_root;
        private Frame.FrameMain m_parent;

        public Date Date
        {
            get { return _date; }
        }

        public Time Time
        {
            get { return _time; }
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