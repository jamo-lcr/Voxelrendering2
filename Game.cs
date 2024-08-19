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
    internal static class Game
    {
        public static double deltatime_ms;
        public static double time_ms;
        public static double cameraspeed = 250f;
        public static double mouserotationsensitivity = 0.1f;
        public static Camera camera;


        static Terrainmeshgenerator terrainmeshgenerator;
        public static void Start()
        {
            camera = new Camera(new Vector3(0.0f, 100.0f, 3.0f), 0, 0f, 45, new Vector2(2000, 2000), 0.1f, 1000f);
            terrainmeshgenerator = new Terrainmeshgenerator(new Vector3i(16, 250, 16), 17);
        }
        public static void Update(GameWindow window)
        {

            Camerainput(Getcurrentinputstate(window), window);
            terrainmeshgenerator.Updatechunkvisibility(terrainmeshgenerator.getchunkpos(camera.Position, terrainmeshgenerator.chunksize), terrainmeshgenerator.Renderdistance, terrainmeshgenerator.chunksize);
            terrainmeshgenerator.Applychunks();
            //TerrainRendering();
            Console.WriteLine(1 / deltatime_ms);
        }
        public static void TerrainRendering()
        {
            //todo use buffersubdata for no lags while chunks are rendered.
            {
                UpdateTerrain();
            }
            {
                terrainmeshgenerator.Applychunks();
            }
        }
        public static Inputvar Getcurrentinputstate(GameWindow window)
        {
            KeyboardState keyboardState = window.KeyboardState;
            MouseState mouseState = window.MouseState.GetSnapshot();
            Inputvar inputvar = new Inputvar();
            inputvar.mouseState = mouseState;
            inputvar.keyboardState = keyboardState;

            if (inputvar.keyboardState.IsKeyDown(Keys.Escape))
            {
                window.Close();
            }
            return inputvar;
        }
        public static void Camerainput(Inputvar input, GameWindow window)
        {
            Inputdatadelta data = Input.checkkamerainputkeyboard(input, camera, window);
            Vector3d deltapos = data.deltapos * deltatime_ms * cameraspeed;
            Vector2 deltarotation = (Vector2)(data.deltarot * mouserotationsensitivity);
            camera.Position += (Vector3)deltapos;
            camera.ProcessMouseMovement(deltarotation.X, deltarotation.Y);

        }

        public static void UpdateTerrain()
        {

            terrainmeshgenerator.Updatechunkvisibility(terrainmeshgenerator.getchunkpos(camera.Position, terrainmeshgenerator.chunksize), terrainmeshgenerator.Renderdistance, terrainmeshgenerator.chunksize);
        }
    }
}
