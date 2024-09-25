using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public static World Instance { get; private set; }
    public Material VoxelMaterial;
    public int worldSize = 5;
    public int chunkSize = 16;
    private Dictionary<Vector3, Chunk> chunks;
    public int noiseSeed = 1234;
    public float maxHeight = 0.2f;
    public float noiseScale = 0.015f;
    public float[,] noiseArray;
    PlayerController playerController;
    Vector3 playerPosition;
    public int loadRadius = 5;
    public int unloadRadius = 7;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: if you want this to persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        noiseArray = GlobalNoise.GetNoise();
    }

    // Start is called before the first frame update
    void Start()
    {
        GlobalNoise.SetSeed();
        playerController = FindObjectOfType<PlayerController>();
        chunks = new Dictionary<Vector3, Chunk>();
    }

    private void Update()
    {
        playerPosition = playerController.getPlayerPosition();
        UpdateChunks(playerPosition);
    }
    void UpdateChunks(Vector3 playerPosition)
    {
        // Determine the chunk coordinates for the player's position
        Vector3Int playerChunkCoordinates = new Vector3Int(
            Mathf.FloorToInt(playerPosition.x / chunkSize),
            Mathf.FloorToInt(playerPosition.y / chunkSize),
            Mathf.FloorToInt(playerPosition.z / chunkSize));

        // Load and unload chunks based on the player's position
        LoadChunksAround(playerChunkCoordinates);
        UnloadDistantChunks(playerChunkCoordinates);
    }
    void LoadChunksAround(Vector3Int centerChunkCoordinates)
    {
        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int z = -loadRadius; z <= loadRadius; z++)
            {
                Vector3Int chunkCoordinates = new Vector3Int(centerChunkCoordinates.x + x, 0, centerChunkCoordinates.z + z);
                Vector3 chunkPosition = new Vector3(chunkCoordinates.x * chunkSize, 0, chunkCoordinates.z * chunkSize);
                if (!chunks.ContainsKey(chunkPosition))
                {
                    GameObject chunkObject = new GameObject($"Chunk_{chunkCoordinates.x}_{chunkCoordinates.z}");
                    chunkObject.transform.position = chunkPosition;
                    chunkObject.transform.parent = this.transform; // Optional, for organizational purposes

                    Chunk newChunk = chunkObject.AddComponent<Chunk>();
                    newChunk.Initialize(chunkSize); // Initialize the chunk with its size

                    chunks.Add(chunkPosition, newChunk); // Add the chunk to the dictionary
                }
            }
        }
    }

    void UnloadDistantChunks(Vector3Int centerChunkCoordinates)
    {
        List<Vector3> chunksToUnload = new List<Vector3>();
        foreach (var chunk in chunks)
        {
            Vector3Int chunkCoord = new Vector3Int(
                Mathf.FloorToInt(chunk.Key.x / chunkSize),
                Mathf.FloorToInt(chunk.Key.y / chunkSize),
                Mathf.FloorToInt(chunk.Key.z / chunkSize));

            if (Vector3Int.Distance(chunkCoord, centerChunkCoordinates) > unloadRadius)
            {
                chunksToUnload.Add(chunk.Key);
            }
        }

        foreach (var chunkPos in chunksToUnload)
        {
            Destroy(chunks[chunkPos].gameObject);
            chunks.Remove(chunkPos);
        }
    }

    public Chunk GetChunkAt(Vector3 globalPosition)
    {
        // Calculate the chunk's starting position based on the global position
        Vector3Int chunkCoordinates = new Vector3Int(
            Mathf.FloorToInt(globalPosition.x / chunkSize) * chunkSize,
            Mathf.FloorToInt(globalPosition.y / chunkSize) * chunkSize,
            Mathf.FloorToInt(globalPosition.z / chunkSize) * chunkSize
        );

        // Retrieve and return the chunk at the calculated position
        if (chunks.TryGetValue(chunkCoordinates, out Chunk chunk))
        {
            return chunk;
        }

        // Return null if no chunk exists at the position
        return null;
    }

   

}
