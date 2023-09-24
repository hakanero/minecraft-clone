extends Camera3D

@export var terrainNode : NodePath
@export var blockIndicatorNode : NodePath

var terrain
var blockInd
func _ready():
	terrain = get_node(terrainNode)
	blockInd = get_node(blockIndicatorNode)

func _physics_process(_delta):
	var space = get_world_3d().direct_space_state
	var query = PhysicsRayQueryParameters3D.create(global_position, global_position - global_transform.basis.z * 100)
	query.exclude = [self]
	var collision = space.intersect_ray(query)
	if collision:
		var pos = collision.position
		pos -= global_transform.basis.z*0.1
		pos = Vector3i(round(pos.x),round(pos.y),round(pos.z))
		blockInd.position = pos
		if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
			terrain.addModification(pos ,true)
		if Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT):
			terrain.addModification(pos, false)
