using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace MinecraftClone.Scripts;

static internal class ChunkManager
{
    static readonly Dictionary<Vector2I, Chunk> chunks = new();
    static readonly Queue<Chunk> generateQueue = new();
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
        //regenerateChunk(blockPosition);
    }

    static void regenerateChunk(Vector2I chunkPosition)
    {
        generateQueue.Enqueue(chunks[chunkPosition]);
    }

    static void regenerateChunk(Vector3I blockPosition)
    {
        int x = blockPosition.X;
        int y = blockPosition.Z;
        x /= Chunk.chunkSize; 
        y /= Chunk.chunkSize;
        regenerateChunk(new Vector2I(x, y));
    }

    public static void addChunk(Chunk chunk)
    {
        chunks.Add(chunk.position, chunk);
        generateQueue.Enqueue(chunk);
    }

    public static void removeChunk(Chunk chunk) {
        chunks.Remove(chunk.position);
    }

    private static Task<Action>[] generationTasks = new Task<Action>[12];
    public static void generateNextChunk()
    {
        for (int i = 0; i<generationTasks.Length;i++)
        {
            Task<Action> task = generationTasks[i];
            if(task != null && task.IsCompleted)
            {
                task.Result();
                generationTasks[i] = null;
            }
            if ((task==null || task.IsCompleted) && generateQueue.Count > 0)
            {
                Chunk c = generateQueue.Dequeue();
                generationTasks[i] = Task.Run(() => {
                    c.Generate();
                    return new Action(c.afterGenerate);
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
