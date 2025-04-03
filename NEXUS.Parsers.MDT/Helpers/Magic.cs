namespace NEXUS.Parsers.MDT.Helpers;

internal static class Magic
{
    public const int FileMagicHeader = 0x01b093ff;
    public const int FileHeaderSize = 32;
    public const int FrameHeaderSize = 22;
    public const int FrameModeSize = 8;
    public const int AxisScalesSize = 30;
    public const int ScanVarsMinSize = 77;
    public const int SpectroVarsMinSize = 38;


    public static readonly byte[] PalleteSignature = "NT-MDT Palette File  1.00!"u8.ToArray();
}