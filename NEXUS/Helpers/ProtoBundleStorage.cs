using System.Security.Cryptography;
using ProtoBuf;

namespace NEXUS.Helpers;


public class ProtoBundleStorage : IDisposable
{
    /// <summary>
    /// Словарь бандла ключ-данные объекта
    /// </summary>
    private Dictionary<string, byte[]> _dataDictionary;
    
    /// <summary>
    /// Хранилище бандла
    /// </summary>
    /// <param name="dataDictionary">Словарь бандла ключ-данные объекта</param>
    private ProtoBundleStorage(Dictionary<string, byte[]> dataDictionary) 
        => _dataDictionary = dataDictionary;


    /// <summary>
    /// Добавление или обновление объекта по ключу
    /// </summary>
    /// <param name="key">Ключ объекта в бандле</param>
    /// <param name="obj">Объект бандла</param>
    /// <typeparam name="T">Тип объекта</typeparam>
    public void Add<T>(string key, T obj)
    {
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, obj);
        _dataDictionary[key] = ms.ToArray();
        ms.Close();
    }
    
    /// <summary>
    /// Получение объекта по ключу
    /// </summary>
    /// <param name="key">Ключ объекта в бандле</param>
    /// <typeparam name="T">Тип объекта</typeparam>
    /// <returns>Искомый объект</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public T Load<T>(string key)
    {
        if (!_dataDictionary.TryGetValue(key, out var raw))
            throw new KeyNotFoundException($"Key '{key}' not found in bundle.");

        using var ms = new MemoryStream(raw);
        var result = Serializer.Deserialize<T>(ms);
        ms.Close();
        return result;
    }

    /// <summary>
    /// Сохранение всего словаря в файл
    /// </summary>
    /// <param name="path">Путь к бандлу</param>
    public void SaveToFile(string path)
    {
        using var file = File.Create(path);
        Serializer.Serialize(file, _dataDictionary);
        file.Close();
    }

    /// <summary>
    /// Загрузка всего словаря из файла
    /// </summary>
    /// <param name="path">Путь к бандлу</param>
    /// <returns>Объект хранилища</returns>
    public static ProtoBundleStorage LoadFromFile(string path)
    {
        using var file = !File.Exists(path) ? File.Create(path) : File.OpenRead(path);
        var loaded = Serializer.Deserialize<Dictionary<string, byte[]>>(file);
        file.Close();
        return new ProtoBundleStorage(loaded);
    }

    /// <summary>
    /// Проверка существования ключа
    /// </summary>
    /// <param name="key">Ключ объекта в бандле</param>
    /// <returns>Предикат нахождения ключа</returns>
    public bool ContainsKey(string key) => _dataDictionary.TryGetValue(key, out _);

    /// <summary>
    /// Вычислить MD5 хеш для объекта
    /// </summary>
    /// <param name="obj">Объект для вычисления хеша</param>
    /// <typeparam name="T">Тип объекта</typeparam>
    /// <returns>Байты хеша</returns>
    public byte[] Md5Hash<T>(T obj)
    {
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, obj);
        return MD5.HashData(ms);
    }

    public void Dispose()
    {
        _dataDictionary.Clear();
        _dataDictionary = null;
    }
}
