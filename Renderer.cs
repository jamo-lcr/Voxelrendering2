using OpenTK.Graphics.ES30;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Diagnostics;
using OpenTK.Input;
using System.Collections.Generic;
using System.Reflection;
using OpentkVoxelRendererAufräumen;

namespace Voxelrendering2
{
    internal class Renderer : GameWindow
    {
        Shader _depthShader;
        Shader _mainShader;

        public static int _vertexArrayObject;

        int _depthMapFBO;
        int _depthMap;

        public static Scene activeScene;
        public static MainBuffermanager mainBuffermanager;

        public int depthmapsize = 16000;
        public Renderer(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            activeScene = new Scene();
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            Game.Start();
            Generateimportantobjects();
            SetOpenGLSettings();
            InitializeVertexArrayObject();
            SetupDepthMapFramebuffer(new Vector2i(depthmapsize, depthmapsize));
            InitializeShaders();
        }
        public void Generateimportantobjects()
        {
            mainBuffermanager = new MainBuffermanager();
            mainBuffermanager.Genmainbuffers();
        }
        #region OnLoad Methodes
        private void SetOpenGLSettings()
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }
        #region initrenderstuff

        private void InitializeVertexArrayObject()
        {
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 6 * sizeof(float));
        }
        #endregion
        #region shadows
        private void SetupDepthMapFramebuffer(Vector2i Shadowsize)
        {
            _depthMapFBO = GL.GenFramebuffer();
            _depthMap = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, _depthMap);
            GL.TexImage2D(All.Texture2D, 0, All.DepthComponent, Shadowsize.X, Shadowsize.Y, 0, All.DepthComponent, All.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
            GL.FramebufferTexture2D(All.Framebuffer, All.DepthAttachment, All.Texture2D, _depthMap, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        #endregion
        #region shaders
        private void InitializeShaders()
        {
            _mainShader = new Shader(GetShaderPath("shader.vert"), GetShaderPath("shader.frag"));
            _depthShader = new Shader(GetShaderPath("depth.vert"), GetShaderPath("depth.frag"));
        }
        static string GetShaderPath(string shaderFileName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, "Shaders", shaderFileName);
        }
        #endregion
        #endregion


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            Vector3 lightpos = new Vector3(0, 200, 60);
            Vector2i Shadowsize = new Vector2i(depthmapsize, depthmapsize);
            Render(Renderdepthmap(Shadowsize, lightpos), Game.camera.ViewMatrix, Game.camera.ProjectionMatrix);

            SwapBuffers();
        }


        public Matrix4 Renderdepthmap(Vector2i Shadowsize, Vector3 lightpos)
        {
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            Matrix4 lightProjection = Matrix4.CreateOrthographic(Shadowsize.X/6000* Game.camera.Position.Y, Shadowsize.Y/6000* Game.camera.Position.Y, 0.1f, 1000f+Game.camera.Position.Y);
            Matrix4 lightView = Matrix4.LookAt(lightpos+new Vector3(Game.camera.Position.X, Game.camera.Position.Y, Game.camera.Position.Z), new Vector3(Game.camera.Position.X, Game.camera.Position.Y, Game.camera.Position.Z), Vector3.UnitY);
            Matrix4 lightSpaceMatrix = lightView * lightProjection;

            _depthShader.Use();
            _depthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            _depthShader.SetMatrix4("lightView", lightView);

            GL.BindVertexArray(_vertexArrayObject);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
            GL.Viewport(0, 0, Shadowsize.X, Shadowsize.Y);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            Matrix4 model = CreateTransformationMatrix(Vector3.Zero, new Vector3(1, 1, 1), new Vector3(0, 0, 0));
            _depthShader.SetMatrix4("model", model);
            GL.DrawElements(PrimitiveType.Triangles, mainBuffermanager._elementcount, DrawElementsType.UnsignedInt, 0);
            return lightSpaceMatrix;
        }
        public static Matrix4 CreateTransformationMatrix(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));

            return rotationMatrix * translationMatrix * scaleMatrix;
        }
        public void Render(Matrix4 lightSpaceMatrix, Matrix4 ViewMatrix, Matrix4 ProjectionMatrix)
        {

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, Size.X, Size.Y);
            Matrix4 model = CreateTransformationMatrix(Vector3.Zero, new Vector3(1, 1, 1), new Vector3(0, 0, 0));
            _mainShader.Use();
            _mainShader.SetMatrix4("view", ViewMatrix);
            _mainShader.SetMatrix4("projection", ProjectionMatrix);
            _mainShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
            _mainShader.SetInt("shadowMap", _depthMap);
            _mainShader.SetInt("shadowSmoothness", 2);
            _mainShader.SetVector3("lightPos", Scene.lightpos);
            _mainShader.SetVector3("viewPos", Game.camera.Position);
            _mainShader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 0.9f) * 1.0f);
            _mainShader.SetMatrix4("model", model);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _depthMap);

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, mainBuffermanager._elementcount, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            Game.time_ms += e.Time;
            Game.deltatime_ms = e.Time;
            Game.Update(this);
        }
    }

}