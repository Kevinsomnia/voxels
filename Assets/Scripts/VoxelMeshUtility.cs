using UnityEngine;
using System.Collections.Generic;

public static class VoxelMeshUtility
{
    public const int TEXTURE_ATLAS_SIZE = 8;
    public const int MAX_MATERIAL_COUNT = TEXTURE_ATLAS_SIZE * TEXTURE_ATLAS_SIZE;

    public enum Face
    {
        Left = 0,
        Right = 1,
        Up = 2,
        Down = 3,
        Forward = 4,
        Back = 5
    }

    // Vertex order: TL, TR, BL, BR
    private static readonly Vector3[] QUAD_POSITIONS = new Vector3[24] {
        // Left
        new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        // Right
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        // Up
        new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        // Down
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        // Forward
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        // Back
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f)
    };

    private static readonly Vector3[] QUAD_NORMALS = new Vector3[6] {
        Vector3.left,
        Vector3.right,
        Vector3.up,
        Vector3.down,
        Vector3.forward,
        Vector3.back
    };

    public static void AddQuad(List<Vector3> positions, List<Vector3> normals, List<Vector2> uvs, List<int> triangles, int x, int y, int z, Face face, VoxelBlock.Material material)
    {
        int indexStart = positions.Count;

        int faceIndex = (int)face;
        int positionsStart = faceIndex * 4;

        for (int i = 0; i < 4; i++)
        {
            Vector3 offset = QUAD_POSITIONS[positionsStart + i];
            positions.Add(new Vector3(offset.x + x, offset.y + y, offset.z + z));
        }

        Vector3 normal = QUAD_NORMALS[faceIndex];
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);

        uvs.Add(new Vector2(0f, 1f));
        uvs.Add(new Vector2(1f, 1f));
        uvs.Add(new Vector2(0f, 0f));
        uvs.Add(new Vector2(1f, 0f));

        // Top-right triangle
        triangles.Add(indexStart);
        triangles.Add(indexStart + 1);
        triangles.Add(indexStart + 3);
        // Bottom-left triangle
        triangles.Add(indexStart);
        triangles.Add(indexStart + 3);
        triangles.Add(indexStart + 2);
    }
}
