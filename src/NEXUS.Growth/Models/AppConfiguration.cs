using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace NEXUS.Growth.Models;

[XmlRoot("app-configuration")]
public class AppConfiguration
{
    [XmlElement("is-startup-pane-opened")]
    public bool IsStartupPaneOpened { get; set; }
    
    [XmlElement("is-startup-auto-folder-naming")]
    public bool IsStartupAutoFolderNaming { get; set; }

    [XmlElement("recent-folders")]
    public HashSet<string> RecentFolders { get; set; } = [];
}

public static class AppConfigurationExtensions
{
    private static string AppConfigurationFolderName = "Assets";
    private static string AppConfigurationFileName = "app-config.xml";
    
    private static void SerializeToXml(this AppConfiguration config, string filePath)
    {
        var serializer = new XmlSerializer(typeof(AppConfiguration));
        using var writer = new StreamWriter(filePath);
        serializer.Serialize(writer, config);
    }

    private static AppConfiguration? DeserializeFromXml(string filePath)
    {
        var serializer = new XmlSerializer(typeof(AppConfiguration));
        using var reader = new StreamReader(filePath);
        return (AppConfiguration?)serializer.Deserialize(reader);
    }

    public static AppConfiguration? DeserializeDefaultsFromXml()
    {
        var filePath = Path.Combine(AppConfigurationFolderName, AppConfigurationFileName);
        
        return !File.Exists(filePath) ? null : DeserializeFromXml(filePath);
    }
    
    public static void SerializeDefaultsToXml(this AppConfiguration options)
    {
        var filePath = Path.Combine(AppConfigurationFolderName, AppConfigurationFileName);
        options.SerializeToXml(filePath);
    }
}