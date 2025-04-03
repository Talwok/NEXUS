using System.Text;
using NEXUS.Parsers.MDT.Helpers;
using NEXUS.Parsers.MDT.Models;
using NEXUS.Parsers.MDT.Models.Pallete;

namespace NEXUS.Parsers.MDT.Parsers;

public static class PalleteParser
{
    public static PalleteFile Parse(string filePath)
    {   
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream, Encoding.Default, true);
            
        var pallete = new PalleteFile
        {
            // Read signature
            Signature = reader.ReadBytes(26)
        };

        if (!CompareByteArrays(pallete.Signature, Magic.PalleteSignature))
            throw new InvalidDataException("Invalid signature");

        // Read count
        pallete.Count = reader.ReadUInt32Le();

        // Read meta values
        for (var i = 0; i < pallete.Count; i++) 
            pallete.MetaValue.Add(ReadMeta(reader));

        reader.ReadBytes(1);

        // Read color tables
        for (ushort i = 0; i < pallete.Count; i++) 
            pallete.Tables.Add(ReadColorTable(pallete, i, reader));

        return pallete;
    }
    
    private static bool CompareByteArrays(byte[] a1, byte[] a2)
    {
        if (a1.Length != a2.Length)
            return false;

        return !a1.Where((t, i) => t != a2[i]).Any();
    }
    
    private static PalleteCollorTableMeta ReadMeta(BinaryReader reader)
    {
        var meta = new PalleteCollorTableMeta();
        
        reader.ReadBytes(7);
        meta.ColorsCount = reader.ReadUInt16();
        reader.ReadBytes(5);
        meta.TitleSize = reader.ReadUInt16Le();
        
        return meta;
    }
    
    private static PalleteColor ReadColor(BinaryReader reader)
    {
        return new PalleteColor
        {
            Red = reader.ReadByte(),
            Unknown = reader.ReadByte(),
            Blue = reader.ReadByte(),
            Green = reader.ReadByte()
        };
    }

    private static PalleteCollorTable ReadColorTable(PalleteFile parent, ushort index, BinaryReader reader)
    {
        var colorTable = new PalleteCollorTable
        {
            Index = index,
            Parent = parent
        };
        
        reader.ReadBytes(2);
        
        colorTable.Title = Encoding.Unicode.GetString(reader.ReadBytes(colorTable.Parent.MetaValue[colorTable.Index].TitleSize));
                
        reader.ReadBytes(2);
        
        for (var i = 0; i < colorTable.Parent.MetaValue[colorTable.Index].ColorsCount - 1; i++) 
            colorTable.Colors.Add(ReadColor(reader));
        
        return colorTable;
    }
}