using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenTK.Graphics.OpenGL.GL;

namespace Voxelrendering2
{
    internal class Chunkgen
    {
        Vector3i size;
        public List<Vector3i> vertstowichcube= new List<Vector3i>();
        public List<float[]> cubeidcolor = new List<float[]>();
        public List<Vector3> verts= new List<Vector3>();
        public List<uint> indicies= new List<uint>();
        
        public Chunkgen(int x,int y,int z) 
        {
            size.X = x;
            size.Y = y;
            size.Z = z;
        }
        public void generateterrainheight(ref short[,,] terrainheight)
        {

            float[,] heights  = Getnoise.getheightfromnoisemap(size,new Vector3i(0,0,0));
            for(int x = 0; x < size.X; x++)
            {
                for(int z = 0; z < size.Z; z++)
                {
                    short height = (short)(heights[x,z]*size.Y);
                    for(int y =0;y < height; y++)
                    {
                        terrainheight[x,y,z] = height;

                    }
                }
            }
        }
        public Mesh generatemesh(ref short[,,]blocktype,Vector3i pos)
        {
            int i = 0;
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    for (int z = 0; z < size.Z; z++)
                    {
                        if (blocktype[pos.X + x, y, pos.Z + z] != 0)
                        {
                            int cubetypeid = blocktype[pos.X + x, y, pos.Z + z];
                            if (x==size.X-1|| blocktype[pos.X + (x + 1), y, pos.Z + z] == 0)
                            {
                                AddQuad(x, y, z, 3, i, cubetypeid );
                                //right
                                i++;
                            }
                            if (x==0|| blocktype[pos.X + x - 1, y, pos.Z + z] == 0)
                            {
                                AddQuad(x, y, z, 2, i, cubetypeid);
                                //left
                                i++;
                            }
                            if (z == size.Z - 1|| blocktype[pos.X + x, y, pos.Z + z + 1] == 0)
                            {
                                AddQuad(x, y, z, 1, i, cubetypeid);
                                //back
                                i++;
                            }
                            if (z == 0|| blocktype[pos.X + x, y, pos.Z + z - 1] == 0)
                            {
                                //front
                                AddQuad(x, y, z, 0, i, cubetypeid);
                                i++;
                            }
                            if (y == 0)
                            {

                            }
                            else if (blocktype[x, y - 1, z] == 0)
                            {
                                //Addquad(x,y,z,4,i,cubetypeid);
                                //bottom
                                //i++;
                            }
                            if (y == size.Y - 1 || blocktype[x, y + 1, z] == 0)
                            {
                                AddQuad(x, y, z, 5, i, cubetypeid);
                                //top
                                i++;
                            }
                        }
                    }
                }
            }
            //Console.WriteLine("Cubeidcolor:"+cubeidcolor.Count);

            float[] vertices = new float[verts.Count * 3];
            for (int o = 0; o < verts.Count; o++)
            {
                vertices[(o * 3) + 0] = verts[o].X;
                vertices[(o * 3) + 1] = verts[o].Y;
                vertices[(o * 3) + 2] = verts[o].Z;

            }
            MeshObject meshObject = new MeshObject();
            Vertex[] vertexarray = new Vertex[verts.Count];
            Random random = new Random();
            for(int o=0;o< vertexarray.Length; o++)
            {
                vertexarray[o].Position = verts[o];
                Vector3i idpos = vertstowichcube[o];
                vertexarray[o].Color = GetRandomcolor(random) ;
                //vertexarray[o].Color = new Vector3(0.8f,0.8f,0.8f) ;
                //vertexarray[o].Color = new Vector3(Getnoise.getgraycolor(idpos) / 256f, Getnoise.getgraycolor(idpos) / 256f, Getnoise.getgraycolor(idpos) / 256f);
            }
            Console.WriteLine(vertices.Length+"verts "+verts.Count+"ind "+indicies.Count+"max "+max);
            meshObject.vertices = vertexarray;
            meshObject.indices = indicies.ToArray();
            meshObject.pos = new Vector3(0,0,0);
            Mesh mesh = new Mesh(meshObject);
            
            return mesh;
        }
        public static Vector3 GetRandomcolor(Random random)
        {
            return new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
        }
        uint max = 0;
        void AddQuad(int x, int y, int z, int face, int i, int cubeTypeId)
        {
            Vector3[] cubeVertices = {
            new Vector3(-0.5f + x, -0.5f + y, -0.5f + z),
            new Vector3(0.5f + x, -0.5f + y, -0.5f + z),
            new Vector3(0.5f + x, 0.5f + y, -0.5f + z),
            new Vector3(-0.5f + x, 0.5f + y, -0.5f + z),
            new Vector3(-0.5f + x, -0.5f + y, 0.5f + z),
            new Vector3(0.5f + x, -0.5f + y, 0.5f + z),
            new Vector3(0.5f + x, 0.5f + y, 0.5f + z),
            new Vector3(-0.5f + x, 0.5f + y, 0.5f + z)
            };

            Dictionary<int, int[]> faceTriangles = new Dictionary<int, int[]>
            {
            { 0, new int[] { 2, 1, 0, 3, 2, 0 } }, // front
            { 1, new int[] { 4, 5, 6, 4, 6, 7 } }, // back
            { 2, new int[] { 0, 4, 7, 0, 7, 3 } }, // left
            { 3, new int[] { 6, 5, 1, 2, 6, 1 } }, // right
            { 4, new int[] { 0, 1, 5, 0, 5, 4 } }, // bottom
            { 5, new int[] { 2, 3, 7, 2, 7, 6 } }  // top
            };

            int[] tri = faceTriangles[face];

            int baseIndex = verts.Count;
            Dictionary<int, int> uniqueVertices = new Dictionary<int, int>();

            for (int j = 0; j < tri.Length; j++)
            {
                int vertexIndex = tri[j];
                if (!uniqueVertices.ContainsKey(vertexIndex))
                {
                    uniqueVertices[vertexIndex] = baseIndex + uniqueVertices.Count;
                    verts.Add(cubeVertices[vertexIndex]);
                    vertstowichcube.Add(new Vector3i(x, y, z));
                }
            }

            for (int j = 0; j < tri.Length; j++)
            {
                if(max< (uint)uniqueVertices[tri[j]])
                {
                    max = (uint)uniqueVertices[tri[j]];
                }
                indicies.Add((uint)uniqueVertices[tri[j]]);
            }
        }


    }
}
