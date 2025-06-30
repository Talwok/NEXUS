namespace NEXUS.Fractal.Controls.Surface;

/// <summary>
/// Vertex data structure (position + color + normal)
/// </summary>
public struct OpenGlPoint(
    float x, float y, float z,
    float r, float g, float b,
    float nx, float ny, float nz,
    bool isBasement)
{
    public float X = x, Y = y, Z = z;
    public float R = r, G = g, B = b;
    public float Nx = nx, Ny = ny, Nz = nz;
    public float IsBasement = isBasement ? 1 : 0;
}