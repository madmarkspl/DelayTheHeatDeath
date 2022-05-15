#version 420 core

in vec4 worldPosition;

uniform vec3 uColor;

out vec4 FragColor;

void main()
{
    vec3 black = vec3(0);
    vec3 white = vec3(1);

    int squareSize = 50;
    int width = 2;

    vec2 coord = worldPosition.xy / squareSize;

    vec2 f = abs(fract(coord) - 0.5f);

    vec2 df = fwidth(coord);

    float fw = max(fwidth(coord.x), fwidth(coord.y));

    vec2 g = smoothstep(-width * df, width * df, f);

    vec3 col = mix(white, black, g.x * g.y);

    FragColor = vec4(col, 0.2f);
}

// this gave some basic grid, so keeping it "just in case"
// FragColor = vec4(multiplier * sin(worldPosition.x),  multiplier * sin(worldPosition.y), 1.0f, 0.2f);