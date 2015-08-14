#version 330

in vec2 in_position;
in vec4 in_color;
in vec2 in_texture;

uniform mat4 projection;

smooth out vec4 color;
smooth out vec2 coords;

void main() {
	color = in_color;
	coords = in_texture;
	gl_Position = projection * vec4(in_position.x, in_position.y, 1.0, 1.0);
}