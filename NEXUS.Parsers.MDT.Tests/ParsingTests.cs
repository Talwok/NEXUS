using System.Diagnostics;
using System.Text;
using Kaitai;
using NEXUS.Parsers.MDT.Parsers;

namespace NEXUS.Parsers.MDT.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    
    [Test]
    [TestCase("Palletes/Palette.pal")]
    [TestCase("Palletes/Stylish.pal")]
    [TestCase("Palletes/TrueCol.pal")]
    [TestCase("Palletes/Rainbows.pal")]
    [TestCase("Palletes/Default.pal")]
    public void PalletesParsingTest(string fileName)
    {
        var pallete = PalleteParser.Parse(fileName);
        
        Console.WriteLine($"################################################################");
        Console.WriteLine($"Palletes signature: {Encoding.UTF8.GetString(pallete.Signature)};");
        Console.WriteLine($"Color tables count: {pallete.Count};");
        Console.WriteLine($"################################################################");

        for (var i = 0; i < pallete.MetaValue.Count; i++)
        {
            var colorTable = pallete.Tables[i];
            var metaValue = pallete.MetaValue[i];
            Console.WriteLine($"----------------------------------------------------------------");
            Console.WriteLine($"Color table index: {colorTable.Index};");
            Console.WriteLine($"Color table title size: {metaValue.TitleSize};");
            Console.WriteLine($"Color table title: {colorTable.Title};");
            Console.WriteLine($"Color table colors count: {metaValue.ColorsCount};");
            Console.WriteLine($"----------------------------------------------------------------");
        }

        Assert.Pass();
    }
    
    [Test]
    [TestCase("Mdt/Fe_#1_Th=65nm_2024.03.31_0.mdt")]
    [TestCase("Mdt/Fe_#2_Th=80nm_2024.03.21_0.mdt")]
    [TestCase("Mdt/Fe_#1_Th=65nm_2024.04.04_0.mdt")]
    public void LoadingTest(string fileName)
    {
        var mdt = MdtParser.Parse(fileName);
        Assert.Pass();
    }
}