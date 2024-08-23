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
        public Matrix4 ProjectionMatrix;

        public float near;
        public Camera(Vector3 position, float yaw, float pitch, float degrees, Vector2 Size, float near, float far)
        {
            setProjectionmatrix(degrees, Size, near, far);
            Position = position;
            Yaw = yaw;
            Pitch = pitch;
            UpdateVectors();
        }
        public void setProjectionmatrix(float degrees, Vector2 Size, float near, float far)
        {
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(degrees), (float)Size.X / Size.Y, near, far);
        }


        public void ProcessMouseMovement(float deltaX, float deltaY)
        {
            Yaw += deltaX;
            Pitch -= deltaY;
            if (Pitch > 89.9f)
                Pitch = 89.9f;
            if (Pitch < -89.9f)
                Pitch = -89.9f;

            UpdateVectors();
        }

        private void UpdateVectors()
        {
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
            Front = Vector3.Normalize(front);

            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
        public Matrix4 GetProjectionMatrix(Vector2i Size, float nearplane, float farplane)
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)Size.X / Size.Y, nearplane, farplane);
        }
    }
}
