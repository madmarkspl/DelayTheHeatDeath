using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace DelayTheHeatDeath;

public class ParticleEmitter
{
    private GL _gl;
    private ShaderProgram _program;
    private readonly VertexArrayObject<Particle, uint> _vao;
    private readonly BufferObject<Particle> _vbo;

    private Particle[] _particles;
    private uint _activeParticlesCount = 0;
    private uint _newParticlesCount = 0;

    private Vector2D<float> _offset;

    private uint _size;

    private readonly Random _rng = new Random();

    private VectorVisualizer _vecVis;

    public unsafe ParticleEmitter(GL gl, Vector2D<float> offset, uint size)
    {
        _gl = gl;
        _offset = offset;
        _size = size;
        _newParticlesCount = _size / 40;

        var vertexSource = File.ReadAllText("Shaders/particles.vert");
        var fragmentSource = File.ReadAllText("Shaders/particles.frag");

        _program = new ShaderProgram(_gl, vertexSource, fragmentSource);

        _particles = new Particle[_size];

        _vbo = new BufferObject<Particle>(_gl, _particles, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<Particle, uint>(_gl, _vbo, default);

        //var pos = _gl.GetAttribLocation(_program, "position");
        //var col = _gl.GetAttribLocation(_program, "color");
        //var life = _gl.GetAttribLocation(_program, "life");

        _vao.VertexAttributePointerAbs(0, 2, VertexAttribPointerType.Float, (uint)sizeof(Particle), 0);
        _vao.VertexAttributePointerAbs(1, 4, VertexAttribPointerType.Float, (uint)sizeof(Particle), 2 * sizeof(Vector2D<float>));
        _vao.VertexAttributePointerAbs(2, 1, VertexAttribPointerType.Float, (uint)sizeof(Particle), 2 * sizeof(Vector2D<float>) + sizeof(Vector4D<float>));

        _vecVis = new VectorVisualizer(_gl);
    }

    public bool IsEmitting { get; private set; }

    public Transform Transform { get; private set; } = new Transform();

    public Vector3D<float> Velocity { get; set; }

    public unsafe void Render(double delta)
    {
        _program.Use();

        _vao.Bind();

        _gl.DrawArrays(PrimitiveType.Points, 0, _activeParticlesCount);

        for (int i = 0; i < _activeParticlesCount; i++)
        {
            if (_particles[i].Life > 0.0f)
            {
                _vecVis.Transform.Position = new Vector3D<float>(_particles[i].Position.X, _particles[i].Position.Y, 0.0f);
                _vecVis.Render(new Vector3D<float>(_particles[i].Velocity.X, _particles[i].Velocity.Y, 0.0f) , Velocity.Length + _particles[i].Velocity.Length, new Vector3D<float>(1f, 1f, 0f));
            }
        }
    }

    public void Update(double delta)
    {
        int lastParticleIndex = (int)_activeParticlesCount - 1;
        for (int i = lastParticleIndex; i >= 0; i--)
        {
            if (_particles[i].Life <= 0.0f)
            {
                if (i != _activeParticlesCount - 1)
                {
                    _particles[i] = _particles[_activeParticlesCount - 1];
                }

                _activeParticlesCount--;
            }
        }

        for (int i = 0; i < _activeParticlesCount; i++)
        {
            if (_particles[i].Life > 0.0f)
            {
                _particles[i].Position += _particles[i].Velocity * (float)delta;

                _particles[i].Life -= 0.5f * (float)delta;
            }
        }

        if (IsEmitting)
        {
            var rotSin = MathF.Sin(Transform.Rotation);
            var rotCos = MathF.Cos(Transform.Rotation);

            var positionOffset = new Vector2D<float>(rotCos * _offset.X - rotSin * _offset.Y, rotSin * _offset.X + rotCos * _offset.Y);
            var velocity = new Vector2D<float>(Velocity.X, Velocity.Y);

            for (int i = 0; i < _newParticlesCount && _activeParticlesCount < _size; i++)
            {
                var angleVariation = _rng.NextSingle() - 0.5f;
                var direction = -Vector2D.Normalize(new Vector2D<float>(-MathF.Sin(Transform.Rotation + angleVariation), MathF.Cos(Transform.Rotation + angleVariation)));
                var life = _rng.NextSingle() / 2.0f + 0.5f;

                _particles[_activeParticlesCount].Position = new Vector2D<float>(Transform.Position.X, Transform.Position.Y) + positionOffset;

                // https://www.desmos.com/calculator/yb4n5yxpow
                // https://www.desmos.com/calculator/vkxgeqyykn
                // https://www.desmos.com/calculator/l1b7hcvhvg

                var colorEquation = MathF.Sqrt(-MathF.Abs(2f * angleVariation) + 1f);
                var alphaEquation = MathF.Pow(MathF.Cos(MathF.PI * angleVariation), 0.5f);
                _particles[_activeParticlesCount].Color = new Vector4D<float>(colorEquation, 1f - colorEquation, 0.0f, alphaEquation);

                _particles[_activeParticlesCount].Velocity =
                    velocity
                    + direction * Math.Clamp(_rng.NextSingle(), 0.01f, 1f) * 500;

                _particles[_activeParticlesCount].Life = life;

                _activeParticlesCount++;
            }
        }

        _vbo.Bind();
        _vbo.UpdateData(_particles);

        //Console.WriteLine($"Part: {_particles[0].Velocity}");
    }

    public void Start()
    {
        IsEmitting = true;
    }

    public void Stop()
    {
        IsEmitting = false;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct Particle
    {
        [FieldOffset(0)]
        public Vector2D<float> Position;

        [FieldOffset(8)]
        public Vector2D<float> Velocity;

        [FieldOffset(16)]
        public Vector4D<float> Color;

        [FieldOffset(32)]
        public float Life;
    }
}
