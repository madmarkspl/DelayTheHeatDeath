using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace DelayTheHeatDeath;

public class Star
{
    private readonly GL _gl;
    private readonly Random _rng;

    public Transform Transform { get; private set; } = new Transform();

    public Star(GL gl, Random rng, float x, float y)
    {
        _gl = gl;
        _rng = rng;

        Transform.Position = new Vector3D<float>(x, y, 0);
    }

    internal void Interact(GravityField field)
    {

    }

    public unsafe void Render(double delta)
    {
        var shimmer = 1.0f - _rng.NextSingle() / 4;

        var colorLocation = _gl.GetUniformLocation(ShaderProgram.Simple, "uColor");
        _gl.Uniform3(colorLocation, new Vector3D<float>(shimmer).ToSystem());

        var modelLocation = _gl.GetUniformLocation(ShaderProgram.Simple, "uModel");
        var modelMatrix = Transform.Matrix;

        _gl.UniformMatrix4(modelLocation, 1, false, (float*)&modelMatrix);

        _gl.DrawArrays(PrimitiveType.Points, 0, 1);
    }

    public void Update(double delta)
    {
        //var displacementX = (_rng.NextSingle() - 0.5f);
        //var displacementY = (_rng.NextSingle() - 0.5f);

        //_transform.Position += new Vector3D<float>(displacementX, displacementY, 0);
    }
}
