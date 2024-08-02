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
            terrainheight = new short[350, height, 350];
            Chunkgen chunkgen = new Chunkgen(350, height, 350);
            chunkgen.generateterrainheight(ref terrainheight);
            Renderer.meshes.Add(chunkgen.generatemesh(ref terrainheight,new Vector3i(0,0,0)));
        }
    }
}
