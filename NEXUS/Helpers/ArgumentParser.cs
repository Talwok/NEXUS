using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace NEXUS.Helpers;

public static class ArgumentParser
{
    public static bool TryParse<TArgs>(string? args, out TArgs parsedArgs) where TArgs : new()
    {
        parsedArgs = new TArgs();

        if (string.IsNullOrEmpty(args))
            return false;

        // Разбиваем строку на пары ключ-значение
        var keyValuePairs = ParseKeyValuePairs(args);

        if (keyValuePairs.Count == 0)
            return false;

        // Получаем свойства и поля типа TArgs
        var properties = typeof(TArgs).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var fields = typeof(TArgs).GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var kvp in keyValuePairs)
        {
            string key = kvp.Key;
            string value = kvp.Value;

            // Ищем свойство или поле с именем, соответствующим ключу
            var property = properties.FirstOrDefault(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
            var field = fields.FirstOrDefault(f => f.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (property != null)
            {
                // Преобразуем значение и устанавливаем его в свойство
                if (!SetValue(parsedArgs, property, value))
                {
                    return false;
                }
            }
            else if (field != null)
            {
                // Преобразуем значение и устанавливаем его в поле
                if (!SetValue(parsedArgs, field, value))
                {
                    return false;
                }
            }
            else
            {
                // Если ключ не соответствует ни одному свойству или полю, возвращаем false
                return false;
            }
        }

        return true;
    }

    private static Dictionary<string, string> ParseKeyValuePairs(string? args)
    {
        var result = new Dictionary<string, string>();

        // Разделяем аргументы по пробелам, учитывая кавычки
        var parts = args.Split(' ').ToList();

        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;

            // Разделяем на ключ и значение по символу '='
            var keyValue = part.Split('=');
            if (keyValue.Length != 2)
                continue;

            var key = keyValue[0].Trim().TrimStart('-'); // Убираем "--" или "-" из ключа
            var value = keyValue[1].Trim().Trim('"'); // Убираем кавычки из значения

            result[key] = value;
        }

        return result;
    }

    private static bool SetValue<TArgs>(TArgs obj, PropertyInfo property, string value)
    {
        try
        {
            // Используем TypeConverter для преобразования строки в тип свойства
            var converter = TypeDescriptor.GetConverter(property.PropertyType);
            var convertedValue = converter.ConvertFromString(null, CultureInfo.InvariantCulture, value);
            property.SetValue(obj, convertedValue);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool SetValue<TArgs>(TArgs obj, FieldInfo field, string value)
    {
        try
        {
            // Используем TypeConverter для преобразования строки в тип поля
            var converter = TypeDescriptor.GetConverter(field.FieldType);
            var convertedValue = converter.ConvertFromString(null, CultureInfo.InvariantCulture, value);
            field.SetValue(obj, convertedValue);
            return true;
        }
        catch
        {
            return false;
        }
    }
}