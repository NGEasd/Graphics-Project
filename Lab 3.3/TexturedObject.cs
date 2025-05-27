using Silk.NET.OpenGL;
using StbImageSharp;

namespace LabirintusProjekt
{
    internal class TexturedObject : IDisposable
    {
        private bool disposedValue;

        public uint Vao { get; private set; }
        public uint Vertices { get; private set; }
        public uint Colors { get; private set; }
        public uint? Texture { get; private set; } = new uint?();
        public uint Indices { get; private set; }
        public uint IndexArrayLength { get; private set; }

        private GL Gl;

        public unsafe static TexturedObject CreateSolider(GL Gl)
        {
            TextureReader reader = new TextureReader();
            if (!reader.ReadObjFile("LabirintusProjekt.Resources.textured_solider.obj"))
            {
                return null;
            }

            var image = ReadTextureImage("armor_texture.jpg");

            return CreateObjectDescriptorFromArrays(Gl, reader.VertexArray, reader.IndexArray, image);
        }

        private static unsafe TexturedObject CreateObjectDescriptorFromArrays(GL Gl, float[] interleavedVertexData, uint[] indexArray,
    ImageResult textureImage = null)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            uint vbo = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)interleavedVertexData.AsSpan(), GLEnum.StaticDraw);

            // A vertex adatok most összefűzve tartalmazzák: pozíció (3), normál (3), textúrakoordináta (2) = 8 float
            uint stride = 8 * sizeof(float);
            uint offsetPos = 0;
            uint offsetNormals = 3 * sizeof(float);
            uint offsetTexture = 6 * sizeof(float);

            // Pozíció attribútum (location = 0)
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            // Normál attribútum (location = 2)
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, (void*)offsetNormals);
            Gl.EnableVertexAttribArray(2);

            // Textúrakoordináta attribútum (location = 3)
            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, stride, (void*)offsetTexture);
            Gl.EnableVertexAttribArray(3);

            uint ebo = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            uint? texture = new uint?();

            if (textureImage != null)
            {
                texture = Gl.GenTexture();
                Gl.ActiveTexture(TextureUnit.Texture0);
                Gl.BindTexture(TextureTarget.Texture2D, texture.Value);

                Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)textureImage.Width,
                    (uint)textureImage.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (ReadOnlySpan<byte>)textureImage.Data.AsSpan());

                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                Gl.BindTexture(TextureTarget.Texture2D, 0);
            }

            return new TexturedObject()
            {
                Vao = vao,
                Vertices = vbo, // A VBO az összes vertex adatot tartalmazza
                Indices = ebo,    // Az EBO tartalmazza az indexeket
                IndexArrayLength = (uint)indexArray.Length,
                Gl = Gl,
                Texture = texture
            };
        }

        private static unsafe ImageResult ReadTextureImage(string textureResource)
        {
            ImageResult result;
            using (Stream skyeboxStream
                = typeof(TexturedObject).Assembly.GetManifestResourceStream("LabirintusProjekt.Resources." + textureResource))
                result = ImageResult.FromStream(skyeboxStream, ColorComponents.RedGreenBlueAlpha);

            return result;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: manage the state of managed objects
                }

                Gl.DeleteBuffer(Vertices);
                Gl.DeleteBuffer(Colors);
                Gl.DeleteBuffer(Indices);
                Gl.DeleteVertexArray(Vao);
                if (Texture.HasValue)
                {
                    Gl.DeleteTexture(Texture.Value);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if Dispose(bool disposing) above has code to free unmanaged resources
        // ~ModelObjectDescriptor()
        // {
        //      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //      Dispose(disposing: false);
        // }

    }
}