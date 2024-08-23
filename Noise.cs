using OpenTK.Mathematics;
using System;
using System;
using System.Drawing;

namespace Voxelrendering2
{
    internal static class Noise
    {
        public class PerlinNoise
        {
            int[] permutation;

            private readonly int[] p;

            public PerlinNoise(int seed, int permutationsize)
            {
                Random rand = new Random(seed);
                // Initialize permutation array
                permutation = setnewpermutation(permutationsize);
                p = new int[permutation.Length * 2];
                permutation.CopyTo(p, 0);

                // Shuffle p array
                for (int i = p.Length - 1; i > 0; i--)
                {
                    int j = rand.Next(i + 1);
                    int temp = p[i];
                    p[i] = p[j];
                    p[j] = temp;
                }

                // Duplicate the permutation array
                for (int i = 0; i < permutation.Length; i++)
                {
                    p[permutation.Length + i] = p[i];
                }
            }
            public int[] setnewpermutation(int permutationSize)
            {
                Random random = new Random(9);
                permutation = new int[permutationSize];
                // Validate input
                if (permutationSize <= 0)
                    throw new ArgumentException("Permutation size must be positive.", nameof(permutationSize));

                // Initialize permutation array with sequential values
                int[] newPermutation = new int[permutationSize];
                for (int i = 0; i < permutationSize; i++)
                {
                    newPermutation[i] = i;
                }

                // Shuffle the array
                for (int i = newPermutation.Length - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    int temp = newPermutation[i];
                    newPermutation[i] = newPermutation[j];
                    newPermutation[j] = temp;
                    Console.WriteLine();
                }
                return newPermutation;
            }

            public float GetNoise(float x, float y)
            {
                int X = (int)Math.Floor(x) & 255;
                int Y = (int)Math.Floor(y) & 255;

                x -= (float)Math.Floor(x);
                y -= (float)Math.Floor(y);

                float u = Fade(x);
                float v = Fade(y);

                int A = p[X] + Y;
                int B = p[X + 1] + Y;

                float result = Lerp(v, Lerp(u, Grad(p[A], x, y), Grad(p[B], x - 1, y)),
                                   Lerp(u, Grad(p[A + 1], x, y - 1), Grad(p[B + 1], x - 1, y - 1)));
                return (result + 1) / 2; // Normalize to 0-1 range
            }

            public float[,] GetPerlinNoiseArray(int width, int height, float scale)
            {
                float[,] noiseArray = new float[width, height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Convert array coordinates to noise coordinates
                        float nx = x / (float)width;
                        float ny = y / (float)height;
                        noiseArray[x, y] = GetNoise(nx * scale, ny * scale); // Scale coordinates as needed
                    }
                }

                return noiseArray;
            }


            private static float Fade(float t)
            {
                return t * t * t * (t * (t * 6 - 15) + 10);
            }

            private static float Lerp(float t, float a, float b)
            {
                return a + t * (b - a);
            }

            private static float Grad(int hash, float x, float y)
            {
                int h = hash & 15;
                float u = h < 8 ? x : y;
                float v = h < 4 ? y : (h == 12 || h == 14 ? x : 0);
                return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
            }
        }

        public class SimplexNoise
        {
            public class Simplexnoisesettings
            {
                public float scale;
                public float yoffset;
                public float xoffset;
                public float zoffset;
                public float maxyvalue;
            }
            private static readonly int[] perm = {
                151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,
                140,36,103,30,69,142,8,99,37,240,21,10,23,190,6,148,
                247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,
                57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,
                74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,
                60,211,133,230,220,105,92,41,55,46,245,40,244,102,143,54,
                65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,
                200,196,135,130,116,188,159,86,164,100,109,198,173,186,3,
                64,52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,
                212,207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,
                213,119,248,152,2,44,154,163,70,221,153,101,155,167,43,172,
                9,129,135,99,155,175,201,41,83,79,100,138,75,59,193,92,
                69,10,194,245,19,140,145,28,127,104,129,22,112,85,187,77,
                39,136,191,98,103,144,111,43,86,212,117,84,133,64,55,76,
                25,130,114,48,48,19,23,140,61,141,68,44,69,36,117,46,
                229,95,47,12,55,121,222,109,114,190,10,71,23,109,78,176,
                90,114,39,129,50,213,155,46,95,30,184,109,181,79,186,105,
                18,164,237,194,61,72,11,206,55,41,139,190,92,174,120,18,
                209,12,50,63,46,46,193,11,108,153,74,228,161,120,24,111,
                135,13,38,83,70,14,49,117,212,64,61,98,71,32,149,106,
                93,117,186,212,45,25,46,84,188,213,38,32,149,151,95,72,
                80,128,19,45,154,72,30,124,127,106,125,75,186,95,214,144,
                120,115,174,137,185,24,147,63,10,60,193,133,174,58,49,183,
                93,137,96,118,53,56,169,36,132,42,213,140,16,22,10,103,
                8,141,75,30,143,7,33,198,17,183,10,100,84,77,144,123,
                167,80,88,132,69,191,18,125,136,183,146,175,66,37,186,176,
                12,62,143,9,173,49,107,182,41,174,7,63,63,27,71,168,
                62,100,148,126,122,85,109,135,21,103,155,110,134,159,23,76,
                66,179,151,13,56,171,185,146,29,187,89,116,48,161,114,26,
                154,179,104,138,133,103,32,141,118,81,72,165,63,74,65,45,
                27,93,110,165,132,19,183,16,166,58,52,16,166,60,20,85,
                142,118,121,119,66,28,189,76,111,78,150,107,115,182,183,158,
                153,22,158,152,128,10,11,11,177,160,103,194,66,139,128,16,
                94,112,164,191,67,17,25,91,105,61,70,105,12,11,55,161,
                129,143,62,67,114,131,156,187,194,46,30,46,114,54,28,83,
                194,161,148,132,142,147,116,62,91,128,182,73,186,194,173,16,
                109,43,158,90,29,96,169,63,63,58,79,158,72,63,113,28,
                158,76,187,46,137,131,75,185,136,38,44,47,190,43,20,182,
                188,182,145,139,144,189,119,49,49,95,143,75,144,189,104,176,
                103,56,91,70,124,104,103,172,114,130
                };

            private static readonly int[] permMod12;

            static SimplexNoise()
            {
                permMod12 = new int[perm.Length];
                for (int i = 0; i < perm.Length; i++)
                {
                    permMod12[i] = perm[i % perm.Length] % 12;
                }
            }
            public Simplexnoisesettings setsettings(float scale, float xoffset, float yoffset,float maxyvalue)
            {
                Simplexnoisesettings settings = new Simplexnoisesettings();
                settings.scale = scale;
                settings.xoffset = xoffset;
                settings.yoffset = yoffset;
                settings.maxyvalue = maxyvalue;
                return settings;
            }

            public float Noise(float x, float y, Simplexnoisesettings simplexsettings)
            {
                y += simplexsettings.yoffset;
                x += simplexsettings.xoffset;
                x *= simplexsettings.scale;
                y *= simplexsettings.scale;
                // Constants for 2D Simplex noise
                float F2 = 0.5f * (MathF.Sqrt(3.0f) - 1.0f);
                float s = (x + y) * F2;
                int i = FastFloor(x + s);
                int j = FastFloor(y + s);
                float G2 = (3.0f - MathF.Sqrt(3.0f)) / 6.0f;

                // Coordinate in simplex grid
                float t = (i + j) * G2;
                float X0 = i - t;
                float Y0 = j - t;
                float x0 = x - X0;
                float y0 = y - Y0;

                // Simplex corner indices
                int i1, j1;
                if (x0 > y0)
                {
                    i1 = 1;
                    j1 = 0;
                }
                else
                {
                    i1 = 0;
                    j1 = 1;
                }

                // Offsets for simplex corners
                float x1 = x0 - i1 + G2;
                float y1 = y0 - j1 + G2;
                float x2 = x0 - 1.0f + 2.0f * G2;
                float y2 = y0 - 1.0f + 2.0f * G2;

                // Hash coordinates
                int ii = i & 255;
                int jj = j & 255;
                int gi0 = permMod12[(ii + perm[jj]) & 255];
                int gi1 = permMod12[(ii + i1 + perm[(jj + j1) & 255]) & 255];
                int gi2 = permMod12[(ii + 1 + perm[(jj + 1) & 255]) & 255];

                // Compute noise contributions from the three corners
                float t0 = 0.5f - x0 * x0 - y0 * y0;
                float n0 = t0 < 0 ? 0 : MathF.Pow(t0, 4.0f) * Dot(gi0, x0, y0);

                float t1 = 0.5f - x1 * x1 - y1 * y1;
                float n1 = t1 < 0 ? 0 : MathF.Pow(t1, 4.0f) * Dot(gi1, x1, y1);

                float t2 = 0.5f - x2 * x2 - y2 * y2;
                float n2 = t2 < 0 ? 0 : MathF.Pow(t2, 4.0f) * Dot(gi2, x2, y2);

                // Return the final noise value
                return ((((70.0f * (n0 + n1 + n2))) + 1f)/2)* simplexsettings.maxyvalue;
            }

            private static float Dot(int g, float x, float y)
            {
                switch (g & 3)
                {
                    case 0: return x + y;
                    case 1: return -x + y;
                    case 2: return x - y;
                    case 3: return -x - y;
                    default: return 0; // This should never happen
                }
            }

            private static int FastFloor(float x)
            {
                return (int)MathF.Floor(x);
            }

            public float[,] GetNoiseArray(int width, int height, Simplexnoisesettings settings)
            {
                float[,] noiseArray = new float[width, height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Convert array coordinates to noise coordinates
                        float nx = (x);
                        float ny = (y);
                        noiseArray[x, y] = Noise(nx, ny, settings); // Scale coordinates as needed
                    }
                }

                return noiseArray;
            }

        }

    }
}