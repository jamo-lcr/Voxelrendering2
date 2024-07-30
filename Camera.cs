using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxelrendering2
{
    internal class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Front { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }

        public float Yaw { get; set; }
        public float Pitch { get; set; }

        public Matrix4 ViewMatrix { get { return Matrix4.LookAt(Position, Position + Front, Up); } }

        public Camera(Vector3 position, float yaw, float pitch)
        {
            Position = position;
            Yaw = yaw;
            Pitch = pitch;
            UpdateVectors();
        }



        public void ProcessMouseMovement(float deltaX, float deltaY)
        {
            Yaw += deltaX;
            Pitch -= deltaY;
            Console.WriteLine("Yaw: "+Yaw+"Pitch: "+Pitch);
            // Constrain the pitch to avoid flipping
            if (Pitch > 89.0f)
                Pitch = 89.0f;
            if (Pitch < -89.0f)
                Pitch = -89.0f;

            UpdateVectors();
        }

        private void UpdateVectors()
        {
            // Calculate the new Front, Right, and Up vectors based on the updated Euler angles
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            Front = Vector3.Normalize(front);

            // Recalculate Right and Up vectors
            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}
