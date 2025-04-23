using Silk.NET.Maths;

public class CameraDescriptor
{
    public Vector3D<float> Position { get; private set; } = new Vector3D<float>(0, 0, 10);
    public Vector3D<float> ForwardVector { get; private set; } = new Vector3D<float>(0, 0, -1);
    public Vector3D<float> UpVector { get; private set; } = new Vector3D<float>(0, 1, 0);
    public Vector3D<float> RightVector => Vector3D.Normalize(Vector3D.Cross(ForwardVector, UpVector));

    public float Yaw { get; private set; } = -MathF.PI / 2;
    public float Pitch { get; private set; } = 0;

    private const float Sensitivity = 0.05f;
    private const float MoveSpeed = 0.5f;

    public void setPosition (int x, int y)
    {
        Position = new Vector3D<float>(x, 1f, y);
    }

    public void MoveForward()
    {
        Position += ForwardVector * MoveSpeed;
    }

    public void MoveBackward()
    {
        Position -= ForwardVector * MoveSpeed;
    }

    public void MoveRight()
    {
        Position += RightVector * MoveSpeed;
    }

    public void MoveLeft()
    {
        Position -= RightVector * MoveSpeed;
    }

    public void MoveUp()
    {
        Position += UpVector * MoveSpeed;
    }

    public void MoveDown()
    {
        Position -= UpVector * MoveSpeed;
    }

    public void Rotate(float deltaYaw, float deltaPitch)
    {
        Yaw += deltaYaw * Sensitivity;
        Pitch += deltaPitch * Sensitivity;

        // korlatozzuk, hogy ne forduljunk 9 foknal tobbet
        Pitch = Math.Clamp(Pitch, -MathF.PI / 2 + 0.1f, MathF.PI / 2 - 0.1f);

        UpdateCameraVectors();
    }

    private void UpdateCameraVectors()
    {
        ForwardVector = new Vector3D<float>(
            MathF.Cos(Yaw) * MathF.Cos(Pitch), // jobbra-balra
            MathF.Sin(Pitch), // fel-le
            MathF.Sin(Yaw) * MathF.Cos(Pitch) // elore, hatra
        );

        ForwardVector = Vector3D.Normalize(ForwardVector);
    }
}
