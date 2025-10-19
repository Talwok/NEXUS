using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace NEXUS.Fractal.Controls.Surface;

public class Shader : IDisposable
{
    private const string GlShaderVersion = "#version 300 es";

    private readonly GL _gl;
    private readonly uint _handle;

    public Shader(GL gl)
    {
        _gl = gl;

        var vertex = LoadShader(ShaderType.VertexShader, VertexShaderSource);
        var fragment = LoadShader(ShaderType.FragmentShader, FragmentShaderSource);
        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);
        _gl.LinkProgram(_handle);
        _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_handle)}");
        }

        _gl.DetachShader(_handle, vertex);
        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    public void Use()
    {
        _gl.UseProgram(_handle);
    }

    public void SetUniform(string name, int value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }

        _gl.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }

        _gl.Uniform1(location, value);
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }

    private uint LoadShader(ShaderType type, string shaderCode)
    {
        uint handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, shaderCode);
        _gl.CompileShader(handle);
        string infoLog = _gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }

    public void UniformMatrix4(string name, Matrix4x4 value)
    {
        var location = GetLocation(name);
        _gl.UniformMatrix4(location, 1, false, MemoryMarshal.CreateReadOnlySpan(ref value.M11, 16));
    }

    public void Uniform1(string name, float value)
    {
        var location = GetLocation(name);
        _gl.Uniform1(location, value);
    }

    public void Uniform1(string name, uint value)
    {
        var location = GetLocation(name);
        _gl.Uniform1(location, value);
    }

    public void Uniform3(string name, Vector3 value)
    {
        var location = GetLocation(name);
        _gl.Uniform3(location, value);
    }

    public int GetLocation(string name) =>
        _gl.GetUniformLocation(_handle, name);

    // Shader sources remain the same as in original
    private string VertexShaderSource => GlShaderVersion + @"
    precision mediump float;
    layout(location = 0) in vec3 aPos;
    layout(location = 1) in vec3 aColor;
    layout(location = 2) in vec3 aNormal;
    layout(location = 3) in float aIsBasement;
    uniform mat4 model;
    uniform mat4 view;
    uniform mat4 projection;
    uniform float heightMultiplier;
    out vec3 FragPos;
    out vec3 Normal;
    out vec3 VertexColor;
    out float IsBasement;
    void main()
    {
        FragPos = vec3(model * vec4(aPos.x, aPos.y * heightMultiplier, aPos.z, 1.0));
        Normal = mat3(transpose(inverse(model))) * aNormal;
        VertexColor = aColor;
        gl_Position = projection * view * vec4(FragPos, 1.0);
        IsBasement = aIsBasement;
    }";

    private string FragmentShaderSource => GlShaderVersion + @"
    precision mediump float; 
    in vec3 FragPos;
    in vec3 Normal;
    in vec3 VertexColor;
    in float IsBasement;
    uniform vec3 lightPosition;
    uniform vec3 cameraPosition;    
    uniform float showFoundation;
    uniform float ambientStrength;
    uniform float specularStrength;
    out vec4 FragColor;
    void main()
    {
        if (showFoundation < 0.5 && IsBasement > 0.5) 
        {
            discard;
        }
        // Ambient
        vec3 ambient = ambientStrength * VertexColor;
        // Diffuse
        vec3 norm = normalize(Normal);
        vec3 lightDir = normalize(lightPosition - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = diff * VertexColor;
        // Specular
        vec3 viewDir = normalize(cameraPosition - FragPos);
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
        vec3 specular = specularStrength * spec * vec3(1.0);
        vec3 result = IsBasement > 0.5 ? VertexColor : ambient + diffuse + specular;
        FragColor = vec4(result, 1.0);  
    }";
}