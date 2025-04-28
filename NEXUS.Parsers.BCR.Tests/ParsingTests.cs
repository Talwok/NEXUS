using System.Text;

namespace NEXUS.Parsers.BCR.Tests;

public class ParsingTests
{
    [SetUp]
    public void Setup()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
    
    [Test]
    [TestCase("Bcr/Mo80 11 10 24 (20,2).bcr")]
    [TestCase("Bcr/Mo80 11 10 24 (21,7).bcr")]
    [TestCase("Bcr/Mo80 11 10 24 (22).bcr")]
    [TestCase("Bcr/Mo80 11 10 24 (28,4).bcr")]
    [TestCase("Bcr/Mo80 12 10 24 (17,5.bcr")]
    [TestCase("Bcr/Mo80 12 10 24 (26,2).bcr")]
    [TestCase("Bcr/Mo80 12 10 24 (27,7).bcr")]
    [TestCase("Bcr/Mo80 12 10 24 (28,41).bcr")]
    [TestCase("Bcr/Mo80 12 10 24 (32).bcr")]
    [TestCase("Bcr/Fe65 22 02 2024 (184).bcr")]
    [TestCase("Bcr/Fe65 22 02 2024 (128).bcr")]
    [TestCase("Bcr/Fe65 22 02 2024 (94.1).bcr")]

    public void LoadingTest(string fileName)
    {
        var bcr = BcrParser.Parse(fileName);
        
        foreach (var keyValuePair in bcr.Metadata) 
            Console.WriteLine(keyValuePair.Key + " : " + keyValuePair.Value);
        
        // var bcr = BcrParser.Parse(fileName);
        Assert.Pass();
    }
}