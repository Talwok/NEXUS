namespace NEXUS.Parsers.MDT.Models.Enums;

public enum MdaDataType : int
{
    Int8 = -1,
    UInt8 = 1,
    Int16 = -2,
    UInt16 = 2,
    Int32 = -4,
    UInt32 = 4,
    Int64 = -8,
    UInt64 = 8,
    Float32 = -(4 + 23 * 256),
    Float64 = -(8 + 52 * 256)
}
