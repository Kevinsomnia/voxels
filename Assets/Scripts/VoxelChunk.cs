using UnityEngine;
using System.Runtime.CompilerServices;

public class VoxelChunk
{
    // Number of blocks in a single dimension of a chunk
    public const int CHUNK_SIZE = 16;

    private const int BLOCK_COUNT = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;

    public VoxelBlock[] blocks;

    private Mesh _mesh;

    public VoxelChunk()
    {
        blocks = new VoxelBlock[BLOCK_COUNT];

        _mesh = new Mesh();
        _mesh.MarkDynamic();
    }

    public void Dispose()
    {
        Object.Destroy(_mesh);
        _mesh = null;
    }

    public void AddBlock(Vector3Int position, VoxelBlock.Material material)
    {
        int index = GetIndex(position.x, position.y, position.z);

        if (index < 0 || index >= BLOCK_COUNT)
            throw new System.IndexOutOfRangeException("position is outside of chunk bounds");

        blocks[index].material = material;
    }

    public void SetBlock(Vector3Int position, VoxelBlock.Material material)
    {
        int index = GetIndex(position.x, position.y, position.z);

        if (index < 0 || index >= BLOCK_COUNT)
            throw new System.IndexOutOfRangeException("position is outside of chunk bounds");

        blocks[index].material = material;
    }

    public void ClearBlock(Vector3Int position)
    {
        int index = GetIndex(position.x, position.y, position.z);

        if (index < 0 || index >= BLOCK_COUNT)
            throw new System.IndexOutOfRangeException("position is outside of chunk bounds");

        blocks[index].material = VoxelBlock.Material.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(int x, int y, int z)
    {
        return (z * CHUNK_SIZE * CHUNK_SIZE) + (y * CHUNK_SIZE) + x;
    }
}
