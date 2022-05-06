using Silk.NET.OpenGL;

namespace DelayTheHeatDeath;

public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
    where TVertexType : unmanaged
    where TIndexType : unmanaged
{
    private readonly GL _gl;
    private readonly uint _handle;

    private readonly BufferObject<TVertexType> _vbo;

    public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType>? ebo)
    {
        _gl = gl;
        _handle = _gl.GenVertexArray();
        _vbo = vbo;

        Bind();

        _vbo.Bind();
        ebo?.Bind();
    }

    public unsafe void VertexAttributePointerAbs(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
    {
        _gl.VertexAttribPointer(index, count, type, false, vertexSize, (void*)offSet);
        _gl.EnableVertexAttribArray(index);
    }

    public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
    {
        _gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(TVertexType), (void*)(offSet * sizeof(TVertexType)));
        _gl.EnableVertexAttribArray(index);
    }

    public void Bind()
    {
        _gl.BindVertexArray(_handle);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_handle);
    }
}
