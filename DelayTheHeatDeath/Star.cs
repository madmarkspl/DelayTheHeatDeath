using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace DelayTheHeatDeath;

public class Star
{
    private readonly GL _gl;
    private readonly Random _rng;

    private bool _isAccelerating = false;
    private float _maxSpeed = 2;
    private Vector3D<float> _acceleration = Vector3D<float>.Zero;
    private Vector3D<float> _velocity = Vector3D<float>.Zero;
    private float _decelerationRate = -0.2f;
    private float _accelerationRate = 0.6f;

    public GravityField _antiGravity;

    public Transform Transform { get; private set; } = new Transform();

    public Star(GL gl, Random rng, float x, float y)
    {
        _gl = gl;
        _rng = rng;

        Transform.Position = new Vector3D<float>(x, y, 0);

        _antiGravity = new GravityField(_gl, 0, 0, 100, -0.001f);
    }

    internal void Interact(GravityField field)
    {
        var direction = field.Transform.Position - Transform.Position;
        var distance = direction.Length;

        var fraction = distance / field.Radius;

        //if (distance < field.Radius / 10)
        //{
        //    //_isAccelerating = true;
        //    _acceleration += (direction / distance) * field.Power * -1000;
        //    //Transform.Position += (direction / distance) * field.Power * -10;
        //}
        //else
        if (fraction < 1)
        {

            //_isAccelerating = true;
            _acceleration += (1 - fraction) * (direction / distance) * field.Power;
            //Transform.Position += (direction / distance) * field.Power;
        }
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
        _velocity += _acceleration * (float)delta;

        //var speed = _velocity.Length;

        //var velNorm = _velocity / speed;

        //if (speed > _maxSpeed)
        //{
        //    _velocity = velNorm * _maxSpeed;
        //}

        Transform.Position += _velocity;

        _antiGravity.Transform.Position = Transform.Position;

        //var displacementX = (_rng.NextSingle() - 0.5f);
        //var displacementY = (_rng.NextSingle() - 0.5f);

        //_transform.Position += new Vector3D<float>(displacementX, displacementY, 0);

        _acceleration = Vector3D<float>.Zero;
    }
}
