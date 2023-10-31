shader_type canvas_item;
render_mode unshaded;

uniform float fade_amount : hint_range(0.0, 1.00);

void fragment() {
	COLOR = texture(TEXTURE, UV);
	COLOR.a = fade_amount;
}