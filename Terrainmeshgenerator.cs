using OpenTK.Mathematics;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Voxelrendering2
{
    public class Terrainmeshgenerator
    {
        public static Vector3 playerpos;
        public Vector3i chunksize;
        public int Renderdistance;
        //allchunkseverseen
        public List<Chunk> allchunksloaded = new List<Chunk>();
        //shouldbevisible
        public List<Chunk> Visiblechunks = new List<Chunk>();
        //isvisible
        public List<Chunk> Renderedchunks = new List<Chunk>();

        public Terrainmeshgenerator(Vector3i chunksize, int Renderdistance)
        {
            this.chunksize = chunksize;
            this.Renderdistance = Renderdistance;
        }
        public void Updatechunkvisibility(Vector3 chunkpos, int Renderdistance, Vector3i chunksize)
        {
            Visiblechunks.Clear();
            for (int x = (int)chunkpos.X - ((Renderdistance * chunksize.X) / 2); x < (Renderdistance * chunksize.X / 2) + chunkpos.X; x += chunksize.X)
            {
                for (int z = (int)chunkpos.Z - ((Renderdistance * chunksize.Z) / 2); z < (Renderdistance * chunksize.Z / 2) + chunkpos.Z; z += chunksize.Z)
                {
                    if (chunkexist(Visiblechunks, new Vector3(x, 0, z)) == false)
                    {
                        Chunk chunk = GetChunkfrompos(allchunksloaded, new Vector3(x, 0, z));
                        if ((chunk == null))
                        {
                            chunk = new Chunk(chunksize, new Vector3(x, 0, z));
                        }
                        Visiblechunks.Add(chunk);
                    }
                }
            }


        }
        public void Applychunks()
        {
            Renderchunks(Visiblechunks, Renderedchunks, 10);
            cleanchunks(allchunksloaded, Visiblechunks);

        }

        public Chunk GetChunkfrompos(List<Chunk> chunks, Vector3 pos)
        {
            foreach (var chunk in chunks)
            {
                if (pos.X == chunk.pos.X && pos.Z == chunk.pos.Z)
                {
                    return chunk;
                }
            }
            return null;
        }
        public void Renderchunks(List<Chunk> Visible, List<Chunk> Rendered, int renderuntilnextupdate)
        {
            List<Chunk> torender = FindMissingElements(Visible, Rendered);
            for (int i = 0; i < renderuntilnextupdate; i++)
            {
                if (i < torender.Count)
                {
                    Chunk chunk = torender[i];
                    //creates and renders the chunk
                    chunk.generatechunk();
                    Renderer.activeScene.addMesh(chunk.chunkmesh);

                    Renderedchunks.Add(chunk);
                    allchunksloaded.Add(chunk);
                    //Performence
                }
            }
        }
        public void cleanchunks(List<Chunk> allchunksloaded, List<Chunk> Visiblechunks)
        {
            derenderchunks(allchunksloaded, Visiblechunks);
        }
        public void derenderchunks(List<Chunk> allchunksloaded, List<Chunk> Visiblechunks)
        {

            List<Chunk> chunkstoderender = FindMissingElements(allchunksloaded, Visiblechunks);
            for (int i = 0; i < chunkstoderender.Count; i++)
            {
                Chunk chunk = chunkstoderender[i];
                Renderer.activeScene.removeMesh(chunk.chunkmesh);
                Renderedchunks.Remove(chunk);
                //chunks später entladen
                deloadchunks(chunk);
                //Performence
                Renderer.activeScene.Updatebuffer = true;
            }
        }
        public void deloadchunks(Chunk chunk)
        {
            allchunksloaded.Remove(chunk);
            Visiblechunks.Remove(chunk);
        }

        public static List<T> FindMissingElements<T>(List<T> listA, List<T> listB)
        {
            HashSet<T> setB = new HashSet<T>(listB);
            // Filtere Elemente aus listA, die nicht in setB enthalten sind
            return listA.Where(element => !setB.Contains(element)).ToList();
        }

        public bool chunkexist(List<Chunk> chunks, Vector3 pos)
        {
            foreach (Chunk chunk in chunks)
            {
                if (chunk.pos.X == pos.X && chunk.pos.Z == pos.Z)
                {
                    return true;
                }
            }
            return false;
        }
        public Vector3 getchunkpos(Vector3 pos, Vector3i chunksize)
        {
            int x = (int)Math.Ceiling(pos.X / (double)chunksize.X) * chunksize.X;
            int z = (int)Math.Ceiling(pos.Z / (double)chunksize.Z) * chunksize.Z;
            return new Vector3(x, 0, z);
        }

    }
}
