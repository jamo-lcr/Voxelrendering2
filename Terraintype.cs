using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Voxelrendering2
{
    public abstract class Voxeldata
    {
        public abstract Vector3 DarkColor { get; }
        public abstract Vector3 LightColor { get; }
    }
    internal static class Terraintype
    {
        public static Noise.SimplexNoise noise = new Noise.SimplexNoise();
        public const ushort GrassType = 1;
        public const ushort StoneType = 2;
        public static Vector3 getColor(ushort value, Vector3 pos)
        {
            if (value == GrassType)
            {
                return Grass.Getcolor(pos);
            }
            if (value == StoneType)
            {
                return Stone.Getcolor(pos);
            }
            return Vector3.One;
        }

        private static class Grass
        {
            public static Vector3 offset = new Vector3(1243, 2134, 23);
            public static Vector3 DarkColor { get; } = new Vector3(0.075f, 0.43f, 0.082f);
            public static Vector3 LightColor { get; } = new Vector3(0.254f, 0.6f, 0.04f);

            public static Vector3 Getcolor(Vector3 pos)
            {
                Noise.SimplexNoise.Simplexnoisesettings settings = new Noise.SimplexNoise.Simplexnoisesettings();
                settings.xoffset = 0;
                settings.yoffset = 0;
                settings.scale = 0.50f;
                settings.maxyvalue = 1f;
                float value = noise.Noise(pos.X + offset.X, pos.Z + offset.Z, settings);
                return Vector3.Lerp(DarkColor, LightColor, value);
            }
        }
        private static class Stone
        {
            public static Vector3 offset = new Vector3(1243, 2134, 23);
            public static Vector3 DarkColor { get; } = new Vector3(0.3f, 0.3f, 0.3f);
            public static Vector3 LightColor { get; } = new Vector3(0.6f, 0.6f, 0.6f);

            public static Vector3 Getcolor(Vector3 pos)
            {
                Noise.SimplexNoise.Simplexnoisesettings settings = new Noise.SimplexNoise.Simplexnoisesettings();
                settings.xoffset = 0;
                settings.yoffset = 0;
                settings.scale = 1f;
                settings.maxyvalue = 0.4f;
                float value = noise.Noise(pos.X + offset.X, pos.Z + offset.Z, settings);
                Noise.SimplexNoise.Simplexnoisesettings settings2 = new Noise.SimplexNoise.Simplexnoisesettings();
                settings2.xoffset = 0;
                settings2.yoffset = 0;
                settings2.scale = 0.02f;
                settings2.maxyvalue = 0.6f;
                float value2 = noise.Noise(pos.X + offset.X, pos.Z + offset.Z, settings2);
                return Vector3.Lerp(DarkColor, LightColor, value+value2);
            }
        }
    }
}
