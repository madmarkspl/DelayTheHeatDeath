using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace DelayTheHeatDeath;

public class ShaderProgram : IDisposable
{
    public static ShaderProgram Simple { get; set; }

    private readonly GL _gl;
    private readonly uint _handle;

    public Matrix4X4<float> UProjectionMatrix;
    private readonly int _uProjectionMatrixLocation;

    public ShaderProgram(GL gl, string vertexSource, string fragmentSource, Matrix4X4<float> projectionMatrix)
    {
        _gl = gl;
        _handle = _gl.CreateProgram();

        UProjectionMatrix = projectionMatrix;

        var vertex = AddShader(ShaderType.VertexShader, vertexSource);
        var fragment = AddShader(ShaderType.FragmentShader, fragmentSource);

        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);

        _gl.LinkProgram(_handle);

        _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            Console.WriteLine($"Error linking shader program '{_gl.GetProgramInfoLog(_handle)}'.");
        }

        _gl.DetachShader(_handle, vertex);
        _gl.DeleteShader(vertex);

        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(fragment);

        _gl.UseProgram(_handle);

        _uProjectionMatrixLocation = _gl.GetUniformLocation(_handle, "uProjection");

        _gl.UseProgram(0);
    }

    public static implicit operator uint(ShaderProgram p) => p._handle;

    public unsafe void Use()
    {
        _gl.UseProgram(_handle);

        fixed (Matrix4X4<float>* ptr = &UProjectionMatrix)
        {
            _gl.UniformMatrix4(_uProjectionMatrixLocation, 1, false, (float*)ptr);
        }
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }

    private uint AddShader(ShaderType type, string source)
    {
        var shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        var infoLog = _gl.GetShaderInfoLog(shader);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            Console.WriteLine($"Error compiling shader '{infoLog}'.");
        }

        return shader;
    }

    public static void SetupSimpleShaderProgram(GL gl, Matrix4X4<float> projectionMatrix)
    {
        const string VertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec4 vPos;
        
            uniform mat4 uModel;
            uniform mat4 uProjection;

            void main()
            {
                gl_Position = uProjection * uModel * vPos;
            }
            ";

        const string FragmentShaderSource = @"
            #version 330 core

            uniform vec3 uColor;

            out vec4 FragColor;
            void main()
            {
                FragColor = vec4(uColor, 1.0f);
            }
            ";

        Simple = new ShaderProgram(gl, VertexShaderSource, FragmentShaderSource, projectionMatrix);
    }
}