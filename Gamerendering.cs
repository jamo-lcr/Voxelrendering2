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
        public double deltatime;
        public double time;
        public double cameraspeed=250f;
        public double mouserotationsensitivity=0.1f;
        public  Camera camera;

        public void Start()
        {
            camera = new Camera(new Vector3(0.0f, 100.0f, 3.0f),0,0f);
            Terrainmeshgenerator terrainmeshgenerator = new Terrainmeshgenerator(250);
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
        
    }
}
