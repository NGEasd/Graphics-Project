using Silk.NET.Maths;
using Silk.NET.OpenGL;
using LabirintusProjekt;

namespace Labirintus_projekt
{
    class LabirintComponents
    {
        public List<GlObject> windows;
        public List<Solider> soliders;
        public List<WallObject> wallList;
        public List<Matrix4X4<float>> windowTransformations;
        public List<Matrix4X4<float>> wallTransformations;
        private GL Gl;

        private readonly int[] directionX = new int[] { -1, 0, 1, 0 }; // bal, fel, jobb, le
        private readonly int[] directionY = new int[] { 0, -1, 0, 1 };

        public LabirintComponents(LabirintMap labirintMap, GL gl)
        {
            windows = new List<GlObject>();
            soliders = new List<Solider>();
            wallList = new List<WallObject>();
            windowTransformations = new List<Matrix4X4<float>>();
            wallTransformations = new List<Matrix4X4<float>>();
            this.Gl = gl;

            // felepitjuk a map-et
            for (int x = 0; x < labirintMap.Height; x++)
            {
                for (int y = 0; y < labirintMap.Width; y++)
                {
                    if (labirintMap.Get(x, y) == 1)
                    {
                        WallObject wall = WallObject.CreateCube(Gl);
                        wallList.Add(wall);

                        var translation = Matrix4X4.CreateTranslation(x * 1f, 1.25f, y * 1f);
                        wallTransformations.Add(translation);
                    }

                    if (labirintMap.Get(x, y) == 7)
                    {
                        Solider solider = new Solider(Gl, new Vector3D<float>(x, 0f, y));
                        soliders.Add(solider);
                    }

                    if (labirintMap.Get(x, y) == 9)
                    { 

                        var window = ObjResourceReader.CreateObject(Gl, [0.5f, 0.5f, 0.5f, 1.0f], "LabirintusProjekt.Resources.bars.obj");
                        windows.Add(window);

                        // kotelezo transzformaciok
                        var rotation = Matrix4X4.CreateRotationX(-(float)Math.PI / 2);
                        var resize  = Matrix4X4.CreateTranslation(-8.0f, -7.1f, 0.0f);
                        var translation = Matrix4X4.CreateTranslation(x, 0f, y);

                        // nem tudom, melyik sik szerint kell forgatni, talan Z?
                        var placementRotation = Matrix4X4.CreateRotationZ(0f);
                        if (needsToRotate(labirintMap, x, y))
                        {
                            placementRotation = Matrix4X4.CreateRotationY((float)Math.PI / 2);
                            translation = Matrix4X4.CreateTranslation(x, 0f, y + 1f);
                        }
                        
                        var transformation = resize * rotation * placementRotation * translation;

                        windowTransformations.Add(transformation);
                    }
                }
            }
        }

        private bool needsToRotate(LabirintMap lab, int x, int y)
        {
            for (int i = 0; i < 4; i++)
            {
                if (lab.IsInside(x + directionX[i], y + directionY[i]))
                {
                    if (lab.IsWall(x + directionX[i], y + directionY[i]))
                    {
                        if (i % 2 == 0) return false;
                        return true;
                    }
                }
            }

            return false;
        }

        public void Dispose()
        {
            foreach (var wall in wallList)
            {
                wall.Dispose();
            }
            wallList.Clear();
        }
    }
}
