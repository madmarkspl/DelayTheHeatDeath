#version 410 core
//in vec2 TexCoords;
in vec4 ParticleColor;
in float Life;

out vec4 color;

//uniform sampler2D sprite;

void main()
{
    //color = (texture(sprite, TexCoords) * ParticleColor);
    //color = vec4(1);
//    color = vec4(ParticleColor.r + Life, ParticleColor.g + Life, ParticleColor.b + Life, Life);
    float blue = 20 * pow(Life - 0.5358, 3);
    color = vec4(ParticleColor.r + blue, ParticleColor.g + blue, blue, ParticleColor.a - 0.5 + Life);
} 