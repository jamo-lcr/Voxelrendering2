#version 300 es
precision mediump float;

in float FragDepth; // Received depth from vertex shader

out vec4 FragColor; // Output color

void main()
{
    FragColor = vec4(vec3(FragDepth), 1.0); // Depth as grayscale
}