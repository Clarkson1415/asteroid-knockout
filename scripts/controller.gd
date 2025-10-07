extends Node2D

@export var max_speed: float = 100
@export var acceleration: float = 300  # in units per second^2
@export var friction: float = 200      # how fast you slow down
@export var joystick_left: VirtualJoystick

var velocity: Vector2 = Vector2.ZERO

func _process(delta: float) -> void:
	var input_vector = Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")

	if joystick_left and joystick_left.is_pressed:
		input_vector = joystick_left.output

	# If there's input, accelerate in that direction
	if input_vector != Vector2.ZERO:
		# Rotate towards input direction
		global_rotation = input_vector.angle()

		# Calculate target velocity
		var desired_velocity = input_vector.normalized() * max_speed
		velocity = velocity.move_toward(desired_velocity, acceleration * delta)
	else:
		# No input: apply friction (decelerate to zero)
		velocity = velocity.move_toward(Vector2.ZERO, friction * delta)

	# Move the character
	position += velocity * delta
