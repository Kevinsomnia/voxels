using UnityEngine;

public struct VoxelBlock
{
    public const float WORLD_SIZE = 0.25f;

    public enum Material : byte
    {
        Empty = 0,  // No block
        Clean,
        Dirt
    }

    public Material material;
}
