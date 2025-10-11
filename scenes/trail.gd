extends Line2D

@export var length = 50
var point = Vector2();

func _process(_delta: float) -> void:
	global_position = Vector2.ZERO 
	global_rotation = 0 
	point = (get_parent() as Node2D).global_position
	add_point(point) 
	while get_point_count() > length:
		remove_point(0)
