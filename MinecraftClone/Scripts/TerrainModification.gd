extends Camera3D

@export var terrainNode : NodePath
@export var blockIndicatorNode : NodePath

var terrain
var blockInd
func _ready():
	terrain = get_node(terrainNode)
	blockInd = get_node(blockIndicatorNode)

var modificationCooldown : float = 0.0
var MOD_COOLDOWN : float = 0.1

func _physics_process(delta):
	modificationCooldown = clampf(modificationCooldown + delta, 0, MOD_COOLDOWN)
	var space = get_world_3d().direct_space_state
	var query = PhysicsRayQueryParameters3D.create(global_position, global_position - global_transform.basis.z * 100, get_parent().get_parent().collision_mask)
	var collision = space.intersect_ray(query)
	if collision:
		var pos = collision.position
		pos -= global_transform.basis.z*0.1
		blockInd.position = pos
		if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT) && modificationCooldown == MOD_COOLDOWN:
			modificationCooldown = 0
			pos += global_transform.basis.z*0.1
			pos = Vector3i(round(pos.x),round(pos.y),round(pos.z))
			terrain.addModification(pos ,true)
		if Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT) && modificationCooldown == MOD_COOLDOWN:
			modificationCooldown = 0
			pos = Vector3i(round(pos.x),round(pos.y),round(pos.z))
			terrain.addModification(pos, false)
