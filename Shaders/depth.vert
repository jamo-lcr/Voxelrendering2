#version 300 es

layout (location = 0) in vec3 aPos;   // the position variable has attribute position 0
layout (location = 1) in vec3 aColor; // the color variable has attribute position 1


uniform mat4 lightSpaceMatrix;
//uniform mat4 lightProjection;
//uniform mat4 lightView;
uniform mat4 model;           // Model matrix
out float FragDepth;

void main()
{
    //mat4 lightSpaceMatrix = lightProjection * lightView * model;

    gl_Position = lightSpaceMatrix * model * vec4(aPos, 1.0);
    FragDepth = gl_Position.z ; 
}