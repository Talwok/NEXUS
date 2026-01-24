using System.Reflection;
using System.Xml.Serialization;

namespace NEXUS.Parsers.Cseg.Helper;

public static class XmlAttributeHelper
{
    public static string GetXmlElementName(this PropertyInfo propertyInfo)
    {
        var attribute = propertyInfo.GetCustomAttribute<XmlElementAttribute>();
        return attribute?.ElementName ?? propertyInfo.Name;
    }

    public static Dictionary<string, string> GetPropertyMappings(Type type)
    {
        return type.GetProperties()
            .Where(p => p.GetCustomAttribute<XmlElementAttribute>() != null)
            .ToDictionary(
                p => p.GetXmlElementName(),
                p => p.Name
            );
    }
}