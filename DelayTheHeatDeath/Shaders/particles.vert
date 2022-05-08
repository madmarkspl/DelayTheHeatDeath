#version 420 core
layout (location = 0) in vec2 position; // <vec2 position, vec2 texCoords>
layout (location = 1) in vec4 color; // <vec2 position, vec2 texCoords>
layout (location = 2) in float life; // <vec2 position, vec2 texCoords>

//out vec2 TexCoords;
out vec4 ParticleColor;
out float Life;

//uniform vec2 uOffset;
//uniform vec4 uColor;

layout (binding = 0) uniform VPMatrices
{
    mat4 uView;
    mat4 uProjection;
};

void main()
{
//    float scale = 10.0f;
    //TexCoords = vertex.zw;
    ParticleColor = color;
    Life = life;

    gl_Position = uProjection * uView * vec4(position, 0.0, 1.0);
}