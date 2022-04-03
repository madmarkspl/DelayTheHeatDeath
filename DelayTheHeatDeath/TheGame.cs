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
    private GL _gl;

    private float[] _points;
    private ShaderProgram? _shaderProgram;
    private VertexArrayObject<float, uint> _vao;
    private BufferObject<float> _vbo;

    private Matrix4X4<float> _modelMatrix = Matrix4X4<float>.Identity;
    private Matrix4X4<float> _projectionMatrix = Matrix4X4<float>.Identity;

    public TheGame()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(_width, _height);
        options.Title = "Delay The Heat Death - Ludum Dare 50 Compo entry";
        options.VSync = false;
        options.UpdatesPerSecond = 60;
        options.FramesPerSecond = 60;

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
        _gl = GL.GetApi(_window);

        _projectionMatrix = Matrix4X4.CreateOrthographic(_width, _height, 0.01f, 100.0f);

        ShaderProgram.SetupSimpleShaderProgram(_gl, _projectionMatrix);

        _shaderProgram = ShaderProgram.Simple;

        _points = GeneratePoints(500, 10000).ToArray();

        _vbo = new BufferObject<float>(_gl, _points, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, default);

        _vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 2, 0);

        _gl.Enable(EnableCap.DepthTest);
        //_gl.Enable(EnableCap.CullFace);
        //_gl.CullFace(CullFaceMode.Back);

        _shaderProgram.Use();
    }

    private void OnUpdate(double obj)
    {
    }

    private unsafe void OnRender(double obj)
    {
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        
        _vao.Bind();

        var colorLocation = _gl.GetUniformLocation(_shaderProgram, "uColor");

        _gl.Uniform3(colorLocation, new Vector3D<float>(1.0f, 1.0f, 1.0f).ToSystem());

        var modelLocation = _gl.GetUniformLocation(_shaderProgram, "uModel");

        fixed (Matrix4X4<float>* mat = &_modelMatrix)
        {
            _gl.UniformMatrix4(modelLocation, 1, false, (float*)mat);
        }

        _gl.DrawArrays(PrimitiveType.Points, 0, 10000);
    }

    private IEnumerable<float> GeneratePoints(float radius, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var r = radius * Math.Sqrt(_rng.NextDouble());
            var theta = 2 * Math.PI * _rng.NextDouble();
            var x = (float)(Math.Cos(theta) * r);
            var y = (float)(Math.Sin(theta) * r);

            yield return x;
            yield return y;
        }
    }
}
