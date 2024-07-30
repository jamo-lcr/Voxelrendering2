using System;
using OpenTK.Graphics.ES30;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using OpenTK.Input;

namespace Voxelrendering2
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
    }
    public class Mesh
    {
        public Vector3 pos;
        public float[] vertices;
        public uint[] indices;
        public Vector3[] Normals;

        public int VertexArrayObject;
        public int ElementBufferObject;


        public Mesh(float[] vertices, uint[] indices)
        {
            this.vertices = vertices;
            this.indices = indices;

            // Initialize VAO and EBO during the constructor
            InitializeVAO();
        }
        private void InitializeVAO()
        {
            // Generate VAO
            GL.GenVertexArrays(1, out VertexArrayObject);
            GL.BindVertexArray(VertexArrayObject);

            // Generate EBO
            GL.GenBuffers(1, out ElementBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticRead);

            // Generate VBO for vertices
            int vertexBufferObject;
            GL.GenBuffers(1, out vertexBufferObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);

            // Calculate normals and append them to the vertices array
            List<float> vertsWithNormals = new List<float>();
            Vector3[] normals = CalculateNormalsWithColor(vertices, indices);

            for (int i = 0; i < vertices.Length; i += 6)
            {
                vertsWithNormals.Add(vertices[i]);
                vertsWithNormals.Add(vertices[i + 1]);
                vertsWithNormals.Add(vertices[i + 2]);

                // Append normal data
                vertsWithNormals.Add(normals[i / 6].X);
                vertsWithNormals.Add(normals[i / 6].Y);
                vertsWithNormals.Add(normals[i / 6].Z);

                // Append color data
                vertsWithNormals.Add(vertices[i + 3]);
                vertsWithNormals.Add(vertices[i + 4]);
                vertsWithNormals.Add(vertices[i + 5]);
            }

            GL.BufferData(BufferTarget.ArrayBuffer, vertsWithNormals.Count * sizeof(float), vertsWithNormals.ToArray(), BufferUsageHint.StaticDraw);

            // Set up vertex attribute pointers (assuming position, normal, and color data)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);

            // Enable and set up vertex attribute pointer for normals
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));

            // Enable and set up vertex attribute pointer for colors
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 6 * sizeof(float));

            // Unbind VBO and VAO to prevent accidental modifications
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
        //onlyvertsnoclor
        public static Vector3[] CalculateNormalsWithColor(float[] verts, uint[] indices)
        {
            List<Vector3> normals = new List<Vector3>();

            // Initialize normals
            for (int i = 0; i < verts.Length; i += 6)
            {
                normals.Add(Vector3.Zero);
            }

            // Compute face normals and accumulate to vertex normals
            for (int i = 0; i < indices.Length; i += 3)
            {
                int i0 = (int)indices[i] * 6;
                int i1 = (int)indices[i + 1] * 6;
                int i2 = (int)indices[i + 2] * 6;

                Vector3 v0 = new Vector3(verts[i0], verts[i0 + 1], verts[i0 + 2]);
                Vector3 v1 = new Vector3(verts[i1], verts[i1 + 1], verts[i1 + 2]);
                Vector3 v2 = new Vector3(verts[i2], verts[i2 + 1], verts[i2 + 2]);

                Vector3 faceNormal = Vector3.Cross(v1 - v0, v2 - v0);

                // Accumulate face normals to vertex normals
                normals[(int)indices[i]] += faceNormal;
                normals[(int)indices[i + 1]] += faceNormal;
                normals[(int)indices[i + 2]] += faceNormal;
            }

            // Normalize vertex normals
            for (int i = 0; i < normals.Count; i++)
            {
                normals[i] = Vector3.Normalize(normals[i]);
            }

            return normals.ToArray();
        }
    }
}
