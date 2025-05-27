using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace LabirintusProjekt
{
    class Soldier
    {
        public GL gl;
        public TexturedObject body;
        private TextureReader reader;

        public Vector3D<float> Position { get; set; } = new Vector3D<float>(0, 0, 0);
        public Matrix4X4<float> transformation;

        public Vector3D<float> Direction { get; private set; } = new Vector3D<float>(1, 0, 0);
        public float Speed { get; private set; }
        private Random random = new Random();

        public Soldier(GL gl, Vector3D<float> startPosition, string textureFilename)
        {
            this.gl = gl;
            transformation = Matrix4X4.CreateTranslation(Position);
            Position = startPosition;
            transformation = Matrix4X4.CreateTranslation(Position);
            reader = new TextureReader();
            if (reader.ReadObjFile("LabirintusProjekt.Resources.textured_solider.obj"))
            {
                body = TexturedObject.CreateSolider(gl);
            }
            else
            {
                Console.WriteLine("Nem sikerült betölteni a katona modelljét.");
                body = null; 
            }

            SetRandomDirection();
            SetRandomSpeed(0.8f, 3f);
        }

        private void SetRandomDirection()
        {
            int randomDirection = random.Next(4);

            switch (randomDirection)
            {
                case 0:
                    Direction = new Vector3D<float>(1, 0, 0);
                    break;
                case 1:
                    Direction = new Vector3D<float>(-1, 0, 0);
                    break;
                case 2:
                    Direction = new Vector3D<float>(0, 0, 1);
                    break;
                case 3:
                    Direction = new Vector3D<float>(0, 0, -1);
                    break;
            }
        }

        private void SetRandomSpeed(float minSpeed, float maxSpeed)
        {
            Speed = (float)(random.NextDouble() * (maxSpeed - minSpeed) + minSpeed);
        }

        public void UpdatePosition(double deltaTime, Func<Vector3D<float>, bool> isCollision)
        {
            Vector3D<float> newPosition = Position + Direction * Speed * (float)deltaTime;

            if (isCollision(newPosition))
            {
                SetRandomDirection();
            }
            else
            {
                Position = newPosition;
            }

            transformation = Matrix4X4.CreateTranslation(Position);
        }
    }
}