using Labirintus_projekt;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace LabirintusProjekt
{
    class PacmanCharacter
    {
        public GL gl;
        public GlObject head;
        public GlObject legs;
        
        // position and transformation descriptor
        public Vector3D<float> Position { get; set; } = new Vector3D<float>(0, 0, 0);
        public Matrix4X4<float> transfHead;
        public Matrix4X4<float> transfLegs;

        // rotation
        public float yaw = 0;

        // default transformations to resetting
        public Matrix4X4<float> deftransfHead;
        public Matrix4X4<float> deftransfLegs;

        // body transformation to idle
        public Matrix4X4<float> prevtransfHead;

        // position
        private Matrix4X4<float> transfPos;

        // size
        private Matrix4X4<float> scale;

        // rotation
        private Matrix4X4<float> rotation ;


        public PacmanCharacter(GL gl)
        {
            this.gl = gl;
            transfPos= Matrix4X4.CreateTranslation(Position);
            head = ObjResourceReader.CreateObject(gl, [1.0f, 1.0f, 0.0f, 1.0f], "LabirintusProjekt.Resources.head.obj");
            legs = ObjResourceReader.CreateObject(gl, [1.0f, 1.0f, 0.0f, 1.0f], "LabirintusProjekt.Resources.legs.obj");
            rotation = Matrix4X4<float>.Identity;

            scale = Matrix4X4.CreateScale(new Vector3D<float>(0.035f, 0.035f, 0.035f));
            transfLegs = scale;
            transfHead = scale;

            deftransfHead = transfHead;
            prevtransfHead = transfHead;
            deftransfLegs = transfLegs;
        }

        public void updatePosition()
        {
            transfPos = Matrix4X4.CreateTranslation(Position);
            transfHead = scale * rotation *  transfPos;
            transfLegs = scale * rotation * transfPos;
        }

        public void Rotate(float yawDelta)
        {
            yaw -= yawDelta * 0.05f; 
            rotation = Matrix4X4.CreateRotationY(yaw);
        }

        public Vector3D<float> GetPlayerForward()
        {
            return new Vector3D<float>(
                -MathF.Cos(yaw),  
                0,
                MathF.Sin(yaw)
            );
        }
        public Vector3D<float> GetPlayerRight()
        {
            return new Vector3D<float>(
                -MathF.Sin(yaw),
                0,
                -MathF.Cos(yaw)
            );
        }

        public void resetCharacter()
        {
            transfLegs = deftransfLegs;
            transfHead = deftransfHead;
        }

        public void startIdleAnimation()
        {
            prevtransfHead = transfHead;
        }

    }
}
