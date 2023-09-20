extends DirectionalLight3D


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


var vis : bool = true


# Called every frame. 'delta' is the elapsed time since the previous frame.
var speed : float = 1
func _process(delta):
	rotation_degrees.x += delta * speed
