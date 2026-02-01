using System.IO.Compression;
using System.Text;
using NEXUS.Fractal.Project.Entity;
using ProtoBuf;

namespace NEXUS.Fractal.Project;

public class ProjectInteractor : IDisposable, IAsyncDisposable
{
    private const string EntitiesFileName = "entities";
    private const string DataFolderName = "data";
    
    private readonly ZipArchive _projectArchive;
    
    private ProjectInteractor(ZipArchive projectArchive)
    {
        _projectArchive = projectArchive;
    }

    #region Entries
    
    public List<ProjectEntity> GetEntities() 
    {
        var entry = DeserializeEntry<List<ProjectEntity>>(EntitiesFileName);
        return entry ?? [];
    }
    
    public T? GetEntityData<T>(Guid id) => 
        DeserializeEntry<T>(Path.Combine(DataFolderName, id.ToString()));

    public void SetEntities(List<ProjectEntity> entities)
    {
        RemoveEntryData(EntitiesFileName);
        SerializeEntry(EntitiesFileName, entities);
    }
    
    public void SetEntityData<T>(Guid id, T? data)
    {
        var path = Path.Combine(DataFolderName, id.ToString());
        RemoveEntryData(path);
        SerializeEntry(path, data);
    }
    
    public void RemoveEntityData(Guid id)
    {
        var dataEntry = _projectArchive.GetEntry(Path.Combine(DataFolderName, id.ToString()));
        dataEntry?.Delete();
    }

    private void RemoveEntryData(string path)
    {
        var entry = _projectArchive.GetEntry(path);
        entry?.Delete();
    }
    
    #endregion

    #region Archive

    public static async Task<ProjectInteractor> OpenOrCreateAsync(string path)
    {
        var archieve = await ZipFile.OpenAsync(path, ZipArchiveMode.Update, Encoding.UTF8);
        return new ProjectInteractor(archieve);
    }

    public static ProjectInteractor OpenOrCreate(string path)
    {
        var archieve = ZipFile.Open(path, ZipArchiveMode.Update, Encoding.UTF8);
        return new ProjectInteractor(archieve);
    }
    
    #endregion
    
    #region Dispose

    public void Dispose()
    {
        _projectArchive.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _projectArchive.DisposeAsync();
    }

    #endregion

    #region Serialization

    private T? DeserializeEntry<T>(string entryName)
    {
        var entry = _projectArchive.GetEntry(entryName);
        
        if (entry is null) 
            return default;
        
        using var stream = entry.Open();
        
        return Serializer.Deserialize<T>(stream);
    }

    private void SerializeEntry<T>(string entryName, T entry)
    {
        var archiveEntry = _projectArchive.CreateEntry(entryName);
        
        using var entryStream = archiveEntry.Open();
        
        Serializer.Serialize(entryStream, entry);
    }

    #endregion


}