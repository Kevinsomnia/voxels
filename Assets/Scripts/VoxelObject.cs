using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class VoxelObject : MonoBehaviour
{
    // Max number of chunks in a single dimension of the voxel object.
    public const int MAX_OBJECT_SIZE = 1024;

    private const int MAX_CHUNK_COUNT = MAX_OBJECT_SIZE * MAX_OBJECT_SIZE * MAX_OBJECT_SIZE;

    private Dictionary<int, VoxelChunk> _chunks;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        _chunks = new Dictionary<int, VoxelChunk>();
    }

    public void AddBlock(Vector3Int pos, VoxelBlock.Material material)
    {
        Vector3Int chunkLoc = GetChunkLocation(pos);

        // Ensure the requested position is within bounds. If not, do nothing.
        int chunkKey = GetChunkKey(chunkLoc.x, chunkLoc.y, chunkLoc.z);

        if (chunkKey < 0 || chunkKey >= MAX_CHUNK_COUNT)
            return;

        // Convert to chunk local coordinate.
        Vector3Int chunkRelativeLoc = pos - (chunkLoc * VoxelChunk.CHUNK_SIZE);

        // Ensure the chunk exists, then add the block to it.
        VoxelChunk chunk = GetOrAllocateChunk(chunkKey);
        chunk.AddBlock(chunkRelativeLoc, material);
    }

    private VoxelChunk GetOrAllocateChunk(int chunkKey)
    {
        VoxelChunk result;

        if (_chunks.TryGetValue(chunkKey, out VoxelChunk chunk))
        {
            result = chunk;
        }
        else
        {
            result = new VoxelChunk();
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
