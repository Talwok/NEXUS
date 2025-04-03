using NEXUS.Parsers.MDT.Models.Pallete;

namespace NEXUS.Parsers.MDT.Models
{
    /// <summary>
    /// It is a color scheme for visualising SPM scans.
    /// </summary>
    public class PalleteFile
    {
        public byte[] Signature { get; set; }
        public uint Count { get; set; }
        public List<PalleteCollorTableMeta> MetaValue { get; } = [];
        public List<PalleteCollorTable> Tables { get; } = [];
    }
}