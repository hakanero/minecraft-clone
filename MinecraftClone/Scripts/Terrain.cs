using Godot;
using MinecraftClone.Scripts;
using System;
using System.Collections.Generic;

public partial class Terrain : Node3D
{

	[Export] NodePath playerNode;
	[Export] Material material;
	Vector3I playerPosition;
	[Export] int loadRadius = 20;

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
		int x = 0; int y = 0;
		int dx = 0; int dy = -1;
		for(int i = 0; i < loadRadius * loadRadius; i++)
		{
			if(-loadRadius/2 < x && x < loadRadius/2 && -loadRadius / 2 < y && y < loadRadius / 2)
			{
				yield return new Vector2I(x + playerPosition.X, y + playerPosition.Z);
			}
			if(x==y || (x < 0 && x == -y) || (x > 0 && x == 1 - y))
			{
				int dc = dx;
				dx = -dy;
				dy = dc;
			}
			x += dx;
			y += dy;
		}
		//Source: https://stackoverflow.com/a/398302
	}

	public void addModification(Vector3I blockPosition, bool isBlock)
	{
		var mod = new ChunkManager.Modification() { isABlock = isBlock, newBlock = "Dirt" };

		if (ChunkManager.isModificated(blockPosition))
		{
			ChunkManager.changeModificiation(blockPosition, mod);
			return;
		}
		ChunkManager.addModification(blockPosition, mod);
	}
	
}
