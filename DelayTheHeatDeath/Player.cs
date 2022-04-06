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

    public Transform _transform = new Transform();

    private float _movementSpeed = 0;
    private float _maxSpeed = 500;
    private float _acceleration = 500;
    private float _deceleration = -200;

    private float _rotationSpeed = 250 * (float)(Math.PI / 180);
    private Vector3D<float> _direction = new Vector3D<float>(0, 1, 0);

    private bool _shouldMove = false;

    private bool _shouldRotate => _rotations.Count > 0;
    private List<Direction> _rotations = new List<Direction>();

    private GravityField _artificialGravity;

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

        _artificialGravity = new GravityField(_gl, 0, 0);
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
                _shouldMove = true;
            }
            else if (keyState == KeyState.Released)
            {
                _shouldMove = false;
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
        _gl.Uniform3(colorLocation, new Vector3D<float>(1.0f, 1.0f, 1.0f).ToSystem());

        var modelLocation = _gl.GetUniformLocation(ShaderProgram.Simple, "uModel");
        var modelMatrix = _transform.Matrix;
        _gl.UniformMatrix4(modelLocation, 1, false, (float*)&modelMatrix);

        _gl.DrawArrays(PrimitiveType.LineLoop, 0, 11);

        _artificialGravity.Render(delta);
    }

    public void Update(double delta)
    {
        if (_shouldMove)
        {
            if (_movementSpeed < _maxSpeed)
            {
                _movementSpeed += _acceleration * (float)delta;
            }

            _transform.Position += _movementSpeed * (float)delta * _direction;
        }
        else if (_movementSpeed > 0)
        {
            _movementSpeed = Math.Max(_movementSpeed + _deceleration * (float)delta, 0);

            _transform.Position += _movementSpeed * (float)delta * _direction;
        }

        if (_shouldRotate)
        {
            Turn(delta);
        }

        _artificialGravity.Update(delta);
        _artificialGravity._transform.Position = _transform.Position;
    }

    public void Turn(double delta)
    {
        _transform.Rotation += _rotationSpeed * (float)delta * (sbyte)_rotations.Last();

        _direction = Vector3D.Normalize(new Vector3D<float>((float)-Math.Sin(_transform.Rotation), (float)Math.Cos(_transform.Rotation), 0));
    }

    //public void Move(double delta)
    //{
    //    _transform.Position += _movementSpeed * (float)delta * _direction;
    //}

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