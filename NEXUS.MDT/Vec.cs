using NEXUS.MDT.Enums;
using NEXUS.MDT.Misc;

namespace NEXUS.MDT;

public partial class Vec : KaitaiStruct
            {
                public static Vec FromFile(string fileName)
                {
                    return new Vec(new KaitaiStream(fileName));
                }

                public Vec(KaitaiStream p__io, Frame.FdMetaData.Image p__parent = null,
                    NtMdt p__root = null) : base(p__io)
                {
                    m_parent = p__parent;
                    m_root = p__root;
                    _read();
                }

                private void _read()
                {
                    _items = new List<double>();
                    for (var i = 0; i < M_Parent.M_Parent.NMesurands; i++)
                    {
                        switch (M_Parent.M_Parent.Mesurands[i].DataType)
                        {
                            case DataType.Uint64:
                            {
                                _items.Add(_stream.ReadU8le());
                                break;
                            }
                            case DataType.Uint8:
                            {
                                _items.Add(_stream.ReadU1());
                                break;
                            }
                            case DataType.Float32:
                            {
                                _items.Add(_stream.ReadF4Le());
                                break;
                            }
                            case DataType.Int8:
                            {
                                _items.Add(_stream.ReadS1());
                                break;
                            }
                            case DataType.Uint16:
                            {
                                _items.Add(_stream.ReadU2Le());
                                break;
                            }
                            case DataType.Int64:
                            {
                                _items.Add(_stream.ReadS8Le());
                                break;
                            }
                            case DataType.Uint32:
                            {
                                _items.Add(_stream.ReadU4Le());
                                break;
                            }
                            case DataType.Float64:
                            {
                                _items.Add(_stream.ReadF8Le());
                                break;
                            }
                            case DataType.Int16:
                            {
                                _items.Add(_stream.ReadS2Le());
                                break;
                            }
                            case DataType.Int32:
                            {
                                _items.Add(_stream.ReadS4Le());
                                break;
                            }
                        }
                    }
                }

                private List<double> _items;
                private NtMdt m_root;
                private Frame.FdMetaData.Image m_parent;

                public List<double> Items
                {
                    get { return _items; }
                }

                public NtMdt M_Root
                {
                    get { return m_root; }
                }

                public Frame.FdMetaData.Image M_Parent
                {
                    get { return m_parent; }
                }
            }
