using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Godot;

namespace MinecraftClone.Scripts;

static internal class ChunkManager
{
    static readonly Dictionary<Vector2I, Chunk> chunks = new();
    static readonly Queue<Chunk> generateQueue = new();
    static readonly Queue<Chunk> updateQueue = new();
    static readonly Dictionary<Vector3I, Modification> modifications = new();
    public static Material material;

    /// <summary>
    /// Does the chunk with the "position" provided exist? Return true and "visit" the chunk if it is. Return false if not. 
    /// </summary>
    /// <param name="position">The position in chunk space</param>
    /// <returns>
    ///     true if chunk exists, false if not
    /// </returns>
    public static bool chunkExists(Vector2I position)
    {
        if (chunks.ContainsKey(position))
        {
            chunks[position].visit();
            return true;
        }
        return false;
    }

    public static bool isModificated(Vector3I blockPosition)
    {
        return modifications.ContainsKey(blockPosition);
    }

    public static Modification getModification(Vector3I blockPosition)
    {
        return modifications[blockPosition];
    }

    public static void addModification(Vector3I blockPosition,  Modification modification)
    {
        modifications.Add(blockPosition, modification);
        regenerateChunk(blockPosition);
    }

    public static void changeModificiation(Vector3I blockPosition, Modification modification)
    {
        modifications[blockPosition] = modification;
        regenerateChunk(blockPosition);
    }

    static void regenerateChunk(Vector2I chunkPosition)
    {
        if(chunks.ContainsKey(chunkPosition))
            updateQueue.Enqueue(chunks[chunkPosition]);
    }

    static void regenerateChunk(Vector3I blockPosition)
    {
        int x = blockPosition.X;
        int y = blockPosition.Z;
        x = Mathf.FloorToInt(x * 1.0 / Chunk.chunkSize);
        y = Mathf.FloorToInt(y * 1.0 / Chunk.chunkSize);
        regenerateChunk(new Vector2I(x, y));
        if (blockPosition.X % Chunk.chunkSize == 0)
            regenerateChunk(new Vector2I(x - 1, y));
        if (blockPosition.X % Chunk.chunkSize == -1)
            regenerateChunk(new Vector2I(x +1 , y));
        if (blockPosition.Z % Chunk.chunkSize == 0)
            regenerateChunk(new Vector2I(x, y - 1));
        if (blockPosition.Z % Chunk.chunkSize == -1)
            regenerateChunk(new Vector2I(x, y +1));

    }

    public static void addChunk(Chunk chunk)
    {
        chunks.Add(chunk.position, chunk);
        generateQueue.Enqueue(chunk);
    }

    public static void removeChunk(Chunk chunk) {
        chunks.Remove(chunk.position);
    }

    private static Task[] generationTasks = new Task[6];
    private static Task[] updateTasks = new Task[3];
    public static void generateNextChunk()
    {
        for (int i = 0; i<generationTasks.Length;i++)
        {
            Task task = generationTasks[i];
            if (generateQueue.Count > 0 && (task == null || task.IsCompleted))
            {
                Chunk c = generateQueue.Dequeue();
                generationTasks[i] = Task.Run(() => {
                    c.firstGenerate();
                });
            }
        }
        for (int i = 0; i < updateTasks.Length; i++)
        {
            Task task = updateTasks[i];
            if ((task == null || task.IsCompleted) && updateQueue.Count > 0)
            {
                Chunk c = updateQueue.Dequeue();
                generationTasks[i] = Task.Run(() => {
                    c.Generate();
                });
            }
        }
    }

    public struct Modification
    {
        public bool isABlock;
        public String newBlock;
    }
}
