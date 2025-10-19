using System;
using Silk.NET.OpenGL;

namespace NEXUS.Fractal.Controls.Surface;

public class GlException(string message) : Exception(message)
{
    public static void ThrowIfError(GL gl)
    {
        if (gl.GetError() is var error and not GLEnum.NoError)
            throw new GlException(error.ToString());
    }
}