using Silk.NET.OpenGL;

namespace LabirintusProjekt
{
    internal class WallObject : IDisposable
    {
        private bool disposedValue;

        public uint Vao { get; private set; }
        public uint Vertices { get; private set; }
        public uint Colors { get; private set; }
        public uint Indices { get; private set; }
        public uint IndexArrayLength { get; private set; }

        private GL Gl;

        // color array in constructor
        public unsafe static WallObject CreateCube(GL Gl)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            // counter clockwise is front facing
            var wallVertices = new float[] {

            // hátsó oldal
            0f, 1.5f, 1f, 0f, 0f, 1f,
            1f, 1.5f, 1f, 0f, 0f, 1f,
            1f, 1.5f, 0f, 0f, 0f, 1f,
            0f, 1.5f, 0f, 0f, 0f, 1f,

            // felső oldal
            0f, 1.5f, 1f, 0f, 1f, 0f,
            0f, -1.5f, 1f, 0f, 1f, 0f,
            1f, -1.5f, 1f, 0f, 1f, 0f,
            1f, 1.5f, 1f, 0f, 1f, 0f,

            // bal oldal
            0f, 1.5f, 1f, -1f, 0f, 0f,
            0f, 1.5f, 0f, -1f, 0f, 0f,
            0f, -1.5f, 0f, -1f, 0f, 0f,
            0f, -1.5f, 1f, -1f, 0f, 0f,

            // elülső oldal
            0f, -1.5f, 1f, 0f, 0f, -1f,
            1f, -1.5f, 1f, 0f, 0f, -1f,
            1f, -1.5f, 0f, 0f, 0f, -1f,
            0f, -1.5f, 0f, 0f, 0f, -1f,

            // alsó oldal
            1f, 1.5f, 0f, 0f, -1f, 0f,
            0f, 1.5f, 0f, 0f, -1f, 0f,
            0f, -1.5f, 0f, 0f, -1f, 0f,
            1f, -1.5f, 0f, 0f, -1f, 0f,

            // jobb oldal
            1f, 1.5f, 1f, 1f, 0f, 0f,
            1f, 1.5f, 0f, 1f, 0f, 0f,
            1f, -1.5f, 0f, 1f, 0f, 0f,
            1f, -1.5f, 1f, 1f, 0f, 0f
        };


            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3,

                4, 5, 6,
                4, 6, 7,

                8, 9, 10,
                10, 11, 8,

                12, 14, 13,
                12, 15, 14,

                17, 16, 19,
                17, 19, 18,

                20, 22, 21,
                20, 23, 22
            };

            float[] colorArray = Enumerable.Repeat(new float[] { 0.5f, 0.5f, 0.5f, 1.0f }, 24)
                               .SelectMany(x => x)
                               .ToArray();

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)wallVertices.AsSpan(), GLEnum.StaticDraw);

            // 0 - pozíció
            // 2 - normálok
            uint offsetPos = 0;
            uint offsetNormals = offsetPos + 3 * sizeof(float);
            uint vertexSize = offsetNormals + 3 * sizeof(float);

            // Pozíció beállítása
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            // Normál beállítása
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, vertexSize, (void*)offsetNormals);
            Gl.EnableVertexAttribArray(2);

            // Színek VBO létrehozása és bindelése
            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);

            // 1 - szín
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            // Indexek VBO létrehozása és bindelése
            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            // Bufferek leválasztása
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            // Visszatérünk a WallObject-el
            return new WallObject() { Vao = vao, Vertices = vertices, Colors = colors, Indices = indices, IndexArrayLength = (uint)indexArray.Length, Gl = Gl };
        }

        public unsafe static WallObject CreateFloor(GL Gl)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            // counter clockwise is front facing
            var corners = new float[] {
                -100, 0, -100, 0, 1, 0,
                100, 0, -100, 0, 1, 0,
                100, 0, 100, 0, 1, 0,
                -100, 0, 100, 0, 1, 0
            };


            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3,
            };

            float[] colorArray = Enumerable.Repeat(new float[] { 0.8f, 0.6f, 0.4f, 1.0f }, 4)
                               .SelectMany(c => c)
                               .ToArray();

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)corners.AsSpan(), GLEnum.StaticDraw);

            // 0 - pozíció
            // 2 - normálok
            uint offsetPos = 0;
            uint offsetNormals = offsetPos + 3 * sizeof(float);
            uint vertexSize = offsetNormals + 3 * sizeof(float);

            // Pozíció beállítása
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            // Normál beállítása
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, vertexSize, (void*)offsetNormals);
            Gl.EnableVertexAttribArray(2);

            // Színek VBO létrehozása és bindelése
            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);

            // 1 - szín
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            // Indexek VBO létrehozása és bindelése
            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            // Bufferek leválasztása
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            // Visszatérünk a WallObject-el
            return new WallObject() { Vao = vao, Vertices = vertices, Colors = colors, Indices = indices, IndexArrayLength = (uint)indexArray.Length, Gl = Gl };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null


                // always unbound the vertex buffer first, so no halfway results are displayed by accident
                Gl.DeleteBuffer(Vertices);
                Gl.DeleteBuffer(Colors);
                Gl.DeleteBuffer(Indices);
                Gl.DeleteVertexArray(Vao);

                disposedValue = true;
            }
        }

        ~WallObject()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
