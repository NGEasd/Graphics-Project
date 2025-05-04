using Silk.NET.Maths;

public class CameraDescriptor
{
    public Vector3D<float> Position { get; private set; } = new Vector3D<float>(0, 0, 10);
    public Vector3D<float> ForwardVector { get; private set; } = new Vector3D<float>(0, 0, -1);
    public Vector3D<float> DirectionVector { get; private set; } = new Vector3D<float>(0, 0, -1);
    public Vector3D<float> UpVector { get; private set; } = new Vector3D<float>(0, 1, 0);
    public Vector3D<float> RightVector => Vector3D.Normalize(Vector3D.Cross(ForwardVector, UpVector));

    // storing variables
    public Vector3D<float> StoringDirVector { get; private set; } = new Vector3D<float>(0, 0, -1);
    public Vector3D<float> StoringPosition { get; private set; } = new Vector3D<float>(0, 0, 10);


    // camera types: player mode and ghost mode
    public bool defaultMode = true;
    public bool ghostMode = false;

    // pressed flags - moving
    public bool pressedForward = false;
    public bool pressedBackward = false;
    public bool pressedLeft = false;
    public bool pressedRight = false;

    // pressed flags - rotating
    public bool pressedRUp = false;
    public bool pressedRDown = false;
    public bool pressedRLeft = false;
    public bool pressedRRight = false;

    public float Yaw { get; private set; } = -MathF.PI / 2;
    public float Pitch { get; private set; } = 0;

    private const float Sensitivity = 0.05f;
    private const float MoveSpeed = 0.05f;

    public void setPosition (int x, int y)
    {
        Position = new Vector3D<float>(x, 1f, y);
    }

    public void MoveForward()
    {
        Position += DirectionVector * MoveSpeed;
    }

    public void MoveBackward()
    {
        Position -= DirectionVector * MoveSpeed;
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
        DirectionVector = ForwardVector;

        if (defaultMode)
        {
            DirectionVector = new Vector3D<float>(
                MathF.Cos(Yaw) * MathF.Cos(Pitch),
                0,
                MathF.Sin(Yaw) * MathF.Cos(Pitch) 
            );
        }

        DirectionVector = Vector3D.Normalize(DirectionVector);
    }

    public void changeCameraModeToGhost()
    {
        StoringDirVector = DirectionVector;
        StoringPosition = Position;
        defaultMode = false;
        ghostMode = true;
    }

    public void changeCameraModeToDefault()
    {
        DirectionVector = StoringDirVector;
        Position = StoringPosition;
        defaultMode = true;
        ghostMode = false;
    }
}
