using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class VoxelObject : MonoBehaviour
{
    // Max number of chunks in a single dimension of the voxel object.
    public const int MAX_OBJECT_SIZE = 1024;
    // Max number of blocks in a single dimension of the voxel object.
    public const int MAX_BLOCK_EDGE_SIZE = MAX_OBJECT_SIZE * VoxelChunk.CHUNK_SIZE;

    private const int MAX_CHUNK_COUNT = MAX_OBJECT_SIZE * MAX_OBJECT_SIZE * MAX_OBJECT_SIZE;

    [SerializeField] private VoxelChunk _chunkPrefab;

    private Dictionary<int, VoxelChunk> _chunks;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        _chunks = new Dictionary<int, VoxelChunk>();
    }

    public void AddBlock(Vector3Int pos, VoxelBlock.Material material, bool updateMesh = true)
    {
        Vector3Int chunkLoc = GetChunkLocation(pos);
        VoxelChunk chunk = GetOrAllocateChunk(chunkLoc);

        if (chunk != null)
        {
            // Convert to chunk local coordinate.
            Vector3Int chunkRelativeLoc = pos - (chunkLoc * VoxelChunk.CHUNK_SIZE);
            chunk.AddBlock(chunkRelativeLoc, material);

            if (updateMesh)
                chunk.UpdateMesh();
        }
    }

    public void AddBlocks(Vector3Int start, Vector3Int end, VoxelBlock.Material material, bool updateMesh = true)
    {
        Vector3Int minBlockLoc = new Vector3Int(
            Mathf.Min(start.x, end.x),
            Mathf.Min(start.y, end.y),
            Mathf.Min(start.z, end.z)
        );
        Vector3Int maxBlockLoc = new Vector3Int(
            Mathf.Max(start.x, end.x),
            Mathf.Max(start.y, end.y),
            Mathf.Max(start.z, end.z)
        );

        for (int z = minBlockLoc.z; z <= maxBlockLoc.z; z++)
        {
            for (int y = minBlockLoc.y; y <= maxBlockLoc.y; y++)
            {
                for (int x = minBlockLoc.x; x <= maxBlockLoc.x; x++)
                    AddBlock(new Vector3Int(x, y, z), material, updateMesh: false);
            }
        }

        if (updateMesh)
        {
            Vector3Int minChunkLoc = GetChunkLocation(minBlockLoc);
            Vector3Int maxChunkLoc = GetChunkLocation(maxBlockLoc);
            UpdateMeshes(minChunkLoc, maxChunkLoc);
        }
    }

    public void RemoveBlock(Vector3Int pos, bool updateMesh = true)
    {
        Vector3Int chunkLoc = GetChunkLocation(pos);
        VoxelChunk chunk = GetOrAllocateChunk(chunkLoc);

        if (chunk != null)
        {
            // Convert to chunk local coordinate.
            Vector3Int chunkRelativeLoc = pos - (chunkLoc * VoxelChunk.CHUNK_SIZE);
            chunk.ClearBlock(chunkRelativeLoc);

            if (updateMesh)
                chunk.UpdateMesh();
        }
    }

    public void RemoveBlocks(Vector3Int start, Vector3Int end, bool updateMesh = true)
    {
        Vector3Int minBlockLoc = new Vector3Int(
            Mathf.Min(start.x, end.x),
            Mathf.Min(start.y, end.y),
            Mathf.Min(start.z, end.z)
        );
        Vector3Int maxBlockLoc = new Vector3Int(
            Mathf.Max(start.x, end.x),
            Mathf.Max(start.y, end.y),
            Mathf.Max(start.z, end.z)
        );

        for (int z = minBlockLoc.z; z <= maxBlockLoc.z; z++)
        {
            for (int y = minBlockLoc.y; y <= maxBlockLoc.y; y++)
            {
                for (int x = minBlockLoc.x; x <= maxBlockLoc.x; x++)
                    RemoveBlock(new Vector3Int(x, y, z), updateMesh: false);
            }
        }

        if (updateMesh)
        {
            Vector3Int minChunkLoc = GetChunkLocation(minBlockLoc);
            Vector3Int maxChunkLoc = GetChunkLocation(maxBlockLoc);
            UpdateMeshes(minChunkLoc, maxChunkLoc);
        }
    }

    private void UpdateMeshes(Vector3Int startChunk, Vector3Int endChunk)
    {
        startChunk.x = Mathf.Clamp(startChunk.x, 0, MAX_BLOCK_EDGE_SIZE - 1);
        startChunk.y = Mathf.Clamp(startChunk.y, 0, MAX_BLOCK_EDGE_SIZE - 1);
        startChunk.z = Mathf.Clamp(startChunk.z, 0, MAX_BLOCK_EDGE_SIZE - 1);
        endChunk.x = Mathf.Clamp(endChunk.x, 0, MAX_BLOCK_EDGE_SIZE - 1);
        endChunk.y = Mathf.Clamp(endChunk.y, 0, MAX_BLOCK_EDGE_SIZE - 1);
        endChunk.z = Mathf.Clamp(endChunk.z, 0, MAX_BLOCK_EDGE_SIZE - 1);

        for (int z = startChunk.z; z <= endChunk.z; z++)
        {
            for (int y = startChunk.y; y <= endChunk.y; y++)
            {
                for (int x = startChunk.x; x <= endChunk.x; x++)
                {
                    VoxelChunk chunk = GetOrAllocateChunk(new Vector3Int(x, y, z));

                    if (chunk != null)
                        chunk.UpdateMesh();
                }
            }
        }
    }

    private VoxelChunk GetOrAllocateChunk(Vector3Int chunkLoc)
    {
        // Position bounds check.
        int chunkKey = GetChunkKey(chunkLoc.x, chunkLoc.y, chunkLoc.z);

        if (chunkKey < 0 || chunkKey >= MAX_CHUNK_COUNT)
            return null;

        VoxelChunk result;

        if (_chunks.TryGetValue(chunkKey, out VoxelChunk chunk))
        {
            result = chunk;
        }
        else
        {
            result = Instantiate(_chunkPrefab);
            Transform tr = result.transform;
            tr.parent = _transform;
            tr.localPosition = new Vector3(chunkLoc.x, chunkLoc.y, chunkLoc.z) * VoxelChunk.CHUNK_WORLD_SIZE;
            _chunks[chunkKey] = result;
        }

        return result;
    }

    public static Vector3Int GetBlockLocation(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.RoundToInt(worldPos.x / VoxelBlock.WORLD_SIZE),
            Mathf.RoundToInt(worldPos.y / VoxelBlock.WORLD_SIZE),
            Mathf.RoundToInt(worldPos.z / VoxelBlock.WORLD_SIZE)
        );
    }

    public static Vector3Int GetChunkLocation(Vector3 worldPos)
    {
        float chunkWorldSize = VoxelChunk.CHUNK_SIZE * VoxelBlock.WORLD_SIZE;

        return new Vector3Int(
            Mathf.RoundToInt(worldPos.x / chunkWorldSize),
            Mathf.RoundToInt(worldPos.y / chunkWorldSize),
            Mathf.RoundToInt(worldPos.z / chunkWorldSize)
        );
    }

    public static Vector3Int GetChunkLocation(Vector3Int blockLoc)
    {
        if (blockLoc.x < 0 || blockLoc.y < 0 || blockLoc.z < 0)
            return new Vector3Int(-1, -1, -1);

        return blockLoc / VoxelChunk.CHUNK_SIZE;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetChunkKey(int x, int y, int z)
    {
        return (z * MAX_OBJECT_SIZE * MAX_OBJECT_SIZE) + (y * MAX_OBJECT_SIZE) + x;
    }
}
