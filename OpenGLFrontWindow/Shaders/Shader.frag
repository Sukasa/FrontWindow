#version 330

smooth in vec4 color;
smooth in vec2 coords;

uniform sampler2D renderTexture;

out vec4 fragColor;

void main()
{
	fragColor = texture2D(renderTexture, coords) * color;
}