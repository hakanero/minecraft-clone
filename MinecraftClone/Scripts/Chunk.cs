using Godot;
using MinecraftClone.Scripts;
using System;
using System.Collections.Generic;

public partial class Chunk : Node3D
{
    MeshInstance3D meshInstance;
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

    int noiseOffsetX = 0, noiseOffsetY = -chunkHeight/2, noiseOffsetZ = 0;
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

        bool isBlock(int x, int y, int z)
        {

            //if (x > chunkSize || y > chunkHeight || z > chunkSize || x < 0 || y < 0 || z < 0) return true;

            float n = noise.GenSingle3D((position.X * chunkSize + x + noiseOffsetX) * mm, (y+noiseOffsetY) * mm, (position.Y * chunkSize + z + noiseOffsetZ) * mm, 0);
            if (n < 0f)
            {
                return true;
            }

            return false;
        }

        bool isBlockDir(int x, int y, int z, Directions dir)
        {
            Vector3I adder = directionVectors[dir];
            return isBlock(x + adder.X, y + adder.Y, z + adder.Z);
        }

        for (int y = 0; y < chunkHeight; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    if(isBlock(x,y,z))
                    {
                        foreach (var dir in listOfDirections)
                        {
                            if (!isBlockDir(x, y, z, dir))
                            {
                                k = AddFace(dir, st, k, new Vector3(x, y, z));
                            }
                        }
                    }
                }
            }
        }

        mesh = st.Commit();

        generated = true;
        
        //GD.Print("generated chunk @ "+position.ToString());
    }
    Mesh mesh;
    public void afterGenerate()
    {
        meshInstance = new();
        meshInstance.MaterialOverride = ChunkManager.material;
        meshInstance.Mesh = mesh;
        AddChild(meshInstance);
    }



    enum Directions : byte
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

    int AddFace(Directions faceDirection, SurfaceTool st, int k, Vector3 cubePosition)
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
        st.SetNormal(directionVectors[faceDirection]);
        
        st.AddVertex(getAdder(0) * .5f + cubePosition);
        st.AddVertex(getAdder(1) * .5f + cubePosition);
        st.AddVertex(getAdder(2) * .5f + cubePosition);
        st.AddVertex(getAdder(3) * .5f + cubePosition);

        st.AddIndex(k);
        st.AddIndex(k + 1);
        st.AddIndex(k + 2);
        st.AddIndex(k + 2);
        st.AddIndex(k + 3);
        st.AddIndex(k);

        return k + 4;
    }


}
