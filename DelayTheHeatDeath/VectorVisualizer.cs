using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace DelayTheHeatDeath;
internal class VectorVisualizer
{
    private static float[] _vertexes;

    private readonly GL _gl;
    private ShaderProgram _program;
    private readonly VertexArrayObject<float, uint> _vao;
    private readonly BufferObject<float> _vbo;

    static VectorVisualizer()
    {
        _vertexes = GenerateVertexes(0);
    }

    public VectorVisualizer(GL gl)
    {
        _gl = gl;

        var vertexSource = File.ReadAllText("Shaders/vectorVisualizer.vert");
        var fragmentSource = File.ReadAllText("Shaders/vectorVisualizer.frag");

        _program = new ShaderProgram(_gl, vertexSource, fragmentSource);

        _vbo = new BufferObject<float>(_gl, _vertexes, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, default);

        _vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 2, 0);
    }

    public Transform Transform { get; private set; } = new Transform();

    public static bool IsActive { get; set; } = true;

    public void Update(double delta)
    {
    }

    public unsafe void Render(Vector3D<float> input, float maxLength, Vector3D<float> color)
    {
        if (!IsActive)
        {
            return;
        }

        _program.Use();

        var inputVectorLocation = _gl.GetUniformLocation(_program, "uColor");
        _gl.Uniform3(inputVectorLocation, color.ToSystem());

        _vao.Bind();

        var up = new Vector3D<float>(0, 1, 0);

        var angle = MathF.Acos(Vector3D.Dot(up, input) / (up.Length * input.Length));
        //var scale = Matrix4X4.CreateScale(0, 50, 0);

        var cross = Vector3D.Cross(up, input);

        Transform.Rotation = cross.Z < 0 ? -angle : 2 * MathF.PI + angle;
        Transform.Scale = new Vector3D<float>((input.Length / maxLength) * 20);

        var modelLocation = _gl.GetUniformLocation(_program, "uModel");
        var modelMatrix = Transform.Matrix;
        _gl.UniformMatrix4(modelLocation, 1, false, (float*)&modelMatrix);

        _gl.DrawArrays(PrimitiveType.Lines, 0, (uint)_vertexes.Length / 2);;
    }

    private static float[] GenerateVertexes(float s)
    {
        return new[]
        {
            0.0f, 0.0f,
            0.0f, 1.0f,
            
            0.0f, 1.0f,
            -0.2f, 0.8f,

            0.0f, 1.0f,
            0.2f, 0.8f
        };
    }
}
