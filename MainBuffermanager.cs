using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using Voxelrendering2;

namespace OpentkVoxelRendererAufräumen
{
    public struct Bufferholder()
    {
        public int _bufferObject;
        //a startindex, b endindex, c meshindex
        public List<Vector3i> usedmemory = new List<Vector3i>();
    }
    public class MainBuffermanager
    {
        public Bufferholder _vertexBuffer;
        public Bufferholder _elementBuffer;

        public int _elementcount = 0;

        public void Genmainbuffers()
        {
            _vertexBuffer = new Bufferholder();
            _elementBuffer = new Bufferholder();

            _elementBuffer._bufferObject = GL.GenBuffer();
            _vertexBuffer._bufferObject = GL.GenBuffer();

            int elementBufferSize = 316108288; 
            int vertexBufferSize = 46003048;   

            setmaxbuffersize(_elementBuffer, _vertexBuffer, elementBufferSize, vertexBufferSize);
        }

        public void setmaxbuffersize(Bufferholder bufferA, Bufferholder bufferB, int bufferAsize, int bufferBsize)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferA._bufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, bufferAsize, IntPtr.Zero, BufferUsageHint.StreamDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferB._bufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, bufferBsize, IntPtr.Zero, BufferUsageHint.StreamDraw);
        }
        public void Setdata(Vertex[] vertices, uint[] elementbufferdata)
        {
            _elementcount=elementbufferdata.Length;
            GL.BindVertexArray(Renderer._vertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer._bufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, elementbufferdata.Length * sizeof(uint), elementbufferdata, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer._bufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float) * 9, vertices, BufferUsageHint.StreamDraw);

        }
        public void AddMeshData(Mesh mesh)
        {
            Vertex[] bufferdata = new Vertex[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                bufferdata[i].Position.X = mesh.vertices[i].Position.X+mesh.pos.X;
                bufferdata[i].Position.Y = mesh.vertices[i].Position.Y+ mesh.pos.Y;
                bufferdata[i].Position.Z = mesh.vertices[i].Position.Z+ mesh.pos.Z;
                bufferdata[i].Normal.X = mesh.vertices[i].Normal.X;
                bufferdata[i].Normal.Y = mesh.vertices[i].Normal.Y;
                bufferdata[i].Normal.Z = mesh.vertices[i].Normal.Z;
                bufferdata[i].Color.X = mesh.vertices[i].Color.X;
                bufferdata[i].Color.Y = mesh.vertices[i].Color.Y;
                bufferdata[i].Color.Z = mesh.vertices[i].Color.Z;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer._bufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer._bufferObject);

            Vector2i freepos = findfreeposition(_vertexBuffer, _elementBuffer, mesh.vertices.Length * sizeof(float) * 9, mesh.indices.Length * sizeof(uint));
            int vertexDataSize = mesh.vertices.Length * sizeof(float) *9; 
            int indexDataSize = mesh.indices.Length * sizeof(uint);        

            uint vertexOffset = (uint)(freepos.X / (sizeof(float) *9)); 

            uint[] adjustedIndices = mesh.indices.Select(index => index + vertexOffset).ToArray();

            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)freepos.X, vertexDataSize, bufferdata);

            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)freepos.Y, indexDataSize, adjustedIndices);

            _elementcount = Math.Max(_elementcount, (int)(freepos.Y + indexDataSize) / sizeof(uint));
            _vertexBuffer.usedmemory.Add(new Vector3i(freepos.X, freepos.X + vertexDataSize, mesh.MeshIndex));
            _elementBuffer.usedmemory.Add(new Vector3i(freepos.Y, freepos.Y + indexDataSize, mesh.MeshIndex));
        }
        public Vector2i findfreeposition(Bufferholder bufferA,Bufferholder bufferB,int sizedataA,int sizedataB)
        {
            int freePositionA = FindFreePositionInBuffer(bufferA, sizedataA);
            int freePositionB = FindFreePositionInBuffer(bufferB, sizedataB);
            return new Vector2i(freePositionA, freePositionB);
        }
        private int FindFreePositionInBuffer(Bufferholder buffer, int size)
        {
            buffer.usedmemory.Sort((a, b) => a.X.CompareTo(b.X));
            int currentPosition = 0;

            foreach (Vector3i segment in buffer.usedmemory)
            {
                if (segment.X - currentPosition >= size)
                {
                    return currentPosition; 
                }
                currentPosition = segment.Y;
            }
            return currentPosition;
        }
        public void addMesh(Mesh chunk)
        {

            Renderer.activeScene.Meshes.Add(chunk);
            AddMeshData(chunk);

        }

        public void removemesh(Mesh chunk)
        {   
            int chunkIndex = Renderer.activeScene.Meshes.IndexOf(chunk);
            Renderer.activeScene.Meshes.RemoveAt(chunkIndex);
            RemoveMeshdatafrombuffer(chunk.MeshIndex);

        }
        public void RemoveMeshdatafrombuffer(int meshIndex)
        {
            Vector3i meshVertexData = _vertexBuffer.usedmemory.FirstOrDefault(v => v.Z == meshIndex);
            Vector3i meshElementData = _elementBuffer.usedmemory.FirstOrDefault(v => v.Z == meshIndex);

            if (meshVertexData != null)
            {
                int vertexDataSize = meshVertexData.Y - meshVertexData.X;
                int vertexOffset = meshVertexData.X;
                float[] zeroedVertexData = new float[vertexDataSize / sizeof(float)];

                // Clear the removed mesh data and usedmemory fakepointer 
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer._bufferObject);
                GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)vertexOffset, vertexDataSize, zeroedVertexData);
                _vertexBuffer.usedmemory.Remove(meshVertexData);
            }
            else
            {
                return;
            }

            if (meshElementData != null)
            {
                int indexDataSize = meshElementData.Y - meshElementData.X;
                int indexOffset = meshElementData.X;
                uint[] zeroedIndexData = new uint[indexDataSize / sizeof(uint)];

                // Clear the removed mesh data and usedmemory fakepointer 
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer._bufferObject);
                GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)indexOffset, indexDataSize, zeroedIndexData);

                _elementBuffer.usedmemory.Remove(meshElementData);
            }
            else
            {
                return;
            }
        }

    }


}

