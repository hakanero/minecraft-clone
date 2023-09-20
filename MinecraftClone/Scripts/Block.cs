using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace MinecraftClone.Scripts;

internal class Block
{
    public string Name;
    public int topTexture = -1;
    public int bottomTexture = -1;
    public int leftTexture = -1;
    public int rightTexture = -1;
    public int forwardTexture = -1;
    public int backwardTexture = -1;
    public int def;

    public Block(string name, int def = 0)
    {
        this.Name = name;
        this.def = def;
    }

    public int getTexturePosition(Chunk.Directions faceDirection)
    {
        return faceDirection switch
        {
            Chunk.Directions.Top => topTexture == -1 ? def : topTexture,
            Chunk.Directions.Bottom => bottomTexture == -1 ? def : bottomTexture,
            Chunk.Directions.Left => leftTexture == -1 ? def : leftTexture,
            Chunk.Directions.Right => rightTexture == -1 ? def : rightTexture,
            Chunk.Directions.Forward => forwardTexture == -1 ? def : forwardTexture,
            Chunk.Directions.Backward => backwardTexture == -1 ? def : backwardTexture,
            _ => def,
        };
    }
}
