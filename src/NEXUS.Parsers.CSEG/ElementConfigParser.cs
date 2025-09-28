using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;
using NEXUS.Parsers.CSEG.Models.Elements;

namespace NEXUS.Parsers.CSEG;

public class ElementConfigParser
{
    public static ElementConfig Parse(string filePath)
    {
        var config = new ElementConfig();
        var currentNamespace = "";
            
        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
                
            // Пропускаем пустые строки и комментарии
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            // Проверяем, является ли строка объявлением namespace
            if (trimmedLine.Contains(".") && trimmedLine.EndsWith("="))
            {
                currentNamespace = trimmedLine.Split('.')[0];
                continue;
            }

            // Обрабатываем строки с данными
            if (trimmedLine.Contains("="))
            {
                ProcessLine(trimmedLine, currentNamespace, config);
            }
        }

        return config;
    }

    public static void Save(string filePath, ElementConfig config)
    {
        List<string> lines = [];
        
        // Global parameters
        AddPropertiesToLines(lines, config.Global, "Global");
        lines.Add("");
        
        // TightBinding parameters
        AddPropertiesToLines(lines, config.TightBinding, "TightBinding");
        lines.Add("");
        
        File.WriteAllLines(filePath, lines);
    }
        
    private static void ProcessLine(string line, string currentNamespace, ElementConfig config)
    {
        var parts = line.Split('=');
        if (parts.Length != 2) return;

        var leftPart = parts[0].Trim();
        var value = parts[1].Trim();

        // Если namespace указан в строке (например, "Global.Title")
        if (leftPart.Contains('.'))
        {
            var namespaceParts = leftPart.Split('.');
            currentNamespace = namespaceParts[0];
            leftPart = namespaceParts[1];
        }

        SetPropertyValue(config, currentNamespace, leftPart, value);
    }

    private static void SetPropertyValue(ElementConfig config, string namespaceName, string propertyName, string value)
    {
        try
        {
            object? targetObject = namespaceName switch
            {
                GlobalParameters.Namespace => config.Global,
                TightBindingParameters.Namespace => config.TightBinding,
                _ => null
            };

            if (targetObject == null) return;

            var property = FindPropertyByXmlElement(targetObject.GetType(), propertyName);
            if (property != null)
            {
                // Fixed: Use invariant culture for numeric types
                object convertedValue;
                if (property.PropertyType == typeof(decimal) || 
                    property.PropertyType == typeof(float) || 
                    property.PropertyType == typeof(double))
                {
                    convertedValue = Convert.ChangeType(value, property.PropertyType, CultureInfo.InvariantCulture);
                }
                else
                {
                    convertedValue = Convert.ChangeType(value, property.PropertyType);
                }
            
                property.SetValue(targetObject, convertedValue);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting property {namespaceName}.{propertyName}: {ex.Message}");
        }
    }

    private static PropertyInfo? FindPropertyByXmlElement(Type type, string xmlElementName)
    {
        return type.GetProperties()
            .FirstOrDefault(prop =>
            {
                var xmlAttr = prop.GetCustomAttribute<XmlElementAttribute>();
                return xmlAttr?.ElementName == xmlElementName;
            });
    }
        
    private static void AddPropertiesToLines(List<string> lines, object obj, string namespaceName)
    {
        var properties = obj.GetType().GetProperties()
            .Where(p => p.GetCustomAttribute<XmlElementAttribute>() != null)
            .OrderBy(p => p.Name);

        lines.AddRange(from prop in properties let xmlAttr = prop.GetCustomAttribute<XmlElementAttribute>() let value = prop.GetValue(obj) select $"{namespaceName}.{xmlAttr.ElementName} = {value}");
    }
}