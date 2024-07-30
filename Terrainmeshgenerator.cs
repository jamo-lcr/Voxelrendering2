using OpenTK.Mathematics;
namespace Voxelrendering2
{
    public class Terrainmeshgenerator
    {
        public short[,,] terrainheight;
        public static int maxheight;
        public Terrainmeshgenerator(int height) 
        {
            maxheight = height;
            terrainheight = new short[250, height, 250];
            Chunkgen chunkgen = new Chunkgen(250, height, 250);
            chunkgen.generateterrainheight(ref terrainheight);
            Renderer.meshes.Add(chunkgen.generatemesh(ref terrainheight,new Vector3i(0,0,0)));
        }
    }
}
