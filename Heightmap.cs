using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxelrendering2
{
    public class Heightmap
    {
        public Bitmap heightmap;
        public string Heightmapfolderlocation;
        public string name;


        public Heightmap(string name= "grayscale_image.png", string folder= "Heightmaps") 
        {
            Heightmapfolderlocation = folder;
            this.name = name;
            heightmap = new Bitmap(getHeightmaplocation("grayscale_image.png"));
        }

        
        public  float[,] getheightfromnoisemap(Vector3i size, Vector3i pos)
        {
            float[,] heights = new float[size.X, size.Z];

            for (int x = 0; x < size.X; x++)
            {
                for (int z = 0; z < size.Z; z++)
                {

                    float height = getgraycolor(new Vector3i(pos.X+x,pos.Y,pos.Z+z));

                    heights[x, z] = height / 256f;
                }
            }

            return heights;
        }
        public  float getgraycolor(Vector3i pos)
        {
            if (pos.X < 0 || pos.X >= heightmap.Width || pos.Z < 0 || pos.Z >= heightmap.Height)
            {
                return 0;
            }
            return (heightmap.GetPixel(pos.X, pos.Z).B);
        }
        public Vector2i getdimensions()
        {
            return new Vector2i(heightmap.Width, heightmap.Height);
        }
        public  string getHeightmaplocation(string name)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, Heightmapfolderlocation, name);
        }
    }
    public static class Heightmapcollection
    {
        public static List<Heightmap> heightmaps = new List<Heightmap>();
        public static Heightmap heightmap;
    }
}
