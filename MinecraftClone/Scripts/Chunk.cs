using Godot;
using MinecraftClone.Scripts;
using System;
using System.Collections.Generic;

public partial class Chunk : Node3D
{
    MeshInstance3D meshInstance;
    CollisionShape3D collisionShapeNode;
    public Vector2I position;
    //static FastNoise noise = FastNoise.FromEncodedNodeTree("CQA=");

    static FastNoise noise = FastNoise.FromEncodedNodeTree("EQACAAAAAAAgQBAAAAAAQBkAEwDD9Sg/DQAEAAAAAAAgQAkAAGZmJj8AAAAAPwEEAAAAAAAAAEBAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAM3MTD4AMzMzPwAAAAA/");

    static public int chunkSize = 16;
    static public int chunkHeight = 128;
    static private int visitLimit = 10;
    public bool generated = false;
    private double timeSinceVisit = 0;
    public Chunk(Vector2I position)
    {
        this.position = position;
    }
    public override void _Ready()
    {
        GlobalPosition = new Vector3(position.X * chunkSize, 0, position.Y * chunkSize);

        collisionShapeNode = new();
        var staticBody = new StaticBody3D();
        staticBody.AddChild(collisionShapeNode);
        AddChild(staticBody);

        meshInstance = new();
        meshInstance.MaterialOverride = ChunkManager.material;
        AddChild(meshInstance);
    }
    public override void _Process(double delta)
    {
        //Increase the time since last visit.
        //If 100 seconds have passed since last visit, delete the chunk
        timeSinceVisit += delta;
        if (timeSinceVisit > visitLimit && generated)
        {
            QueueFree();
            ChunkManager.removeChunk(this);
        }
    }
    public void visit()
    {
        //If this chunk is visited, reset the timer
        timeSinceVisit = 0;
    }

    Vector3I noiseOffset = new Vector3I(0, -chunkHeight/2, 0);
    public void Generate()
    {
        //Generate the chunk
        /*
        // Test mesh. A plane elevated according to noise.
        meshInstance.Mesh = new PlaneMesh() { Size = new Vector2(chunkSize, chunkSize) };
        Translate(new Vector3(0, ChunkManager.noise.GetNoise2Dv(position)* chunkHeight, 0));
        */

        float[] noiseData = new float[chunkSize*chunkSize*chunkHeight];
        float mm = 0.005f;
        //var mnmx = noise.GenUniformGrid3D(noiseData, position.X*chunkSize, -10, position.Y*chunkSize, chunkSize, chunkHeight, chunkSize, mm, 1);
        SurfaceTool st = new();
        int k = 0;
        
        st.Begin(Mesh.PrimitiveType.Triangles);

        bool isBlock(Vector3I blockPosition)
        {

            //if (x > chunkSize || y > chunkHeight || z > chunkSize || x < 0 || y < 0 || z < 0) return true;
            if (ChunkManager.isModificated(blockPosition))
                return ChunkManager.getModification(blockPosition).isABlock;
            Vector3 noiseVector = (blockPosition + noiseOffset);
            float n = noise.GenSingle3D(noiseVector.X * mm, noiseVector.Y * mm, noiseVector.Z * mm, 0);
            if (n < 0f)
            {
                return true;
            }

            return false;
        }

        String getBlockType(Vector3I blockPosition)
        {
            Vector3 noiseVector = (blockPosition + noiseOffset);
            float t = noise.GenSingle3D(noiseVector.X * mm, noiseVector.Y * mm,noiseVector.Z * mm , 1337);
            if (t < .7f)
                return "Grass";
            else
                return "Stone";
        }

        bool isBlockDir(Vector3I blockPosition, Directions dir)
        {
            return isBlock(blockPosition + directionVectors[dir]);
        }

        Vector3I getBlockPositionWorld(int x, int y, int z)
        {
            return new Vector3I(position.X* chunkSize + x, y, position.Y*chunkSize +z);
        }

        for (int y = 0; y < chunkHeight; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    var bpw = getBlockPositionWorld(x, y, z);

                    if (ChunkManager.isModificated(bpw))
                    {
                        var modification = ChunkManager.getModification(bpw);
                        if (modification.isABlock)
                        {
                            foreach (var dir in listOfDirections)
                            {
                                if (!isBlockDir(bpw, dir))
                                {
                                    k = AddFace(dir, st, k, new Vector3(x, y, z), modification.newBlock);
                                }
                            }
                            continue;
                        }
                    }
                    if (isBlock(bpw))
                    {
                        string blockType = getBlockType(bpw);
                        foreach (var dir in listOfDirections)
                        {
                            if (!isBlockDir(bpw, dir))
                            {
                                k = AddFace(dir, st, k, new Vector3(x, y, z), blockType);
                            }
                        }
                    }
                }
            }
        }

        mesh = st.Commit();
        collisionShape = mesh.CreateTrimeshShape();
        //GD.Print("generated chunk @ "+position.ToString());
    }
    Mesh mesh;
    ConcavePolygonShape3D collisionShape;
    public void afterGenerate()
    {
        
        meshInstance.Mesh = mesh;
        collisionShapeNode.Shape = collisionShape;

        generated = true;
    }



    public enum Directions : byte
    {
        Top,
        Bottom,
        Left,
        Right,
        Forward,
        Backward
    }
    Directions[] listOfDirections = new Directions[]
    {
        Directions.Top, Directions.Bottom, Directions.Left, Directions.Right, Directions.Forward, Directions.Backward
    };
    Dictionary<Directions, Vector3I> directionVectors = new Dictionary<Directions, Vector3I>() {
        {Directions.Top, new Vector3I(0, 1, 0)},
        {Directions.Bottom, new Vector3I(0, -1, 0)},
        {Directions.Left, new Vector3I(1, 0, 0)},
        {Directions.Right, new Vector3I(-1, 0, 0)},
        {Directions.Forward, new Vector3I(0, 0, 1)},
        {Directions.Backward, new Vector3I(0, 0, -1)},
    };

    int AddFace(Directions faceDirection, SurfaceTool st, int k, Vector3 cubePosition, String blockType)
    {
        int[,] adders;
        switch (faceDirection)
        {
            case Directions.Top:
                adders = new int[,] { { 1, 1, -1 }, { 1, 1, 1 }, { -1, 1, 1 }, { -1, 1, -1 } };
                break;
            case Directions.Bottom:
                adders = new int[,] { { -1, -1, -1 }, { -1, -1, 1 }, { 1, -1, 1 }, { 1, -1, -1 } };
                break;
            case Directions.Left:
                adders = new int[,] { { 1, -1, 1 }, { 1, 1, 1 }, { 1, 1, -1 }, { 1, -1, -1 } };
                break;
            case Directions.Right:
                adders = new int[,] { { -1, -1, -1 }, { -1, 1, -1 }, { -1, 1, 1 }, { -1, -1, 1 } };
                break;
            case Directions.Forward:
                adders = new int[,] { { -1, -1, 1 }, { -1, 1, 1 }, { 1, 1, 1 }, { 1, -1, 1 } };
                break;
            case Directions.Backward:
                adders = new int[,] { { 1, -1, -1 }, { 1, 1, -1 }, { -1, 1, -1 }, { -1, -1, -1 } };
                break;
            default:
                adders = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
                break;
        }
        Vector3 getAdder(int i)
        {
            return new Vector3(adders[i, 0], adders[i, 1], adders[i, 2]);
        }

        var UVs = TextureManager.getUVs(blockType, faceDirection);

        st.SetNormal(directionVectors[faceDirection]);
        
        for (int i = 0; i < 4; i++)
        {
            st.SetUV(UVs[i]);
            st.AddVertex(getAdder(i) * .5f + cubePosition);
        }

        st.AddIndex(k);
        st.AddIndex(k + 1);
        st.AddIndex(k + 2);
        st.AddIndex(k + 2);
        st.AddIndex(k + 3);
        st.AddIndex(k);

        return k + 4;
    }


}
