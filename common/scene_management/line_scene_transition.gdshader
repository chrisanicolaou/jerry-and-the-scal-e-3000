shader_type canvas_item;

uniform float num_lines = 20.;
uniform float y_threshold: hint_range(0.0, 1.0) = 0.5;

void fragment() {
	vec2 tiled_uv = vec2(fract(UV.x * num_lines / 2.), UV.y);
	if (tiled_uv.x < 0.5){
		if(tiled_uv.y < y_threshold){
			COLOR = texture(TEXTURE, UV);
		} else {
			COLOR = vec4(0.0);
		}
	} else {
		if (tiled_uv.y > 1. - y_threshold){
			COLOR = texture(TEXTURE, UV);
		} else {
			COLOR = vec4(0.0);
		}
	}
}