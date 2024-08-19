#version 300 es
precision mediump float;

out vec4 FragColor;

in vec3 FragPos;            // Fragment position in world space
in vec3 Normal;             // Normal vector at the fragment
in vec3 color;              // Color of the fragment
in vec4 FragPosLightSpace;  // Fragment position in light space

uniform sampler2D shadowMap;   // Blurred shadow map texture
uniform int shadowSmoothness;  // Number of samples for PCF
uniform vec3 lightPos;         // Light position
uniform vec3 viewPos;          // Camera (view) position
uniform vec3 lightColor;       // Light color

// Variance Shadow Calculation with PCF
float ShadowCalculation(vec4 fragPosLightSpace)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;

    // Check if fragment is outside the light's frustum
    if (projCoords.z > 1.0)
        return 1.0;

    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    int samples = shadowSmoothness * 2 + 1;
    float sampleCount = float(samples * samples);

    for (int x = -shadowSmoothness; x <= shadowSmoothness; ++x)
    {
        for (int y = -shadowSmoothness; y <= shadowSmoothness; ++y)
        {
            vec2 offset = vec2(x, y) * texelSize;
            vec2 moments = texture(shadowMap, projCoords.xy + offset).rg;
            float currentDepth = projCoords.z;

            // Compute the mean and variance from the shadow map moments
            float mean = moments.x;
            float meanSquared = moments.y;
            float variance = meanSquared - (mean * mean);
            variance = max(variance, 0.0000005); // Avoid negative variance

            // Compute the Chebyshev's upper bound
            float d = currentDepth - mean;
            float p = variance / (variance + d * d);

            // Accumulate shadow factor
            shadow += (1.0 - p);
        }
    }
    
    // Average the results
    shadow /= sampleCount;

    return shadow;
}

void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);

    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * lightColor;

    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    float specularStrength = 0.5;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specularStrength * spec * lightColor;

    // Calculate shadow factor with PCF
    float shadow = ShadowCalculation(FragPosLightSpace);

    // Apply lighting and shadowing
    vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * color;
    FragColor = vec4(lighting, 1.0);
}