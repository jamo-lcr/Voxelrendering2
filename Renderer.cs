using OpenTK.Graphics.ES30;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using OpenTK.Input;

namespace Voxelrendering2
{
    public class Renderer:GameWindow
    {
        Matrix4 ModelMatrix = Matrix4.Identity;
        Matrix4 ViewMatrix = Matrix4.Identity;
        Matrix4 ProjectionMatrix = Matrix4.Identity;

        Gamerendering rendering;
        private readonly float[] _vertices =
        {

        };

        private readonly uint[] _indices =
        {

        };
        public static List<Mesh> meshes= new List<Mesh>();
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _elementBufferObject;

        private Shader _shader;
        public Renderer(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings): base(gameWindowSettings, nativeWindowSettings)
        {
            meshes.Add(new Mesh(_vertices, _indices));
            rendering = new Gamerendering();
        }
        public static Matrix4 CreateTransformationMatrix(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));

            Matrix4 transformationMatrix =  rotationMatrix* translationMatrix * scaleMatrix;
            return transformationMatrix;
        }
        // Now, we start initializing OpenGL.
        protected override void OnLoad()
        {
            base.OnLoad();

            rendering.Start();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, meshes[0].vertices.Length * sizeof(float), meshes[0].vertices , BufferUsageHint.StreamDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, meshes[0].indices.Length * sizeof(uint), meshes[0].indices, BufferUsageHint.StreamDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);


            Console.WriteLine(GetShaderPath("shader.vert"));
            //_shader = new Shader("C:\\Users\\PC\\source\\repos\\Voxelrendering2\\Shaders\\shader.vert", "C:\\Users\\PC\\source\\repos\\Voxelrendering2\\Shaders\\shader.frag");
            _shader = new Shader(GetShaderPath("shader.vert"), GetShaderPath("shader.frag"));
            //_shader.Use();

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //GL.BindVertexArray(vertexArray);
            for (int i=0;i<meshes.Count;i++)
            {
                ModelMatrix = CreateTransformationMatrix(meshes[i].pos, new Vector3(1, 1, 1), new Vector3(0, 0, 0));
                _shader.Use(ModelMatrix, ViewMatrix, ProjectionMatrix,false);
                GL.BindVertexArray(_vertexArrayObject);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, meshes[i].vertices.Length * sizeof(float), meshes[i].vertices, BufferUsageHint.DynamicDraw);
                GL.BufferData(BufferTarget.ElementArrayBuffer, meshes[i].indices.Length * sizeof(int), meshes[i].indices, BufferUsageHint.DynamicDraw);
                GL.DrawElements(PrimitiveType.Triangles, meshes[i].indices.Length, DrawElementsType.UnsignedInt, 0);
                //GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            }

            SwapBuffers();
        }
        static string GetShaderPath(string shaderFileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, "Shaders", shaderFileName);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {

            base.OnUpdateFrame(e);
            KeyboardState keyboardState = KeyboardState;
            MouseState mouseState = MouseState.GetSnapshot();
            Inputvar inputvar = new Inputvar();
            inputvar.mouseState = mouseState;
            inputvar.keyboardState = keyboardState;
            rendering.Update(inputvar,this);
            ViewMatrix = rendering.camera.ViewMatrix;
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)this.Size.X / this.Size.Y, 0.1f, 100000f);
            Console.WriteLine(this.ClientSize.X / this.ClientSize.Y);
            rendering.deltatime = e.Time;
            rendering.time += e.Time;

            if (inputvar.keyboardState.IsKeyDown(Keys.Escape))
            {
                Close(); 
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }
    }
}
