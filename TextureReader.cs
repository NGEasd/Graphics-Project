using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Silk.NET.Maths;

namespace LabirintusProjekt
{
    public class TextureReader
    {
        public float[] VertexArray { get; private set; }
        public uint[] IndexArray { get; private set; }

        public bool ReadObjFile(string objPath)
        {
            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();
            uint indexCounter = 0;

            try
            {
                using (Stream objStream = typeof(TextureReader).Assembly.GetManifestResourceStream(objPath))
                using (StreamReader objReader = new StreamReader(objStream))
                {
                    List<Vector3D<float>> objVertices = new List<Vector3D<float>>();
                    List<Vector2D<float>> objTexCoords = new List<Vector2D<float>>();
                    List<Vector3D<float>> objNormals = new List<Vector3D<float>>();
                    List<Face> objFaces = new List<Face>();
                    string mtlFilename = "";

                    while (!objReader.EndOfStream)
                    {
                        var line = objReader.ReadLine()?.Trim();
                        if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                            continue;

                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 0) continue;

                        switch (parts[0])
                        {
                            case "v":
                                objVertices.Add(new Vector3D<float>(
                                    float.Parse(parts[1], CultureInfo.InvariantCulture),
                                    float.Parse(parts[2], CultureInfo.InvariantCulture),
                                    float.Parse(parts[3], CultureInfo.InvariantCulture)
                                ));
                                break;
                            case "vt":
                                objTexCoords.Add(new Vector2D<float>(
                                    float.Parse(parts[1], CultureInfo.InvariantCulture),
                                    float.Parse(parts[2], CultureInfo.InvariantCulture)
                                ));
                                break;
                            case "vn":
                                objNormals.Add(new Vector3D<float>(
                                    float.Parse(parts[1], CultureInfo.InvariantCulture),
                                    float.Parse(parts[2], CultureInfo.InvariantCulture),
                                    float.Parse(parts[3], CultureInfo.InvariantCulture)
                                ));
                                break;
                            case "f":
                                Face face = new Face();
                                foreach (var vertexData in parts.Skip(1))
                                {
                                    var indicesStr = vertexData.Split('/');
                                    face.VertexIndices.Add(int.Parse(indicesStr[0], CultureInfo.InvariantCulture) - 1);
                                    if (indicesStr.Length > 1 && !string.IsNullOrEmpty(indicesStr[1]))
                                        face.TexCoordIndices.Add(int.Parse(indicesStr[1], CultureInfo.InvariantCulture) - 1);
                                    if (indicesStr.Length > 2 && !string.IsNullOrEmpty(indicesStr[2]))
                                        face.NormalIndices.Add(int.Parse(indicesStr[2], CultureInfo.InvariantCulture) - 1);
                                }
                                objFaces.Add(face);
                                break;
                            case "mtllib":
                                mtlFilename = parts[1];
                                break;
                        }
                    }

                    foreach (var face in objFaces)
                    {
                        for (int i = 0; i < face.VertexIndices.Count; i++)
                        {
                            int vertexIndex = face.VertexIndices[i];
                            vertices.Add(objVertices[vertexIndex].X);
                            vertices.Add(objVertices[vertexIndex].Y);
                            vertices.Add(objVertices[vertexIndex].Z);

                            if (face.NormalIndices.Count > 0 && i < face.NormalIndices.Count)
                            {
                                int normalIndex = face.NormalIndices[i];
                                vertices.Add(objNormals[normalIndex].X);
                                vertices.Add(objNormals[normalIndex].Y);
                                vertices.Add(objNormals[normalIndex].Z);
                            }
                            else
                            {
                                vertices.Add(0.0f); // Default normal X
                                vertices.Add(1.0f); // Default normal Y (pointing up)
                                vertices.Add(0.0f); // Default normal Z
                            }

                            if (face.TexCoordIndices.Count > 0 && i < face.TexCoordIndices.Count)
                            {
                                int texCoordIndex = face.TexCoordIndices[i];
                                vertices.Add(objTexCoords[texCoordIndex].X);
                                vertices.Add(objTexCoords[texCoordIndex].Y);
                            }
                            else
                            {
                                vertices.Add(0.0f); // Default texture U
                                vertices.Add(0.0f); // Default texture V
                            }
                            indices.Add(indexCounter++);
                        }
                    }

                    VertexArray = vertices.ToArray();
                    IndexArray = indices.ToArray();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Hiba történt az OBJ fájl olvasása közben: " + e.Message);
                return false;
            }
        }
    }

    public class Face
    {
        public List<int> VertexIndices { get; } = new List<int>();
        public List<int> TexCoordIndices { get; } = new List<int>();
        public List<int> NormalIndices { get; } = new List<int>();
    }
}