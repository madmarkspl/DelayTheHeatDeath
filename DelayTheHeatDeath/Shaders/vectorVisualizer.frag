﻿#version 420 core

uniform vec3 uColor;

out vec4 FragColor;

void main()
{
    FragColor = vec4(uColor, 0.5f);
}