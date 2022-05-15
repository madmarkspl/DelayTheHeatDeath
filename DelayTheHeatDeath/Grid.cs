using Silk.NET.OpenGL;

namespace DelayTheHeatDeath;

public class Grid
{
    private static float[] _vertexes;
    
    private readonly GL _gl;
    private ShaderProgram _program;
    private readonly VertexArrayObject<float, uint> _vao;
    private readonly BufferObject<float> _vbo;

    static Grid()
    {
        _vertexes = GenerateVertexes(1.0f);
    }

    public Grid(GL gl)
    {
        _gl = gl;

        var vertexSource = File.ReadAllText("Shaders/grid.vert");
        var fragmentSource = File.ReadAllText("Shaders/grid.frag");

        _program = new ShaderProgram(_gl, vertexSource, fragmentSource);

        _vbo = new BufferObject<float>(_gl, _vertexes, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, default);

        _vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 2, 0);
    }

    public Transform Transform { get; private set; } = new Transform();

    public void Update(double delta)
    {
    }

    public unsafe void Render(double delta)
    {
        _program.Use();

        var modelLocation = _gl.GetUniformLocation(_program, "uModel");
        var modelMatrix = Transform.Matrix;
        _gl.UniformMatrix4(modelLocation, 1, false, (float*)&modelMatrix);

        _vao.Bind();

        _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)_vertexes.Length / 2);
    }

    private static float[] GenerateVertexes(float s)
    {
        return new[]
        {
            // TOP-LEFT
            s, s,
            -s, s,
            -s, -s,

            // BOTTOM-RIGHT
            s, s,
            -s, -s,
            s, -s
        };
    }
}
