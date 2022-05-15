using Silk.NET.Maths;

namespace DelayTheHeatDeath;

public class Transform
{
    private static Vector3D<float> _up = new Vector3D<float>(0, 0, 1);

    public Vector3D<float> Position { get; set; } = new Vector3D<float>(0, 0, 0);

    public float Rotation { get; set; } = 0;

    public Vector3D<float> Scale { get; set; } = new Vector3D<float>(1);

    public Matrix4X4<float> Matrix => Matrix4X4<float>.Identity * Matrix4X4.CreateFromAxisAngle(_up, Rotation) * Matrix4X4.CreateScale(Scale) * Matrix4X4.CreateTranslation(Position);
}
