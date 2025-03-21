using NEXUS.MDT.Enums;
using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Vars : KaitaiStruct
        {
            public static Vars FromFile(string fileName)
            {
                return new Vars(new KaitaiStream(fileName));
            }

            public Vars(KaitaiStream p__io, Frame.FdScanned p__parent = null, NtMdt p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }

            private void _read()
            {
                _xScale = new AxisScale(_stream, this, m_root);
                _yScale = new AxisScale(_stream, this, m_root);
                _zScale = new AxisScale(_stream, this, m_root);
                _channelIndex = ((AdcMode)_stream.ReadU1());
                _mode = ((Frame.FdScanned.Mode)_stream.ReadU1());
                _xres = _stream.ReadU2Le();
                _yres = _stream.ReadU2Le();
                _ndacq = _stream.ReadU2Le();
                _stepLength = _stream.ReadF4Le();
                _adt = _stream.ReadU2Le();
                _adcGainAmpLog10 = _stream.ReadU1();
                _adcIndex = _stream.ReadU1();
                _inputSignalOrVersion = _stream.ReadU1();
                _substrPlaneOrderOrPassNum = _stream.ReadU1();
                _scanDir = new FdScanned.ScanDir(_stream, this, m_root);
                _powerOf2 = _stream.ReadU1();
                _velocity = _stream.ReadF4Le();
                _setpoint = _stream.ReadF4Le();
                _biasVoltage = _stream.ReadF4Le();
                _draw = _stream.ReadU1();
                _reserved = _stream.ReadU1();
                _xoff = _stream.ReadS4Le();
                _yoff = _stream.ReadS4Le();
                _nlCorr = _stream.ReadU1();
            }

            private AxisScale _xScale;
            private AxisScale _yScale;
            private AxisScale _zScale;
            private AdcMode _channelIndex;
            private Mode _mode;
            private ushort _xres;
            private ushort _yres;
            private ushort _ndacq;
            private float _stepLength;
            private ushort _adt;
            private byte _adcGainAmpLog10;
            private byte _adcIndex;
            private byte _inputSignalOrVersion;
            private byte _substrPlaneOrderOrPassNum;
            private FdScanned.ScanDir _scanDir;
            private byte _powerOf2;
            private float _velocity;
            private float _setpoint;
            private float _biasVoltage;
            private byte _draw;
            private byte _reserved;
            private int _xoff;
            private int _yoff;
            private byte _nlCorr;
            private NtMdt m_root;
            private Frame.FdScanned m_parent;

            public AxisScale XScale
            {
                get { return _xScale; }
            }

            public AxisScale YScale
            {
                get { return _yScale; }
            }

            public AxisScale ZScale
            {
                get { return _zScale; }
            }

            /// <summary>
            /// s_mode
            /// </summary>
            public AdcMode ChannelIndex
            {
                get { return _channelIndex; }
            }

            /// <summary>
            /// s_dev
            /// </summary>
            public Mode Mode
            {
                get { return _mode; }
            }

            /// <summary>
            /// s_nx
            /// </summary>
            public ushort Xres
            {
                get { return _xres; }
            }

            /// <summary>
            /// s_ny
            /// </summary>
            public ushort Yres
            {
                get { return _yres; }
            }

            /// <summary>
            /// Step (DAC)
            /// </summary>
            public ushort Ndacq
            {
                get { return _ndacq; }
            }

            /// <summary>
            /// s_rs in Angstrom's (Angstrom*gwy_get_gfloat_le(&amp;p))
            /// </summary>
            public float StepLength
            {
                get { return _stepLength; }
            }

            /// <summary>
            /// s_adt
            /// </summary>
            public ushort Adt
            {
                get { return _adt; }
            }

            /// <summary>
            /// s_adc_a
            /// </summary>
            public byte AdcGainAmpLog10
            {
                get { return _adcGainAmpLog10; }
            }

            /// <summary>
            /// ADC index
            /// </summary>
            public byte AdcIndex
            {
                get { return _adcIndex; }
            }

            /// <summary>
            /// MDTInputSignal smp_in; s_smp_in (for signal) s_8xx (for version)
            /// </summary>
            public byte InputSignalOrVersion
            {
                get { return _inputSignalOrVersion; }
            }

            /// <summary>
            /// s_spl or z_03
            /// </summary>
            public byte SubstrPlaneOrderOrPassNum
            {
                get { return _substrPlaneOrderOrPassNum; }
            }

            /// <summary>
            /// s_xy TODO: interpretation
            /// </summary>
            public FdScanned.ScanDir ScanDir
            {
                get { return _scanDir; }
            }

            /// <summary>
            /// s_2n (bool)
            /// </summary>
            public byte PowerOf2
            {
                get { return _powerOf2; }
            }

            /// <summary>
            /// s_vel (Angstrom/second)
            /// </summary>
            public float Velocity
            {
                get { return _velocity; }
            }

            /// <summary>
            /// s_i0
            /// </summary>
            public float Setpoint
            {
                get { return _setpoint; }
            }

            /// <summary>
            /// s_ut
            /// </summary>
            public float BiasVoltage
            {
                get { return _biasVoltage; }
            }

            /// <summary>
            /// s_draw (bool)
            /// </summary>
            public byte Draw
            {
                get { return _draw; }
            }

            public byte Reserved
            {
                get { return _reserved; }
            }

            /// <summary>
            /// s_x00 (in DAC quants)
            /// </summary>
            public int Xoff
            {
                get { return _xoff; }
            }

            /// <summary>
            /// s_y00 (in DAC quants)
            /// </summary>
            public int Yoff
            {
                get { return _yoff; }
            }

            /// <summary>
            /// s_cor (bool)
            /// </summary>
            public byte NlCorr
            {
                get { return _nlCorr; }
            }

            public NtMdt M_Root
            {
                get { return m_root; }
            }

            public Frame.FdScanned M_Parent
            {
                get { return m_parent; }
            }
        }