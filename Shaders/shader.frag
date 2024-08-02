#version 300 es
precision mediump float;

out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;
in vec3 color;
in vec4 FragPosLightSpace;

uniform sampler2D shadowMap;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform vec3 lightColor;

float ShadowCalculation(vec4 fragPosLightSpace)
{
    // Perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // Transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    // Get closest depth value from light's perspective (using [0,1] range fragPosLightSpace as coords)
    float closestDepth = texture(shadowMap, projCoords.xy).r;
    // Get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    // Check whether current frag pos is in shadow
    float shadow = currentDepth > closestDepth + 0.005 ? 1.0 : 0.0; // Bias to prevent shadow acne
    return shadow;
}

void main()
{
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);

    // Ambient
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * lightColor;

    // Diffuse
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    // Specular
    float specularStrength = 0.5;
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specularStrength * spec * lightColor;

    // Shadow
    float shadow = ShadowCalculation(FragPosLightSpace);

    vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * color;
    FragColor = vec4(lighting, 1.0);

}