using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace DelayTheHeatDeath;

public class GravityField
{
    private const int LineSegmentCount = 36;
    private static float[] _vertexes;

    public Transform _transform = new Transform();

    private readonly GL _gl;
    private readonly VertexArrayObject<float, uint> _vao;
    private readonly BufferObject<float> _vbo;

    static GravityField()
    {
        _vertexes = GenerateVertexes(0, 2 * MathF.PI, LineSegmentCount).ToArray();
    }

    public GravityField(GL gl, float x, float y)
    {
        _gl = gl;

        _transform.Position = new Vector3D<float>(x, y, 0);

        _vbo = new BufferObject<float>(_gl, _vertexes, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, default);

        _vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 2, 0);
    }

    public void Update(double delta)
    {
        //var displacementX = (_rng.NextSingle() - 0.5f);
        //var displacementY = (_rng.NextSingle() - 0.5f);

        //_transform.Position += new Vector3D<float>(displacementX, displacementY, 0);
    }

    public unsafe void Render(double delta)
    {
        _vao.Bind();

        var colorLocation = _gl.GetUniformLocation(ShaderProgram.Simple, "uColor");
        _gl.Uniform3(colorLocation, new Vector3D<float>(0.2f, 0.8f, 0.7f).ToSystem());

        var modelLocation = _gl.GetUniformLocation(ShaderProgram.Simple, "uModel");
        var modelMatrix = _transform.Matrix;

        _gl.UniformMatrix4(modelLocation, 1, false, (float*)&modelMatrix);

        _gl.DrawArrays(PrimitiveType.LineLoop, 0, LineSegmentCount);
    }

    private static IEnumerable<float> GenerateVertexes(float startAngle, float endAngle, int numberOfSegments)
    {
        var angleIncrement = (endAngle - startAngle) / numberOfSegments;

        var xRadius = 100f;

        var yRadius = 100f;

        for (var i = 0; i < numberOfSegments; i++)
        {
            var angle = startAngle + angleIncrement * i;

            var x = MathF.Cos(angle) * xRadius;
            var y = MathF.Sin(angle) * yRadius;

            Console.WriteLine($"({x}, {y})");

            yield return x;
            yield return y;

            //if (angle >= endAngle)
            //{
            //    yield break;
            //}
        }

        //for (float angle = 0.0f; angle <= MathF.PI; angle += angleIncrement)
        //{
        //    var x = MathF.Cos(angle) * xRadius;
        //    var y = MathF.Sin(angle) * yRadius;

        //    yield return x;
        //    yield return y;
        //    yield return 0.0f;
        //}
    }
}
