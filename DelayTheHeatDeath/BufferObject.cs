using Silk.NET.OpenGL;

namespace DelayTheHeatDeath;

public class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    private readonly BufferTargetARB _bufferType;
    private readonly GL _gl;
    private readonly uint _handle;

    public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
    {
        _bufferType = bufferType;
        _gl = gl;
        _handle = _gl.GenBuffer();

        Bind();

        fixed (void* d = data)
        {
            _gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
    }

    public void Bind()
    {
        _gl.BindBuffer(_bufferType, _handle);
    }

    public unsafe void UpdateData(Span<TDataType> data, int offset = 0)
    {
        fixed (void* d = data)
        {
            // todo: check for buffer size
            _gl.BufferSubData(_bufferType, offset, (nuint)(data.Length * sizeof(TDataType)), d);
        }
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_handle);
    }
}