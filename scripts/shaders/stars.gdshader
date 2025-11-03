shader_type canvas_item;

uniform float speed = 0;

uniform float density = 0.01;

void fragment() {
    vec2 uv = FRAGCOORD.xy / SCREEN_PIXEL_SIZE / 800.0;
    uv.y += TIME * speed;
    float n = fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453);
    if (n > 1.0 - density) {
        COLOR = vec4(vec3(1.0), 1.0);
    } else {
        COLOR = vec4(0.0);
    }
}
