using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace DelayTheHeatDeath;

public class Player
{
    private static float[] _vertexes;

    private readonly GL _gl;
    private readonly VertexArrayObject<float, uint> _vao;
    private readonly BufferObject<float> _vbo;

    private bool _isAccelerating = false;
    private float _maxSpeed = 10;
    private Vector3D<float> _velocity = Vector3D<float>.Zero;
    private float _decelerationRate = -0.02f;
    private float _accelerationRate = 0.06f;

    private float _rotationSpeed = 250 * (float)(Math.PI / 180);
    private Vector3D<float> _direction = new Vector3D<float>(0, 1, 0);

    private bool _shouldRotate => _rotations.Count > 0;
    private List<Direction> _rotations = new List<Direction>();

    public GravityField _artificialGravity;

    public Transform Transform { get; private set; } = new Transform();

    private ParticleEmitter _exhaustEmitter;

    static Player()
    {
        _vertexes = GenerateVertexes(20);
    }

    public Player(GL gl)
    {
        _gl = gl;

        _vbo = new BufferObject<float>(_gl, _vertexes, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, default);
        
        _vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 2, 0);

        _artificialGravity = new GravityField(_gl, 0, 0, 100, 10f);

        _exhaustEmitter = new ParticleEmitter(_gl, ShaderProgram.Simple.UProjectionMatrix, new Vector2D<float>(0, -15), 50000);
    }

    internal void Interact(Star star)
    {

    }

    public void InterpretInput(Key key, KeyState keyState)
    {
        if (key == Key.Up)
        {
            if (keyState == KeyState.Pressed)
            {
                _isAccelerating = true;
                _exhaustEmitter.Start();
            }
            else if (keyState == KeyState.Released)
            {
                _isAccelerating = false;
                _exhaustEmitter.Stop();
            }
        }
        else if (key == Key.Left)
        {
            if (keyState == KeyState.Pressed)
            {
                _rotations.Add(Direction.Left);
            }
            else if (keyState == KeyState.Released)
            {
                _rotations.Remove(Direction.Left);
            }
        }
        else if (key == Key.Right)
        {
            if (keyState == KeyState.Pressed)
            {
                _rotations.Add(Direction.Right);
            }
            else if (keyState == KeyState.Released)
            {
                _rotations.Remove(Direction.Right);
            }
        }
    }

    public unsafe void Render(double delta)
    {
        _vao.Bind();

        var colorLocation = _gl.GetUniformLocation(ShaderProgram.Simple, "uColor");

        // temporarily change color of ship; will be replaced by exhaust particles later
        if (_isAccelerating)
        {
            _gl.Uniform3(colorLocation, new Vector3D<float>(0.0f, 1.0f, 0.0f).ToSystem());
        }
        else if (_velocity.LengthSquared > 0)
        {
            _gl.Uniform3(colorLocation, new Vector3D<float>(1.0f, 0.0f, 0.0f).ToSystem());
        }
        else
        {
            _gl.Uniform3(colorLocation, new Vector3D<float>(1.0f, 1.0f, 1.0f).ToSystem());
        }

        var modelLocation = _gl.GetUniformLocation(ShaderProgram.Simple, "uModel");
        var modelMatrix = Transform.Matrix;
        _gl.UniformMatrix4(modelLocation, 1, false, (float*)&modelMatrix);

        _gl.DrawArrays(PrimitiveType.LineLoop, 0, 11);

        _artificialGravity.Render(delta);

        _exhaustEmitter.Render(delta);
    }

    public void Update(double delta)
    {
        Turn(delta);

        Move(delta);

        //_artificialGravity.Update(delta);
        _artificialGravity.Transform.Position = Transform.Position;
        _exhaustEmitter.Transform.Position = -Transform.Position;

        if (_isAccelerating)
        {
            _exhaustEmitter.Position = Transform.Position;
            _exhaustEmitter.Rotation = Transform.Rotation;
            _exhaustEmitter.Velocity = _velocity;
        }

        _exhaustEmitter.Update(delta);
    }

    private void Move(double delta)
    {
        var acceleration = 0f;
        var accelerationVector = Vector3D<float>.Zero;

        if (_isAccelerating)
        {
            acceleration = _accelerationRate * (float)delta;
            accelerationVector = _direction * _accelerationRate;
        }
        else if (_velocity.Length > 0)
        {
            acceleration = _decelerationRate * (float)delta;
            accelerationVector = (_velocity / _velocity.Length) * _decelerationRate;
        }

        _velocity += accelerationVector;

        var speed = _velocity.Length;

        var velNorm = _velocity / speed;
        var accNorm = accelerationVector / accelerationVector.Length;

        if (speed > _maxSpeed)
        {
            _velocity = velNorm * _maxSpeed;
        }

        var dot = Vector3D.Dot(velNorm, accNorm);

        if (acceleration < 0 && dot >= 0.999)
        {
            _velocity = Vector3D<float>.Zero;
        }

        Transform.Position += _velocity;
    }

    private void Turn(double delta)
    {
        if (_shouldRotate)
        {
            Transform.Rotation += _rotationSpeed * (float)delta * (sbyte)_rotations.Last();

            _direction = Vector3D.Normalize(new Vector3D<float>((float)-Math.Sin(Transform.Rotation), (float)Math.Cos(Transform.Rotation), 0));
        }
    }

    private static float[] GenerateVertexes(float s)
    {
        return new[]
        {
            // POINTY THING
            0.0f, s,

            // RIGHT HALF
            s * 0.25f, s * 0.5f,
            s * 0.25f, -s * 0.5f,
            s * 0.5f, -s * 0.75f,
            s * 0.5f, -s,
            s * 0.25f, -s * 0.75f,
            
            // LEFT HALF
            -0.25f * s, -0.75f * s,
            -0.5f * s, -s,
            -0.5f * s, -0.75f * s,
            -0.25f * s, -0.5f * s,
            -0.25f * s, 0.5f * s
        };
    }
}

public enum Direction : sbyte
{
    Right = -1,
    None = 0,
    Left = 1,
}