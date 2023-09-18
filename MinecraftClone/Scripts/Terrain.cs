using Godot;
using MinecraftClone.Scripts;
using System;
using System.Collections.Generic;

public partial class Terrain : Node3D
{

	[Export] NodePath playerNode;
	[Export] Noise noise;
	[Export] Material material;
	Vector3I playerPosition;
	int loadRadius = 12;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playerPosition = Vector3I.Zero;
		ChunkManager.material = material;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		playerPosition = (Vector3I)(GetNode<Node3D>(playerNode).Position / Chunk.chunkSize).Round();

		foreach(var nearbyChunk in getNearbyChunkPositions())
		{
			if (!ChunkManager.chunkExists(nearbyChunk))
			{
				Chunk newChunk = new(nearbyChunk);
				AddChild(newChunk);
				ChunkManager.addChunk(newChunk);
			}
		}

		ChunkManager.generateNextChunk();

	}

	//Return coordinates for all the chunks that should be generated
	public IEnumerable<Vector2I> getNearbyChunkPositions()
	{
		for(int i = -loadRadius; i <= loadRadius; i++)
		{
			for (int j = -loadRadius; j <= loadRadius; j++)
			{
				yield return new Vector2I(playerPosition.X + i, playerPosition.Z + j);
			}
		}
	}
	
}
