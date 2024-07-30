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
        public List<Vector3> verts;
        public List<uint> indicies;
        
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
            verts = new List<Vector3>();
            indicies = new List<uint>();
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
                                Addquad(x, y, z, 3, i, cubetypeid );
                                //right
                                i++;
                            }
                            if (x==0|| blocktype[pos.X + x - 1, y, pos.Z + z] == 0)
                            {
                                Addquad(x, y, z, 2, i, cubetypeid);
                                //left
                                i++;
                            }
                            if (z == size.Z - 1|| blocktype[pos.X + x, y, pos.Z + z + 1] == 0)
                            {
                                Addquad(x, y, z, 1, i, cubetypeid);
                                //back
                                i++;
                            }
                            if (z == 0|| blocktype[pos.X + x, y, pos.Z + z - 1] == 0)
                            {
                                //front
                                Addquad(x, y, z, 0, i, cubetypeid);
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
                                Addquad(x, y, z, 5, i, cubetypeid);
                                //top
                                i++;
                            }
                        }
                    }
                }
            }
            List<float[]> vertcolor = new List<float[]>();
            Random random = new Random();
            for(int o = 0; o < vertstowichcube.Count; o++)
            {
                Vector3i idpos = vertstowichcube[o];
                //Console.WriteLine(idpos);
                int type = blocktype[idpos.X, idpos.Y, idpos.Z]/Terrainmeshgenerator.maxheight;
                float gray = Getnoise.getgraycolor(idpos)/256f;
                //float gray = type;
                float[] color = new float[3];
                //gray = random.NextSingle();
                color[0] = gray;
                color[1] = gray;
                color[2] = gray;

                vertcolor.Add(color);
            }
            //Console.WriteLine("Cubeidcolor:"+cubeidcolor.Count);

            float[] vertices = new float[verts.Count * 6];
            for (int o=0;o<verts.Count; o++)
            {
                vertices[(o * 6) + 0] = verts[o].X;
                vertices[(o * 6) + 1] = verts[o].Y;
                vertices[(o * 6) + 2] = verts[o].Z;
                vertices[(o * 6) + 3] = vertcolor[o][0];
                vertices[(o * 6) + 4] = vertcolor[o][1];
                vertices[(o * 6) + 5] = vertcolor[o][2];
            }
            
            Mesh mesh = new Mesh(vertices,indicies.ToArray());
            return mesh;
        }



        void Addquad(int x, int y, int z, int face, int i, int cubetype_id)
        {

            Vector3[] cube_vertices = {
            new Vector3(-0.5f + x, -0.5f + y, -0.5f + z),
            new Vector3(0.5f + x, -0.5f + y, -0.5f + z),
            new Vector3(0.5f + x, 0.5f + y, -0.5f + z),
            new Vector3(-0.5f + x, 0.5f + y, -0.5f + z),
            new Vector3(-0.5f + x, -0.5f + y, 0.5f + z),
            new Vector3(0.5f + x, -0.5f + y, 0.5f + z),
            new Vector3(0.5f + x, 0.5f + y, 0.5f + z),
            new Vector3(-0.5f + x, 0.5f + y, 0.5f + z)
            };


            int[] tri = new int[6];
            if (face == 0)
            {
                tri = new int[] { 2, 1, 0, 3, 2, 0 }; // front
            }
            else if (face == 1)
            {
                tri = new int[] { 4, 5, 6, 4, 6, 7 }; // back

            }
            else if (face == 2)
            {
                tri = new int[] { 0, 4, 7, 0, 7, 3 }; // left
            }
            else if (face == 3)
            {
                tri = new int[] { 6, 5, 1, 2, 6, 1 }; // right
            }
            else if (face == 4)
            {
                tri = new int[] { 0, 1, 5, 0, 5, 4 }; // bottom
            }
            else if (face == 5)
            {
                tri = new int[] { 2, 3, 7, 2, 7, 6 }; // top
            }
            int baseIndex = verts.Count;

            List<Vector3> newverts = new List<Vector3>();
            for (int j = 0; j < 8; j++)
            {
                if (tri.Contains(j) == true&& newverts.Contains(cube_vertices[j])==false)
                {
                    newverts.Add(cube_vertices[j]);
                    verts.Add(cube_vertices[j]);
                    vertstowichcube.Add(new Vector3i(x, y, z));
                }
            }
            int[] tricopy = tri; // Assuming the size is 6
            Array.Copy(tri, tricopy, tri.Length); // Copy the original array to tricopy

            // Get the distinct elements and sort them
            int[] distinctSorted = tricopy.Distinct().OrderBy(x => x).ToArray();

            // Create a dictionary to map original values to their shortened values
            Dictionary<int, int> valueMapping = distinctSorted.Select((value, index) => new { value, index }).ToDictionary(pair => pair.value, pair => pair.index);

            // Modify tricopy based on the mapping
            for (int j = 0; j < tricopy.Length; j++)
            {
                tricopy[j] = valueMapping[tricopy[j]];
            }
            tri = tricopy;


            for (int j = 0; j < 6; j++)
            {
                indicies.Add((uint)(tri[j]+baseIndex));
            }
        }
    }
}
