using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NEXUS.Growth.ViewModels;

public abstract class StatefulViewModelBase(string fileName) : ViewModelBase
{
    [JsonIgnore]
    public bool IsDeserializing { get; private set; }

    public async Task Save(object obj)
    {
        try
        {
            await File.WriteAllTextAsync(fileName, JsonSerializer.Serialize(obj));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task Load()
    {
        try
        {
            IsDeserializing = true;
            await using var fileStream = new FileStream(fileName, FileMode.OpenOrCreate);
            var obj = await JsonSerializer.DeserializeAsync(fileStream, GetType());
            
            foreach (var propertyInfo in GetType().GetProperties())
            {
                if(!propertyInfo.CanWrite)
                    continue;

                var propertyValue = propertyInfo.GetValue(obj);
            
                propertyInfo.SetValue(this, propertyValue);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            IsDeserializing = false;
        }
    }
}