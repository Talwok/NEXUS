using System;
using Silk.NET.OpenGL;

namespace NEXUS.Fractal.Controls.Surface;

public class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly BufferTargetARB _bufferType;

    public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
    {
        _gl = gl;
        _bufferType = bufferType;
        //Clear existing error code.
        GLEnum error;
        do error = _gl.GetError();
        while (error != GLEnum.NoError);
        _handle = _gl.GenBuffer();
        Bind();
        GlException.ThrowIfError(gl);
        fixed (void* d = data)
        {
            _gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
        GlException.ThrowIfError(gl);
    }

    public void Bind()
    {
        _gl.BindBuffer(_bufferType, _handle);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_handle);
    }
}