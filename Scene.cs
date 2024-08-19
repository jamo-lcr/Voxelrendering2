using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Voxelrendering2
{
    internal class Scene
    {
        public static Camera camera;
        public static Vector3 lightpos;

        public bool Updatebuffer = true;
        public List<Mesh> Meshes = new List<Mesh>();

        public void addMesh(Mesh mesh)
        {
            Meshes.Add(mesh);
            Renderer.mainBuffermanager.addMesh(mesh);
        }

        public void removeMesh(Mesh mesh)
        {
            int meshindex = Meshes.IndexOf(mesh);
            Meshes.RemoveAt(meshindex);
            Renderer.mainBuffermanager.removemesh(mesh);
        }

    }
}
