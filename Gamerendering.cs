using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using System.Diagnostics;
using OpenTK.Windowing.Desktop;

namespace Voxelrendering2
{
    internal class Gamerendering
    {
        public static double deltatime;
        public double time;
        public double cameraspeed=250f;
        public double mouserotationsensitivity=0.1f;
        public  Camera camera;

        public void Start()
        {
            camera = new Camera(new Vector3(0.0f, 100.0f, 3.0f),0,0f);
            Terrainmeshgenerator terrainmeshgenerator = new Terrainmeshgenerator(250);
           
            Renderer.meshes.Add(addcube(Renderer.lightpos,10));

        }
        public void Update(Inputvar inputvar,GameWindow window)
        {
            Camerainput(inputvar,window); 

        }
        public void Camerainput(Inputvar input, GameWindow window)
        {
            Inputresdata data = Input.checkkamerainputkeyboard(input,camera,window);
            Vector3d deltapos = data.coord*deltatime* cameraspeed;
            Vector2 deltarotation =(Vector2)(data.deltarot*mouserotationsensitivity);
            Console.WriteLine(1/deltatime);
            camera.Position += (Vector3)deltapos;
            camera.ProcessMouseMovement(deltarotation.X,deltarotation.Y);
           
        }
        public static Mesh addcube(Vector3 pos,float scale)
        {
            Vector3[] cubeVertices = {
            new Vector3(-0.5f + pos.X, -0.5f + pos.Y, -0.5f + pos.Z),
            new Vector3(0.5f + pos.X, -0.5f + pos.Y, -0.5f + pos.Z),
            new Vector3(0.5f + pos.X, 0.5f + pos.Y, -0.5f + pos.Z),
            new Vector3(-0.5f + pos.X, 0.5f + pos.Y, -0.5f + pos.Z),
            new Vector3(-0.5f + pos.X, -0.5f + pos.Y, 0.5f + pos.Z),
            new Vector3(0.5f + pos.X, -0.5f + pos.Y, 0.5f + pos.Z),
            new Vector3(0.5f + pos.X, 0.5f + pos.Y, 0.5f + pos.Z),
            new Vector3(-0.5f + pos.X, 0.5f + pos.Y, 0.5f + pos.Z)
            };
            uint[] indicies = {
             2, 1, 0, 3, 2, 0 , // front
             4, 5, 6, 4, 6, 7 , // back
             0, 4, 7, 0, 7, 3 , // left
             6, 5, 1, 2, 6, 1 , // right
             0, 1, 5, 0, 5, 4 , // bottom
             2, 3, 7, 2, 7, 6   // top
            };
            MeshObject meshObject = new MeshObject();
            Vertex[] vertexarray = new Vertex[cubeVertices.Length];
            Random random = new Random();
            for (int o = 0; o < vertexarray.Length; o++)
            {
                vertexarray[o].Position = cubeVertices[o]*scale;
                vertexarray[o].Color = new Vector3(1,1,1);
            }
            meshObject.vertices = vertexarray;
            meshObject.indices = indicies;
            meshObject.pos = new Vector3(0, 0, 0);
            Mesh mesh = new Mesh(meshObject);
            return mesh;
        }
        
    }
}
