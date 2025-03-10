using System.Text.Json;
using System.Text.Json.Serialization;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.ViewModels;

public abstract class StatefulViewModelBase(string fileName) : ViewModelBase
{
    [JsonIgnore]
    protected bool IsDeserializing { get; private set; }
    
    [JsonIgnore]
    protected bool IsDeserialized { get; private set; }

    protected bool ValidateState() => 
        File.Exists(fileName) 
        && File.ReadAllText(fileName).Length != 0;
    
    protected async Task Save(object obj)
    {
        try
        {
            if (Path.GetDirectoryName(fileName) is {} dir && !Directory.Exists(dir)) 
                Directory.CreateDirectory(dir);
            
            await File.WriteAllTextAsync(fileName, JsonSerializer.Serialize(obj));
        }
        catch (Exception e)
        {
            Console.WriteLine($"{GetType().Name} save failed: {e.Message}");
        }
    }

    public async Task Load()
    {
        try
        {
            IsDeserializing = true;

            if(!ValidateState())
                return;
            
            await using var fileStream = new FileStream(fileName, FileMode.OpenOrCreate);
            var obj = await JsonSerializer.DeserializeAsync(fileStream, GetType());
            
            foreach (var propertyInfo in GetType().GetProperties())
            {
                if(!propertyInfo.CanWrite)
                    continue;

                var propertyValue = propertyInfo.GetValue(obj);
            
                propertyInfo.SetValue(this, propertyValue);
            }

            IsDeserialized = true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"{GetType().Name} load failed: {e.Message}");
        }
        finally
        {
            IsDeserializing = false;
        }
    }
}