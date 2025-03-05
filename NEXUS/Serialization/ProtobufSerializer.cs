using ProtoBuf;

namespace NEXUS.Serialization;

public class ProtobufSerializer<T>
{
    /// Проверка, что тип T имеет атрибут ProtoContract
    static ProtobufSerializer()
    {
        if (typeof(T).GetCustomAttributes(typeof(ProtoContractAttribute), inherit: true).Length == 0)
        {
            throw new InvalidOperationException($"Тип {typeof(T).Name} не имеет атрибута [ProtoContract].");
        }
    }

    /// Асинхронная сериализация объекта в массив байтов
    public async Task<byte[]> SerializeAsync(T obj, CancellationToken token = default)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj), "Объект для сериализации не может быть null.");
        }

        using (var memoryStream = new MemoryStream())
        {
            // Сериализация в поток
            Serializer.Serialize(memoryStream, obj);
            return memoryStream.ToArray(); // Просто возвращаем массив байтов
        }
    }

    /// Асинхронная десериализация массива байтов в объект
    public async Task<T> DeserializeAsync(byte[] data, CancellationToken token = default)
    {
        if (data == null || data.Length == 0)
        {
            throw new ArgumentNullException(nameof(data), "Данные для десериализации не могут быть null или пустыми.");
        }

        using (var memoryStream = new MemoryStream(data))
        {
            // Десериализация из потока
            return Serializer.Deserialize<T>(memoryStream);
        }
    }

    /// Асинхронная сериализация объекта в файл
    public async Task SerializeAsync(T obj, string path, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path), "Путь к файлу не может быть null или пустым.");
        }

        byte[] data = await SerializeAsync(obj, token);
        await File.WriteAllBytesAsync(path, data, token);
    }

    /// Асинхронная десериализация файла в объект
    public async Task<T> DeserializeAsync(string path, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path), "Путь к файлу не может быть null или пустым.");
        }

        byte[] data = await File.ReadAllBytesAsync(path, token);
        return await DeserializeAsync(data, token);
    }
}