using System.Diagnostics;
using Kaitai;

namespace NEXUS.Parsers.MDT.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    [TestCase("Fe_#1_Th=65nm_2024.03.31_0.mdt")]
    [TestCase("Fe_#2_Th=80nm_2024.03.21_0.mdt")]
    [TestCase("Fe_#1_Th=65nm_2024.04.04_0.mdt")]
    public void LoadingTest(string mdtFileName)
    {
        var sw = Stopwatch.StartNew();
        var data = new Mdt(new KaitaiStream(File.ReadAllBytes(mdtFileName)));
        sw.Stop();
        Assert.Pass();
    }
}