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
    static bool generateWorking = false;
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

    public static void addChunk(Chunk chunk)
    {
        chunks.Add(chunk.position, chunk);
        generateQueue.Enqueue(chunk);
    }

    public static void removeChunk(Chunk chunk) {
        chunks.Remove(chunk.position);
    }

    private static Task<Action>[] generationTasks = new Task<Action>[10];
    public static async void generateNextChunk()
    {
        for (int i = 0; i<generationTasks.Length;i++)
        {
            Task<Action> task = generationTasks[i];
            if ((task==null || task.IsCompleted) && generateQueue.Count > 0)
            {
                Chunk c = generateQueue.Dequeue();
                generationTasks[i] = Task.Run(() => {
                    c.Generate();
                    return new Action(c.afterGenerate);
                });
                (await task)();
            }
        }
        
    }
}
