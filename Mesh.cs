using System;
using OpenTK.Graphics.ES30;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using OpenTK.Input;
using System.Security.Cryptography.X509Certificates;

namespace Voxelrendering2
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal ;
        public Vector3 Color;
    }
    public class Mesh
    {
        public Vertex[] vertices;
        public uint[] indices;
        public Vector3 pos;

        public Mesh(Vertex[] vertices, uint[] indices)
        {
            CalculateNormals(vertices, indices);
            this.vertices = vertices;
            this.indices = indices;
            InitializeVAO();
        }
        private void InitializeVAO()
        {
            List<float> bufferdata = new List<float>();

            for (int i = 0; i < vertices.Length; i++) 
            {
                bufferdata.Add(vertices[i].Position.X);
                bufferdata.Add(vertices[i].Position.Y);
                bufferdata.Add(vertices[i].Position.Z);
                bufferdata.Add(vertices[i].Normal.X);
                bufferdata.Add(vertices[i].Normal.Y);
                bufferdata.Add(vertices[i].Normal.Z);
                bufferdata.Add(vertices[i].Color.X);
                bufferdata.Add(vertices[i].Color.Y);
                bufferdata.Add(vertices[i].Color.Z);
            }
            int projectedSize = bufferdata.Count * sizeof(float);
            int actualSize = bufferdata.ToArray().Length * sizeof(float);
            GL.BufferData(BufferTarget.ArrayBuffer, bufferdata.Count * sizeof(float), bufferdata.ToArray(), BufferUsageHint.StaticDraw);
            //Console.WriteLine($"Projected Size: {projectedSize} bytes");
            //Console.WriteLine($"Actual Size: {actualSize} bytes");
        }
        //onlyvertsnoclor
        public static Vector3[] CalculateNormals(Vertex[] verticies, uint[] indices)
        {
            // Initialize normals to zero
            Vector3[] normals = new Vector3[verticies.Length];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.Zero;
            }

            // Compute face normals and accumulate to vertex normals
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

                // Accumulate face normals to vertex normals
                normals[i0] += faceNormal;
                normals[i1] += faceNormal;
                normals[i2] += faceNormal;
            }

            // Normalize vertex normals
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.Normalize(normals[i]);
            }

            return normals;
        }
    }
}