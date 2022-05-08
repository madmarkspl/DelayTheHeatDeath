#version 420 core

layout (location = 0) in vec4 vPos;
        
uniform mat4 uModel;

layout (binding = 0) uniform VPMatrices
{
    mat4 uView;
    mat4 uProjection;
};

void main()
{
    gl_Position = uProjection * uView * uModel * vPos;
}