using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace DelayTheHeatDeath;

public class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;

    public ShaderProgram(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;
        _handle = _gl.CreateProgram();

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

        var vpMatricesBlockIndex = _gl.GetUniformBlockIndex(this, "VPMatrices");
        // binds program uniform index to context binding point
        _gl.UniformBlockBinding(this, vpMatricesBlockIndex, VPMatricesUniformBindingIndex);

        _gl.DetachShader(_handle, vertex);
        _gl.DeleteShader(vertex);

        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(fragment);

        _gl.UseProgram(0);
    }

    public static implicit operator uint(ShaderProgram p) => p._handle;

    public static ShaderProgram Simple { get; set; }

    public static BufferObject<Matrix4X4<float>> VPMatricesUbo { get; private set; }

    public static uint VPMatricesUniformBindingIndex { get; private set; }

    public unsafe void Use()
    {
        _gl.UseProgram(_handle);
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

    public static void Setup(GL gl)
    {
        SetupVPMatricesUniformBuffer(gl);
        SetupSimpleShaderProgram(gl);
    }

    private static void SetupSimpleShaderProgram(GL gl)
    {
        var vertexShaderSource = File.ReadAllText("Shaders/simple.vert");
        var fragmentShaderSource = File.ReadAllText("Shaders/simple.frag");

        Simple = new ShaderProgram(gl, vertexShaderSource, fragmentShaderSource);
    }

    private static void SetupVPMatricesUniformBuffer(GL gl)
    {
        VPMatricesUniformBindingIndex = 0;

        var initializationArray = new Matrix4X4<float>[2];

        VPMatricesUbo = new BufferObject<Matrix4X4<float>>(gl, initializationArray, BufferTargetARB.UniformBuffer, BufferUsageARB.DynamicDraw);

        gl.BindBufferBase(GLEnum.UniformBuffer, VPMatricesUniformBindingIndex, VPMatricesUbo);
    }
}