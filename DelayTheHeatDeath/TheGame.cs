using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace DelayTheHeatDeath;

internal class TheGame
{
    private int _width = 1280;
    private int _height = 720;

    private IWindow _window;

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
    }

    public void Run()
    {
        _window.Run();
    }

    private void OnLoad()
    {
    }

    private void OnUpdate(double obj)
    {
    }

    private void OnRender(double obj)
    {
    }
}
