extends Camera3D

@export var terrainNode : NodePath

var terrain
func _ready():
	terrain = get_node(terrainNode)

func _physics_process(_delta):
	if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
		var space = get_world_3d().direct_space_state
		var query = PhysicsRayQueryParameters3D.create(global_position, global_position - global_transform.basis.z * 100)
		var collision = space.intersect_ray(query)
		if collision:
			var pos = collision.position
			pos += global_transform.basis.z*0.1
			terrain.addModification(Vector3i(floor(pos.x),floor(pos.y),floor(pos.z)),true)
