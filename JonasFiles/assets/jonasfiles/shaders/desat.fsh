#version 330 core

in vec2 texCoord;
out vec4 fragColor;

uniform sampler2D primaryScene;
uniform sampler2D depthTex;
uniform float desaturationStrength;

vec3 applyDesaturation(vec3 col, float strength)
{
    if (strength <= 0.0) return col;
    
    float gray = dot(col, vec3(0.299, 0.587, 0.114));
    return mix(col, vec3(gray), strength);
}

void main()
{
    vec4 sceneColor = texture(primaryScene, texCoord);
    float depth = texture(depthTex, texCoord).r;
    
    float desatAmount = 0.0;
    if (depth < 1.0) {
        desatAmount = desaturationStrength;
    }
    
    vec3 finalColor = applyDesaturation(sceneColor.rgb, desatAmount);
    fragColor = vec4(finalColor, sceneColor.a);
}
