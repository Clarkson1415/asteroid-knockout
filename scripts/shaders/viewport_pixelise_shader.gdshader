// pixelise shader that does not zoom with the camera.
shader_type canvas_item;
render_mode unshaded; // Important for CanvasGroup

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform int pixel_size = 4; // Adjust this value to control the size of the "pixels"

void fragment() 
{
    // Calculate the block size in UV space
    vec2 pixel_block_size = (float(pixel_size) * SCREEN_PIXEL_SIZE);
    
    // 2. Round the UV to the nearest "pixel block" coordinate
    // This snaps the UV to a grid based on the pixel_size
    vec2 snapped_uv = floor(SCREEN_UV / pixel_block_size) * pixel_block_size;
    
    // 3. Sample the screen texture at the snapped UV coordinate
    vec4 color = texture(screen_texture, snapped_uv);

    // 4. Preserve the original CanvasGroup behavior (blending)
    // The CanvasGroup documentation often suggests this step for custom shaders
    // that replace the default CanvasGroup behavior.
    if (color.a > 0.0001) {
        color.rgb /= color.a; // Pre-multiply alpha correction
    }

    // 5. Output the pixelated color
    COLOR = color;
}