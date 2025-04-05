using NEXUS.Parsers.MDT.Models.Pallete;

namespace NEXUS.Parsers.MDT.Models
{
    /// <summary>
    /// It is a color scheme for visualising SPM scans.
    /// </summary>
    public class PalleteFile
    {
        public string Path { get; set; }
        public byte[] Signature { get; set; }
        public uint Count { get; set; }
        public List<PaletteCollorTableMeta> MetaValue { get; } = [];
        public List<PaletteCollorTable> Tables { get; } = [];
    }
}