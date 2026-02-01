using ProtoBuf;

namespace NEXUS.Fractal.Project.Data;

[ProtoContract]
public class ProjectHeightmapEntityData
{
    [ProtoMember(1)]
    public int Width { get; set; }
    [ProtoMember(2)]
    public int Height { get; set; }
    [ProtoMember(3)]
    public float[]? Data { get; set; }
    
    public static ProjectHeightmapEntityData FromHeightMap(float[,] heightmap) =>
        new ()
        {
            Height = heightmap.GetLength(0), 
            Width = heightmap.GetLength(1), 
            Data = Flatten(heightmap)
        };

    public float[,] GetHeightmap()
    {
        if (Data == null) return new float[0, 0];
        if (Data.Length != Width * Height)
            throw new ArgumentException("Array length does not match width * height");

        var heightmap = new float[Height, Width];
        for (var i = 0; i < Height; i++)
        {
            for (var j = 0; j < Width; j++)
            {
                heightmap[i, j] = Data[i * Width + j];
            }
        }
        return heightmap;
    }

    private static float[] Flatten(float[,] heightmap)
    {
        var height = heightmap.GetLength(0);
        var width = heightmap.GetLength(1);
        var data = new float[width * height];
        for (var i = 0; i < height; i++)
        for (var j = 0; j < width; j++)
            data[i * width + j] = heightmap[i, j];
    
        return data;
    }
}