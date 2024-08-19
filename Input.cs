using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Windows.Input;
using OpenTK.Input;
using OpenTK.Mathematics;
using System.Drawing;
using OpenTK.Windowing.Common;
using OpenTK.Input;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;

namespace Voxelrendering2
{
    public struct Inputvar
    {
        public MouseState mouseState;
        public KeyboardState keyboardState;
    }
    public struct Inputdatadelta
    {
        public Vector3d deltapos;
        public Vector2d deltarot;
    }
    internal static class Input
    {
        public static Vector2 prevpos;
        public static MouseState prevMouseState;
        public static unsafe Inputdatadelta checkkamerainputkeyboard(Inputvar inputvar, Camera camera, GameWindow window)
        {
            Inputdatadelta inputresdata = new Inputdatadelta();
            KeyboardState Inputkeyboard = inputvar.keyboardState;
            GLFW.SetCursorPos(window.WindowPtr, window.Size.X / 2, window.Size.Y / 2);
            MouseState mouseState = inputvar.mouseState;
            Vector3 dir = new Vector3();
            if (Inputkeyboard.IsKeyDown(Keys.W))
            {
                dir += camera.Front;
            }
            if (Inputkeyboard.IsKeyDown(Keys.S))
            {
                dir -= camera.Front;
            }
            if (Inputkeyboard.IsKeyDown(Keys.A))
            {
                dir -= Vector3.Cross(camera.Front, Vector3.UnitY).Normalized();
            }
            if (Inputkeyboard.IsKeyDown(Keys.D))
            {
                dir += Vector3.Cross(camera.Front, Vector3.UnitY).Normalized();
            }
            if (Inputkeyboard.IsKeyDown(Keys.Space))
            {
                dir += new Vector3(0, 1, 0);
            }
            if (Inputkeyboard.IsKeyDown(Keys.LeftControl))
            {
                dir -= new Vector3(0, 1, 0);
            }
            if (prevMouseState == null)
            {
                GLFW.SetCursorPos(window.WindowPtr, window.Size.X / 2, window.Size.Y / 2);
                prevMouseState = mouseState;
            }
            prevpos = new Vector2(window.Size.X / 2, window.Size.Y / 2);
            float deltaX = mouseState.X - prevpos.X;
            float deltaY = mouseState.Y - prevpos.Y;

            GLFW.SetCursorPos(window.WindowPtr, window.Size.X / 2, window.Size.Y / 2);

            inputresdata.deltapos = dir;
            inputresdata.deltarot.X = deltaX;
            inputresdata.deltarot.Y = deltaY;
            return inputresdata;
        }
    }
}
