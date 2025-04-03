using NEXUS.Parsers.MDT.Models.Frames;

namespace NEXUS.Parsers.MDT.Models;

public class MdtFile
{
    public uint Size { get; set; }
    public ushort LastFrame { get; set; }
    public List<MdtFrame> Frames { get; set; } = [];
}