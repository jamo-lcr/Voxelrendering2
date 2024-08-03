using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
namespace Voxelrendering2
{
    internal class Shader
    {
            
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        // This is how you create a simple shader.
        // Shaders are written in GLSL, which is a language very similar to C in its semantics.
        // The GLSL source is compiled *at runtime*, so it can optimize itself for the graphics card it's currently being used on.
        // A commented example of GLSL can be found in shader.vert.
        public Shader(string vertPath, string fragPath)
        {
            // There are several different types of shaders, but the only two you need for basic rendering are the vertex and fragment shaders.
            // The vertex shader is responsible for moving around vertices, and uploading that data to the fragment shader.
            //   The vertex shader won't be too important here, but they'll be more important later.
            // The fragment shader is responsible for then converting the vertices to "fragments", which represent all the data OpenGL needs to draw a pixel.
            //   The fragment shader is what we'll be using the most here.

            // Load vertex shader and compile
            var shaderSource = File.ReadAllText(vertPath);

            // GL.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            // Now, bind the GLSL source code
            GL.ShaderSource(vertexShader, shaderSource);

            // And then compile
            CompileShader(vertexShader);

            // We do the same for the fragment shader.
            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
            // To do this, create a program...
            Handle = GL.CreateProgram();

            // Attach both shaders...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // And then link them together.
            LinkProgram(Handle);

            // When the shader program is linked, it no longer needs the individual shaders attached to it; the compiled code is copied into the shader program.
            // Detach them, and then delete them.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
            // Querying this from the shader is very slow, so we do it once on initialization and reuse those values
            // later.

            // First, we have to get the number of active uniforms in the shader.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            _uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }
        public void Use() => GL.UseProgram(Handle);
        // A wrapper function that enables the shader program.
        public void setvert(Matrix4 modelMatrix, Matrix4 lightSpaceMatrix,Matrix4 view)
        {

            int modelLocation = GL.GetUniformLocation(Handle, "model");
            GL.UniformMatrix4(modelLocation, false, ref modelMatrix);

            int viewLocation = GL.GetUniformLocation(Handle, "projection");
            GL.UniformMatrix4(viewLocation, false, ref lightSpaceMatrix);

            int viewloc = GL.GetUniformLocation(Handle, "view");
            GL.UniformMatrix4(viewloc, false, ref view);

        }
        public void setvert(Matrix4 modelMatrix, Matrix4 view,Matrix4 projection, Matrix4 lightSpaceMatrix)
        {

            int modelLocation = GL.GetUniformLocation(Handle, "model");
            GL.UniformMatrix4(modelLocation, false, ref modelMatrix);

            int viewLocation = GL.GetUniformLocation(Handle, "projection");
            GL.UniformMatrix4(viewLocation, false, ref lightSpaceMatrix);

            int viewloc = GL.GetUniformLocation(Handle, "view");
            GL.UniformMatrix4(viewloc, false, ref view);

        }
        public void setvert(Matrix4 modelMatrix, Matrix4 viewMatrix, Matrix4 projectionMatrix, bool staticModel)
        {

            int modelLocation = GL.GetUniformLocation(Handle, "model");
            GL.UniformMatrix4(modelLocation, false, ref modelMatrix);

            int viewLocation = GL.GetUniformLocation(Handle, "view");
            GL.UniformMatrix4(viewLocation, false, ref viewMatrix);

            int projectionLocation = GL.GetUniformLocation(Handle, "projection");
            GL.UniformMatrix4(projectionLocation, false, ref projectionMatrix);

            float staticmodelfloat = 0f;
            if (staticModel == true)
            {
                staticmodelfloat = 1f;
            }
            int staticmodelLocation = GL.GetUniformLocation(Handle, "useModelMatrix");
            GL.Uniform1(projectionLocation, staticmodelfloat);
        }
        public void setfrag(int shadowMap, Vector3 lightPos, Vector3 viewPos, Vector3 lightColor,int shadowsmothness)
        {
            Use();
            SetInt("shadowMap", shadowMap);
            SetInt("shadowSmoothness", shadowsmothness);
            SetVector3("lightPos", lightPos);
            SetVector3("viewPos", viewPos);
            SetVector3("lightColor", lightColor);
        }
        public void SetMatrix4(string name, Matrix4 matrix)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, false, ref matrix);
        }

        public void SetVector3(string name, Vector3 vector)
        {
            int location = GL.GetUniformLocation(Handle,name);
            GL.Uniform3(location, vector);
        }

        public void SetInt(string name, int value)
        {
            int location = GL.GetUniformLocation(Handle,name);
            GL.Uniform1(location, value);
        }

        public void Usedepthshader(Matrix4 lightSpaceMatrix, Matrix4 modelMatrix)
        {
            if (Handle == 0)
            {
                throw new InvalidOperationException("Shader program handle is not valid.");
            }

            int modelLocation = GL.GetUniformLocation(Handle, "model");
            GL.UniformMatrix4(modelLocation, false, ref modelMatrix);

            int viewLocation = GL.GetUniformLocation(Handle, "lightSpaceMatrix");
            GL.UniformMatrix4(viewLocation, false, ref lightSpaceMatrix);

        }

    }
}

