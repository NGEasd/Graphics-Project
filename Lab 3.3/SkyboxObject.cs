using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LabirintusProjekt
{
    internal class SkyboxObject: IDisposable
    {
        private bool disposedValue;

        public uint Vao { get; private set; }
        public uint Vertices { get; private set; }
        public uint Colors { get; private set; }
        public uint? Texture { get; private set; } = new uint?();
        public uint Indices { get; private set; }
        public uint IndexArrayLength { get; private set; }

        private GL Gl;

        public unsafe static SkyboxObject CreateSkyBox(GL Gl)
        {
            // counter clockwise is front facing
            // vx, vy, vz, nx, ny, nz, tu, tv
            float[] vertexArray = new float[] {
                // top face
                -0.5f, 0.5f, 0.5f, 0f, -1f, 0f, 1f/4f, 0f/3f,
                0.5f, 0.5f, 0.5f, 0f, -1f, 0f, 2f/4f, 0f/3f,
                0.5f, 0.5f, -0.5f, 0f, -1f, 0f, 2f/4f, 1f/3f,
                -0.5f, 0.5f, -0.5f, 0f, -1f, 0f, 1f/4f, 1f/3f,

                // front face
                -0.5f, 0.5f, 0.5f, 0f, 0f, -1f, 1, 1f/3f,
                -0.5f, -0.5f, 0.5f, 0f, 0f, -1f, 4f/4f, 2f/3f,
                0.5f, -0.5f, 0.5f, 0f, 0f, -1f, 3f/4f, 2f/3f,
                0.5f, 0.5f, 0.5f, 0f, 0f, -1f,  3f/4f, 1f/3f,

                // left face
                -0.5f, 0.5f, 0.5f, 1f, 0f, 0f, 0, 1f/3f,
                -0.5f, 0.5f, -0.5f, 1f, 0f, 0f,1f/4f, 1f/3f,
                -0.5f, -0.5f, -0.5f, 1f, 0f, 0f, 1f/4f, 2f/3f,
                -0.5f, -0.5f, 0.5f, 1f, 0f, 0f, 0f/4f, 2f/3f,

                // bottom face
                -0.5f, -0.5f, 0.5f, 0f, 1f, 0f, 1f/4f, 1f,
                0.5f, -0.5f, 0.5f,0f, 1f, 0f, 2f/4f, 1f,
                0.5f, -0.5f, -0.5f,0f, 1f, 0f, 2f/4f, 2f/3f,
                -0.5f, -0.5f, -0.5f,0f, 1f, 0f, 1f/4f, 2f/3f,

                // back face
                0.5f, 0.5f, -0.5f, 0f, 0f, 1f, 2f/4f, 1f/3f,
                -0.5f, 0.5f, -0.5f, 0f, 0f, 1f, 1f/4f, 1f/3f,
                -0.5f, -0.5f, -0.5f,0f, 0f, 1f, 1f/4f, 2f/3f,
                0.5f, -0.5f, -0.5f,0f, 0f, 1f, 2f/4f, 2f/3f,

                // right face
                0.5f, 0.5f, 0.5f, -1f, 0f, 0f, 3f/4f, 1f/3f,
                0.5f, 0.5f, -0.5f,-1f, 0f, 0f, 2f/4f, 1f/3f,
                0.5f, -0.5f, -0.5f, -1f, 0f, 0f, 2f/4f, 2f/3f,
                0.5f, -0.5f, 0.5f, -1f, 0f, 0f, 3f/4f, 2f/3f,
            };

            float[] colorArray = new float[] {
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, 0.0f, 1.0f,
            };

            uint[] indexArray = new uint[] {
                0, 2, 1,
                0, 3, 2,

                4, 6, 5,
                4, 7, 6,

                8, 10, 9,
                10, 8, 11,

                12, 13, 14,
                12, 14, 15,

                17, 19, 16,
                17, 18, 19,

                20, 21, 22,
                20, 22, 23
            };

            var skyboxImage = ReadTextureImage("desert_skybox.jpg");

            return CreateObjectDescriptorFromArrays(Gl, vertexArray, colorArray, indexArray, skyboxImage);
        }

        private static unsafe SkyboxObject CreateObjectDescriptorFromArrays(GL Gl, float[] vertexArray, float[] colorArray, uint[] indexArray,
            ImageResult textureImage = null)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            // 0 is position
            // 2 is normals
            // 3 is texture
            uint offsetPos = 0;
            uint offsetNormals = offsetPos + 3 * sizeof(float);
            uint offsetTexture = offsetNormals + 3 * sizeof(float);
            uint vertexSize = offsetTexture + (textureImage == null ? 0u : 2 * sizeof(float));

            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, vertexSize, (void*)offsetNormals);
            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetTexture);
            Gl.EnableVertexAttribArray(3);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);


            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            // 1 is color
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

            uint? texture = new uint?();

            if (textureImage != null)
            {
                // set texture
                // create texture
                texture = Gl.GenTexture();

                // activate texture 0
                Gl.ActiveTexture(TextureUnit.Texture0);
                // bind texture
                Gl.BindTexture(TextureTarget.Texture2D, texture.Value);
                // Here we use "result.Width" and "result.Height" to tell OpenGL about how big our texture is.
                Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)textureImage.Width,
                    (uint)textureImage.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (ReadOnlySpan<byte>)textureImage.Data.AsSpan());
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                // unbinde texture
                Gl.BindTexture(TextureTarget.Texture2D, 0);
            }

            return new SkyboxObject() { Vao = vao, Vertices = vertices, Colors = colors, Indices = indices, IndexArrayLength = (uint)indexArray.Length, Gl = Gl, Texture = texture };
        }

        private static unsafe ImageResult ReadTextureImage(string textureResource)
        {
            ImageResult result;
            using (Stream skyeboxStream
                = typeof(SkyboxObject).Assembly.GetManifestResourceStream("LabirintusProjekt.Resources." + textureResource))
                result = ImageResult.FromStream(skyeboxStream, ColorComponents.RedGreenBlueAlpha);

            return result;
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

        ~SkyboxObject()
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
