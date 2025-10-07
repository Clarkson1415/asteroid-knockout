extends Node2D

@export var speed : float = 100
@export var joystick_left : VirtualJoystick

var facing_dir := Vector2.ZERO

func _process(delta: float) -> void:
	var x = cos(global_rotation)
	var y = sin(global_rotation)
	facing_dir = Vector2(x, y)
	position += facing_dir * speed * delta
	
	# for testing on PC
	var keysVector = Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	
	# Rotation:
	if joystick_left and joystick_left.is_pressed:
		global_rotation = joystick_left.output.angle()
	
	if keysVector != Vector2.ZERO:
		print("keys: %s", keysVector)
		global_rotation = keysVector.angle()
		print("angle: %s", global_rotation)
