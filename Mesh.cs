using System;
using OpenTK.Graphics.ES30;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using OpenTK.Input;
using System.Security.Cryptography.X509Certificates;
using System.Linq;

namespace Voxelrendering2
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Color;
    }
    public class Mesh
    {
        public Vertex[] vertices;
        public uint[] indices;
        public Vector3 pos;
        public int MeshIndex;

        public Mesh(Vertex[] vertices, uint[] indices)
        {
            MeshIndex = GenerateMeshIndex(this);
            setNormals(ref vertices, indices);
            this.vertices = vertices;
            this.indices = indices;

        }
        public int GenerateMeshIndex(Mesh mesh)
        {
            System.Random rand = new System.Random();


            return rand.Next(int.MaxValue);
        }

        public static Vector3[] CalculateNormals(Vertex[] verticies, uint[] indices)
        {
            Vector3[] normals = new Vector3[verticies.Length];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.Zero;
            }

            for (int i = 0; i < indices.Length; i += 3)
            {
                int i0 = (int)indices[i];
                int i1 = (int)indices[i + 1];
                int i2 = (int)indices[i + 2];

                Vector3 v0 = verticies[i0].Position;
                Vector3 v1 = verticies[i1].Position;
                Vector3 v2 = verticies[i2].Position;

                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;
                Vector3 faceNormal = Vector3.Cross(edge1, edge2);

                normals[i0] += faceNormal;
                normals[i1] += faceNormal;
                normals[i2] += faceNormal;
            }

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.Normalize(normals[i]);
            }
            return normals;
        }

        public void setNormals(ref Vertex[] verticies, uint[] indices)
        {
            Vector3[] Normals = CalculateNormals(verticies, indices);
            for (int i = 0; i < Normals.Length; i++)
            {
                verticies[i].Normal = Normals[i];
            }
        }

    }
}