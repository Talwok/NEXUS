using System;
using Silk.NET.OpenGL;

namespace NEXUS.Fractal.Controls.Surface;

public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
    where TVertexType : unmanaged
    where TIndexType : unmanaged
{
    private readonly GL _gl;
    private readonly uint _handle;

    public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo)
    {
        _gl = gl;

        _handle = _gl.GenVertexArray();
        Bind();
        vbo.Bind();
        ebo.Bind();
    }

    public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, int offSet)
    {
        
        _gl.VertexAttribPointer(index, count, type, false, (uint) sizeof(TVertexType), (void*) (offSet * sizeof(float)));
        _gl.EnableVertexAttribArray(index);
    }

    public void Bind()
    {
        _gl.BindVertexArray(_handle);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_handle);
    }
}