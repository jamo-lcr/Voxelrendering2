﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Voxelrendering2
{
    public class Chunk
    {
        public ushort[,,] terraintype;

        public Vector3 pos;
        public Vector3i size;

        public Chunkgenerator chunkgenerator;
        public Mesh chunkmesh;


        public Chunk(Vector3i size, Vector3 pos)
        {
            chunkgenerator = new Chunkgenerator(this);
            this.size = size;
            this.pos = pos;
        }
        public void generatechunk()
        {
            chunkgenerator.generatechunk(1, size, pos.X, pos.Z, pos);
        }
        public ushort[,,] getterraintype()
        {
            return terraintype;
        }

        public class Chunkgenerator
        {
            public Chunk chunk;
            public List<Vector3i> vertstowichcube = new List<Vector3i>();
            public List<float[]> cubeidcolor = new List<float[]>();
            public List<Vector3> verts = new List<Vector3>();
            public List<uint> indicies = new List<uint>();

            public Chunkgenerator(Chunk chunk)
            {
                this.chunk = chunk;
            }

            public void generatechunk(int getheightmode, Vector3i chunksize, float offsetX, float offsetZ, Vector3 pos)
            {
                if (getheightmode == 0)
                {
                    generateterrainheightandtypefromnoise(chunksize, offsetX, offsetZ);
                }
                if (getheightmode == 1) 
                {
                    Vector2i dimensions = Heightmapcollection.heightmap.getdimensions();
                    if (offsetX < dimensions.X&&offsetZ<dimensions.Y&&offsetZ>0&&offsetX>0)
                    {
                        generateterrainheightandtypefromheightmap(Heightmapcollection.heightmap, chunksize, offsetX, offsetZ);
                    }
                    else
                    {
                        generateterrainheightandtypefromnoise(chunksize, offsetX, offsetZ);
                    }

                }
                chunk.chunkmesh = generatemesh(chunk.terraintype, new Vector3i(0, 0, 0), chunksize);
                chunk.chunkmesh.pos = pos;
            }
            public void generateterrainheightandtypefromnoise(Vector3i chunksize, float offsetX, float offsetZ)
            {
                ushort[,,] terraintype = new ushort[chunksize.X + 2, chunksize.Y, chunksize.Z + 2];
                Noise.SimplexNoise noise = new Noise.SimplexNoise();
                float[,] heights = noise.GetNoiseArray(chunksize.X + 2, chunksize.Z + 2, noise.setsettings(0.01f, offsetX - 1, offsetZ - 1,0.1f));
                float[,] slope = CalculateSlopemap(heights);

                ushort max = 0;
                for (int x = 0; x < chunksize.X + 2; x++)
                {
                    for (int z = 0; z < chunksize.Z + 2; z++)
                    {
                        ushort height = (ushort)((heights[x, z]) * chunksize.Y);
                        if(height>chunksize.Y)
                            height=(ushort)chunksize.Y;
                        for (int y = 0; y < height; y++)
                        {
                            terraintype[x, y, z] = determineterraintype(x, y, z, heights, slope, terraintype);
                        }
                    }
                }
                chunk.terraintype = terraintype;
            }
            public void generateterrainheightandtypefromheightmap(Heightmap heightmap, Vector3i chunksize, float offsetX, float offsetZ)
            {
                ushort[,,] terraintype = new ushort[chunksize.X + 2, chunksize.Y, chunksize.Z + 2];
                float[,] heights = heightmap.getheightfromnoisemap(new Vector3i(chunksize.X + 2, chunksize.X + 2, chunksize.Z + 2), new Vector3i((int)offsetX,0, (int)offsetZ));
                float[,] slope = CalculateSlopemap(heights);
                for (int x = 0; x < chunksize.X + 2; x++)
                {
                    for (int z = 0; z < chunksize.Z + 2; z++)
                    {
                        ushort height = (ushort)((heights[x, z] ) * chunksize.Y);
                        for (int y = 0; y < height; y++)
                        {
                            terraintype[x, y, z] = determineterraintype(x,y,z,heights,slope,terraintype);
                        }
                    }
                }
                chunk.terraintype = terraintype;
            }
            public ushort determineterraintype(int x,int y,int z,float[,] height, float[,] slope, ushort[,,] terraintype)
            {
                if (slope[x,z] <= 0.002)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            public float[,] CalculateSlopemap(float[,] heightmap)
            {
                int width = heightmap.GetLength(0);
                int height = heightmap.GetLength(1);
                float[,] slopeMap = new float[width, height];

                for (int x = 1; x < width - 1; x++)
                {
                    for (int z = 1; z < height - 1; z++)
                    {
                        float X1 = Math.Abs(heightmap[x + 1, z] - heightmap[x, z]) / 2.0f;
                        float X2 = Math.Abs(heightmap[x - 1, z] - heightmap[x, z]) / 2.0f;
                        float Z1 = Math.Abs(heightmap[x, z + 1] - heightmap[x, z]) / 2.0f;
                        float Z2 = Math.Abs(heightmap[x, z - 1] - heightmap[x, z]) / 2.0f;

                        float X1Z1 = Math.Abs(heightmap[x + 1, z + 1] - heightmap[x, z]) / (2.0f * (float)Math.Sqrt(2));
                        float X1Z2 = Math.Abs(heightmap[x + 1, z - 1] - heightmap[x, z]) / (2.0f * (float)Math.Sqrt(2));
                        float X2Z1 = Math.Abs(heightmap[x - 1, z + 1] - heightmap[x, z]) / (2.0f * (float)Math.Sqrt(2));
                        float X2Z2 = Math.Abs(heightmap[x - 1, z - 1] - heightmap[x, z]) / (2.0f * (float)Math.Sqrt(2));

                        float gradientX = (X1 + X2) / 2.0f;
                        float gradientZ = (Z1 + Z2) / 2.0f;

                        float gradientDiag = (X1Z1 + X1Z2 + X2Z1 + X2Z2) / 4.0f;
                        slopeMap[x, z] = (float)Math.Sqrt(gradientX * gradientX + gradientZ * gradientZ + gradientDiag * gradientDiag);
                    }
                }

                return slopeMap;
            }
            public Mesh generatemesh(ushort[,,] blocktype, Vector3i pos, Vector3i size)
            {
                int i = 0;
                for (int x = 1; x < size.X + 1; x++)
                {
                    for (int y = 0; y < size.Y; y++)
                    {
                        for (int z = 1; z < size.Z + 1; z++)
                        {
                            if (blocktype[pos.X + x, y, pos.Z + z] != 0)
                            {
                                int cubetypeid = blocktype[pos.X + x, y, pos.Z + z];
                                if (blocktype[pos.X + (x + 1), y, pos.Z + z] == 0)
                                {
                                    AddQuad(x, y, z, 3, i, cubetypeid);
                                    //right
                                    i++;
                                }
                                if (blocktype[pos.X + x - 1, y, pos.Z + z] == 0)
                                {
                                    AddQuad(x, y, z, 2, i, cubetypeid);
                                    //left
                                    i++;
                                }
                                if (blocktype[pos.X + x, y, pos.Z + z + 1] == 0)
                                {
                                    AddQuad(x, y, z, 1, i, cubetypeid);
                                    //back
                                    i++;
                                }
                                if (blocktype[pos.X + x, y, pos.Z + z - 1] == 0)
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

                float[] vertices = new float[verts.Count * 3];
                for (int o = 0; o < verts.Count; o++)
                {
                    vertices[(o * 3) + 0] = verts[o].X;
                    vertices[(o * 3) + 1] = verts[o].Y;
                    vertices[(o * 3) + 2] = verts[o].Z;

                }
                Vertex[] vertexarray = new Vertex[verts.Count];
                Random random = new Random();
                for (int o = 0; o < vertexarray.Length; o++)
                {
                    vertexarray[o].Position = verts[o];
                    Vector3i idpos = vertstowichcube[o];
                    vertexarray[o].Color = Terraintype.getColor(chunk.terraintype[idpos.X, idpos.Y, idpos.Z], this.chunk.pos + new Vector3(idpos.X, idpos.Y, idpos.Z));
                }
                Mesh mesh = new Mesh(vertexarray, indicies.ToArray());

                return mesh;
            }
            public static Vector3 GetRandomcolor(Random random)
            {
                return new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            }

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
                    { 0, new int[] { 2, 1, 0, 3, 2, 0 } },
                    { 1, new int[] { 4, 5, 6, 4, 6, 7 } },
                    { 2, new int[] { 0, 4, 7, 0, 7, 3 } },
                    { 3, new int[] { 6, 5, 1, 2, 6, 1 } },
                    { 4, new int[] { 0, 1, 5, 0, 5, 4 } },
                    { 5, new int[] { 2, 3, 7, 2, 7, 6 } }
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
                uint max = 0;
                for (int j = 0; j < tri.Length; j++)
                {
                    if (max < (uint)uniqueVertices[tri[j]])
                    {
                        max = (uint)uniqueVertices[tri[j]];
                    }
                    indicies.Add((uint)uniqueVertices[tri[j]]);
                }
            }
        }
    }
}
