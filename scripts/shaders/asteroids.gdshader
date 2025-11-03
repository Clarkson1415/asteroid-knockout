shader_type canvas_item;

uniform float density : hint_range(0.95, 1.0) = 0.985; // lower = more asteroids
uniform float scale : hint_range(0.0001, 0.005) = 0.0012; // spacing
uniform float drift_speed : hint_range(0.0, 200.0) = 40.0; // pixels/sec
uniform float time_scale : hint_range(0.0, 1.0) = 0.05; // wobble speed

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

void fragment() {
    // Create a slow drifting motion
    vec2 drift = vec2(TIME * drift_speed * 0.05, TIME * drift_speed * 0.03);
    vec2 uv = (FRAGCOORD.xy + drift) * scale;

    vec2 cell = floor(uv);
    float rnd = hash(cell);

    // Only some cells get asteroids
    if (rnd > density) {
        vec2 local = fract(uv) - 0.5;
        float shape = noise(cell) * 0.5 + 0.5;
        float radius = 0.25 + shape * 0.25;
        float d = length(local);
        float edge = smoothstep(radius, radius - 0.05, d);
        COLOR = mix(vec4(0.4, 0.4, 0.4, 1.0), vec4(0.0), edge);
    } else {
        COLOR = vec4(0.0);
    }
}
