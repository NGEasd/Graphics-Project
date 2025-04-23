using GrafikaSzeminarium;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labirintus_projekt
{
    class LabirintWalls
    {
        public List<WallObject> wallList;
        public List<Matrix4X4<float>> transformations;
        private GL Gl;

        public LabirintWalls(LabirintMap labirintMap, GL gl)
        {

            wallList = new List<WallObject>();
            transformations = new List<Matrix4X4<float>>();
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

                        var translation = Matrix4X4.CreateTranslation(x * 1f, 0f, y * 1f);
                        transformations.Add(translation);
                    }
                }
            }
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
