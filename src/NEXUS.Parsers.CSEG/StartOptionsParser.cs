using System.Xml;
using System.Xml.Serialization;
using NEXUS.Parsers.CSEG.Models.StartOptions;

namespace NEXUS.Parsers.CSEG;

public class StartOptionsParser
{
    public static StartOptions? Parse(string filePath)
    {
        var serializer = new XmlSerializer(typeof(StartOptions));

        using var reader = new StringReader(File.ReadAllText(filePath));
        using var xmlReader = XmlReader.Create(reader, new XmlReaderSettings
        {
            IgnoreWhitespace = true
        });
        
        if (serializer.Deserialize(xmlReader) is StartOptions startOptions)
            return startOptions;

        return null;
    }

    public static void Save(string filePath, StartOptions options)
    {
        var serializer = new XmlSerializer(typeof(StartOptions));
        using var writer = new StringWriter();
        using var fileStream = File.OpenWrite(filePath);
        serializer.Serialize(fileStream, options);
    }
}