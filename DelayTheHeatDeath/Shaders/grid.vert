#version 420 core

layout (location = 0) in vec4 vPos;
        
uniform mat4 uModel;

layout (binding = 0) uniform VPMatrices
{
    mat4 uView;
    mat4 uProjection;
};

out vec4 worldPosition;

void main()
{
    // -100 so it gets rendered behind
    worldPosition = uModel * vec4(vPos.xy, -100, 1);

    gl_Position = uProjection * uView * worldPosition;
}