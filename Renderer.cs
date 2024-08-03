using OpenTK.Graphics.ES30;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using OpenTK.Input;

namespace Voxelrendering2
{
    public class Renderer : GameWindow
    {
        Matrix4 ModelMatrix = Matrix4.Identity;
        Matrix4 ViewMatrix = Matrix4.Identity;
        Matrix4 ProjectionMatrix = Matrix4.Identity;

        Gamerendering rendering;

        public static List<Mesh> meshes = new List<Mesh>();
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _elementBufferObject;

        int _depthMapFBO;
        int _depthMap;
        Shader _depthShader;
        private Shader _shader;
        
        public static Vector3 lightpos = new Vector3(0,200,300);
        public Vector2i Shadowsize = new Vector2i(16000,16000);
        public Renderer(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            rendering = new Gamerendering();
        }
        public static Matrix4 CreateTransformationMatrix(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));

            Matrix4 transformationMatrix = rotationMatrix * translationMatrix * scaleMatrix;
            return transformationMatrix;
        }
        // Now, we start initializing OpenGL.
        protected override void OnLoad()
        {
            base.OnLoad();

            rendering.Start();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            // Set up vertex buffers and attributes
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, meshes[0].meshobjectdata.vertices.Length * 9 * sizeof(float), meshes[0].meshobjectdata.vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, meshes[0].meshobjectdata.indices.Length * sizeof(uint), meshes[0].meshobjectdata.indices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 6 * sizeof(float));

            // Set up the depth map framebuffer
            _depthMapFBO = GL.GenFramebuffer();
            _depthMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _depthMap);
            GL.TexImage2D(All.Texture2D, 0, All.DepthComponent32f, Shadowsize.X, Shadowsize.Y, 0, All.DepthComponent, All.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
            GL.FramebufferTexture2D(All.Framebuffer, All.DepthAttachment, All.Texture2D, _depthMap, 0);

            GL.DrawBuffers(0, new All[] { });
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Load shaders
            _shader = new Shader(GetShaderPath("shader.vert"), GetShaderPath("shader.frag"));
            _depthShader = new Shader(GetShaderPath("depth.vert"), GetShaderPath("depth.frag"));
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            lightpos.X += (float)Gamerendering.deltatime *10;
            base.OnRenderFrame(e);

            // Clear the screen
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            // Light space transformation
            //Matrix4 lightProjection = Matrix4.CreateOrthographicOffCenter(-Sh, 256, -256, 256, 0f, 1000f);
            Matrix4 lightProjection = Matrix4.CreateOrthographic(Shadowsize.X/32,Shadowsize.Y/32, 0f, 450f);
            Matrix4 lightView = Matrix4.LookAt(lightpos, new Vector3(125, 0, 125), Vector3.UnitY);
            //Matrix4 lightSpaceMatrix =  lightView* lightProjection;

            // First pass: Render scene to depth map
            _depthShader.Use();
            _depthShader.SetMatrix4("lightProjection", lightProjection);
            _depthShader.SetMatrix4("lightView", lightView);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
            GL.Viewport(0, 0, Shadowsize.X, Shadowsize.Y);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObject);
            for (int i = 0; i < meshes.Count; i++)
            {
                Matrix4 model = CreateTransformationMatrix(meshes[i].meshobjectdata.pos, new Vector3(1, 1, 1), new Vector3(0, 0, 0));
                _depthShader.SetMatrix4("model", model);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, meshes[i].meshobjectdata.vertices.Length * sizeof(float) * 9, meshes[i].meshobjectdata.vertices, BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, meshes[i].meshobjectdata.indices.Length * sizeof(int), meshes[i].meshobjectdata.indices, BufferUsageHint.DynamicDraw);

                GL.DrawElements(PrimitiveType.Triangles, meshes[i].meshobjectdata.indices.Length, DrawElementsType.UnsignedInt, 0);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            
            // Second pass: Render scene with shadow mapping
            GL.Viewport(0, 0, Size.X, Size.Y);
            _shader.Use();
            _shader.SetMatrix4("view", ViewMatrix);
            _shader.SetMatrix4("projection", ProjectionMatrix);
            _shader.SetMatrix4("lightProjection", lightProjection);
            _shader.SetMatrix4("lightView", lightView);
            //_shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            _shader.SetInt("shadowMap", _depthMap);
            _shader.SetInt("shadowSmoothness", 4);
            _shader.SetVector3("lightPos", lightpos);
            _shader.SetVector3("viewPos", rendering.camera.Position);
            _shader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 0.9f) * 1.0f);
            
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _depthMap);
            
            GL.BindVertexArray(_vertexArrayObject);
            for (int i = 0; i < meshes.Count; i++)
            {
                Matrix4 model = CreateTransformationMatrix(meshes[i].meshobjectdata.pos, new Vector3(1, 1, 1), new Vector3(0, 0, 0));
                _shader.SetMatrix4("model", model);
            
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, meshes[i].meshobjectdata.vertices.Length * sizeof(float) * 9, meshes[i].meshobjectdata.vertices, BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, meshes[i].meshobjectdata.indices.Length * sizeof(int), meshes[i].meshobjectdata.indices, BufferUsageHint.DynamicDraw);
            
                GL.DrawElements(PrimitiveType.Triangles, meshes[i].meshobjectdata.indices.Length, DrawElementsType.UnsignedInt, 0);
            }
            
            SwapBuffers();
        }
        static string GetShaderPath(string shaderFileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, "Shaders", shaderFileName);
        }
        Matrix4 CreateViewMatrix(Vector3 lightPosition, Vector3 lightTarget, Vector3 upDirection)
        {
            return Matrix4.LookAt(lightPosition, lightTarget, upDirection);
        }
        Matrix4 CreateOrthographicMatrix(float left, float right, float bottom, float top, float near, float far)
        {
            return Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, near, far);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {

            base.OnUpdateFrame(e);
            KeyboardState keyboardState = KeyboardState;
            MouseState mouseState = MouseState.GetSnapshot();
            Inputvar inputvar = new Inputvar();
            inputvar.mouseState = mouseState;
            inputvar.keyboardState = keyboardState;
            rendering.Update(inputvar, this);
            ViewMatrix = rendering.camera.ViewMatrix;
            //ViewMatrix=Matrix4.LookAt(lightpos, Vector3.Zero, Vector3.UnitY);
            //ProjectionMatrix = Matrix4.CreateOrthographic(Shadowsize.X, Shadowsize.Y, 0.1f, 3000f);
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)this.Size.X / this.Size.Y, 0.1f, 1000f);
            //Console.WriteLine(this.ClientSize.X / this.ClientSize.Y);
            Gamerendering.deltatime = e.Time;
            rendering.time += e.Time;
            meshes[1].meshobjectdata.pos = lightpos;
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