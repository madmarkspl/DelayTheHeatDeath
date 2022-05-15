using System.Diagnostics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace DelayTheHeatDeath;

internal class TheGame
{
    private int _width = 1280;
    private int _height = 720;

    private readonly Random _rng;
    private IWindow _window;
    private IKeyboard _primaryKeyboard;
    private GL _gl;
    private Stopwatch _stopwatch = new Stopwatch();

    private ShaderProgram? _shaderProgram;
    private VertexArrayObject<float, uint> _vao;
    private BufferObject<float> _vbo;

    private Matrix4X4<float> _projectionMatrix = Matrix4X4<float>.Identity;

    // TODO: 
    private Transform _viewTransform = new Transform();

    private Player _player;
    private List<Star> _stars = new List<Star>();

    private GravityField _blackHole;

    private ParticleEmitter _particleEmitter;

    private Grid _grid;

    public TheGame()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(_width, _height);
        options.Title = "Delay The Heat Death";
        options.VSync = false;
        options.UpdatesPerSecond = 60;
        //options.FramesPerSecond = 60;

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Update += OnUpdate;

        _rng = new Random();
    }

    public void Run()
    {
        _window.Run();
    }

    private unsafe void OnLoad()
    {
        var input = _window.CreateInput();

        _primaryKeyboard = input.Keyboards.FirstOrDefault();

        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
            input.Keyboards[i].KeyUp += KeyUp;
        }

        _gl = GL.GetApi(_window);

        _projectionMatrix = Matrix4X4.CreateOrthographic(_width, _height, 0.01f, 100.0f);

        ShaderProgram.Setup(_gl);

        ShaderProgram.VPMatricesUbo.UpdateData(new[] { _projectionMatrix }, sizeof(Matrix4X4<float>));

        _shaderProgram = ShaderProgram.Simple;

        foreach (var point in GeneratePoints(200, 200))
        {
            _stars.Add(new(_gl, _rng, point.X, point.Y));
        }

        // TODO: drawing 10000 points from one array once or drawing 10000 times each point alone
        // this needs wrapping in some Universe class; then maybe single buffer could be cleanly used for positions
        var zeroZero = new[] { 0.0f, 0.0f };
        _vbo = new BufferObject<float>(_gl, zeroZero, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, default);

        _vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 2, 0);

        _gl.Enable(EnableCap.DepthTest);
        //_gl.Enable(EnableCap.CullFace);
        //_gl.CullFace(CullFaceMode.Back);
        _gl.Enable(GLEnum.Blend);
        _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        _shaderProgram.Use();

        _player = new Player(_gl);
        _blackHole = new GravityField(_gl, 0, 0, 100, 10);

        _particleEmitter = new ParticleEmitter(_gl, new Vector2D<float>(0, 0), 5000);

        _grid = new Grid(_gl);
        _grid.Transform.Scale = new Vector3D<float>(_width / 2.0f, _height / 2.0f, 0.0f);

        _stopwatch.Start();
    }

    private void OnUpdate(double delta)
    {
        //if (_stopwatch.Elapsed > TimeSpan.FromSeconds(1))
        //{
        //    //Console.WriteLine($"{(int)(1 / delta)}");
        //    Console.Clear();
        //    _stopwatch.Restart();
        //}

        foreach (var star in _stars)
        {
            foreach (var otherStar in _stars.Where(s => s != star))
            {
                star.Interact(otherStar._antiGravity);
            }

            star.Interact(_player._artificialGravity);
            star.Interact(_blackHole);

            star.Update(delta);
        }

        _player.Update(delta);

        _viewTransform.Position = -_player.Transform.Position;
        
        _blackHole.Update(delta);
        //_particleEmitter.Update(delta);

        _grid.Transform.Position = _player.Transform.Position;

        _grid.Update(delta);

        ShaderProgram.VPMatricesUbo.UpdateData(new[] { _viewTransform.Matrix });
    }

    private unsafe void OnRender(double delta)
    {
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        _shaderProgram.Use();

        _vao.Bind();

        foreach (var star in _stars)
        {
            star.Render(delta);
        }

        _blackHole.Render(delta);

        _player.Render(delta);

        //_particleEmitter.Render(delta);

        _grid.Render(delta);
    }

    private void KeyDown(IKeyboard arg1, Key key, int arg3)
    {
        if (key == Key.Escape)
        {
            _window.Close();
            return;
        }

        if (key == Key.Q)
        {
            _particleEmitter.Start();
            return;
        }

        if (key == Key.E)
        {
            _particleEmitter.Stop();
            return;
        }

        if (key == Key.V)
        {
            VectorVisualizer.IsActive = !VectorVisualizer.IsActive;
            return;
        }

        if (key == Key.W)
        {
            _particleEmitter.Transform.Position += new Vector3D<float>(0, 1, 0);
            return;
        }

        if (key == Key.S)
        {
            _particleEmitter.Transform.Position += new Vector3D<float>(0, -1, 0);
            return;
        }

        if (key == Key.A)
        {
            _particleEmitter.Transform.Position += new Vector3D<float>(-1, 0, 0);
            return;
        }

        if (key == Key.D)
        {
            _particleEmitter.Transform.Position += new Vector3D<float>(1, 0, 0);
            return;
        }

        _player.InterpretInput(key, KeyState.Pressed);
    }

    private void KeyUp(IKeyboard arg1, Key key, int arg3)
    {
        _player.InterpretInput(key, KeyState.Released);
    }

    private IEnumerable<Vector2D<float>> GeneratePoints(float radius, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var r = radius * Math.Sqrt(_rng.NextDouble());
            var theta = 2 * Math.PI * _rng.NextDouble();
            var x = (float)(Math.Cos(theta) * r);
            var y = (float)(Math.Sin(theta) * r);

            yield return new(x, y);
        }
    }
}
