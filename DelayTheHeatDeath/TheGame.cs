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

    private float[] _points;
    private ShaderProgram? _shaderProgram;
    private VertexArrayObject<float, uint> _vao;
    private BufferObject<float> _vbo;

    private Matrix4X4<float> _modelMatrix = Matrix4X4<float>.Identity;
    private Matrix4X4<float> _projectionMatrix = Matrix4X4<float>.Identity;

    // TODO: 
    private Transform _viewTransform = new Transform();
    private Vector3D<float> _direction = new Vector3D<float>(0, -1, 0);
    private float _rotation = 0;

    private Player _player;
    private List<Star> _stars = new List<Star>();

    private GravityField _blackHole;

    public TheGame()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(_width, _height);
        options.Title = "Delay The Heat Death - Ludum Dare 50 Compo entry";
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

    private void OnLoad()
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

        ShaderProgram.SetupSimpleShaderProgram(_gl, _projectionMatrix);

        _shaderProgram = ShaderProgram.Simple;

        //_points = GeneratePoints(2000, 10000);

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

        _shaderProgram.Use();

        _player = new Player(_gl);
        _blackHole = new GravityField(_gl, 0, 0, 100, 10);

        _stopwatch.Start();
    }

    private void OnUpdate(double delta)
    {
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

        _viewTransform.Position = -_player._transform.Position;

        //if (_stopwatch.Elapsed > TimeSpan.FromSeconds(1))
        //{
        //    Console.WriteLine($"{(int)(1 / delta)}");
        //    _stopwatch.Restart();
        //}

        _blackHole.Update(delta);
    }

    private unsafe void OnRender(double delta)
    {
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        
        _vao.Bind();

        var viewLocation = _gl.GetUniformLocation(_shaderProgram, "uView");
        var viewMatrix = _viewTransform.Matrix;
        _gl.UniformMatrix4(viewLocation, 1, false, (float*)&viewMatrix);

        foreach (var star in _stars)
        {
            star.Render(delta);
        }

        //var modelLocation = _gl.GetUniformLocation(_shaderProgram, "uModel");
        //fixed (Matrix4X4<float>* mat = &_modelMatrix)
        //{
        //    _gl.UniformMatrix4(modelLocation, 1, false, (float*)mat);
        //}

        //_gl.DrawArrays(PrimitiveType.Points, 0, 10000);

        _player.Render(delta);

        _blackHole.Render(delta);
    }

    private void KeyDown(IKeyboard arg1, Key key, int arg3)
    {
        if (key == Key.Escape)
        {
            _window.Close();
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
