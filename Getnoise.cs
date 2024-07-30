using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;

namespace Voxelrendering2
{
    public static class Getnoise
    {
        //static Bitmap bmp = new Bitmap("C:\\Users\\PC\\Desktop\\Innogames\\Java\\Obj_to_Heightmap - Kopie - Kopie\\Output\\grayscale_image.png");
        static Bitmap bmp = new Bitmap(getHeightmap("grayscale_image.png"));
        public static float[,] getheightfromnoisemap(Vector3i size, Vector3i pos)
        {
            float[,] heights = new float[size.X, size.Z];

            Console.WriteLine($"heights dimensions: {heights.GetLength(0)} x {heights.GetLength(1)}");

            for (int x = 0; x < size.X; x++)
            {
                for (int z = 0; z < size.Z; z++)
                {
                    bool calculatet = false;
                    float height = 0;

                    if (pos.X + x < bmp.Width && pos.Z + z < bmp.Height)
                    {
                        height = (bmp.GetPixel(pos.X + x, pos.Z + z).R);
                        calculatet = true;
                    }

                    if (calculatet==false)
                    {
                        height =(128f);
                    }

                    heights[x, z] = height/256f;
                }
            }

            return heights;
        }
        public static float getgraycolor(Vector3i pos)
        {
            return (bmp.GetPixel(pos.X, pos.Z).B);
        }
        public static string getHeightmap(string name)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, "Heightmaps", name);
        }

    }
}
