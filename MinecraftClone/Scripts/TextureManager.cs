using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftClone.Scripts;

static internal class TextureManager
{
    public static int widthOfAtlas = 8;
    public static int heightOfAtlas = 8;
    public static Dictionary<String, Block> blocks = new Dictionary<string, Block>() {
        {"Air", new Block("Air") },
        {"Grass", new Block("Grass", 1){
            topTexture = 0,
            bottomTexture = 2 } 
        },
        {"Dirt", new Block("Dirt", 2) },
        {"Stone", new Block("Stone", 3)}

    };

    public static Vector2[] getUVs(String blockType, Chunk.Directions faceDirection)
    {
        int textPos = blocks[blockType].getTexturePosition(faceDirection);
        float x = textPos % widthOfAtlas;
        float y = Mathf.Floor(textPos / widthOfAtlas);
        return new Vector2[] { 
            new Vector2(x / widthOfAtlas, (y+1) / heightOfAtlas),
            new Vector2(x / widthOfAtlas, y / heightOfAtlas),
            new Vector2((x+1) / widthOfAtlas, y / heightOfAtlas),
            new Vector2((x+1) / widthOfAtlas, (y+1) / heightOfAtlas)
        };
    }
}
