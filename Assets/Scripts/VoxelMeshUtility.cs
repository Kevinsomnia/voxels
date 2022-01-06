using UnityEngine;
using System.Collections.Generic;

public static class VoxelMeshUtility
{
    public const int TEXTURE_ATLAS_SIZE = 4;
    public const int MAX_MATERIAL_COUNT = TEXTURE_ATLAS_SIZE * TEXTURE_ATLAS_SIZE;

    private const float UV_SCALE = 1.0f / TEXTURE_ATLAS_SIZE;
    // Avoid sampling another tile.
    private const float UV_INSET = 0.005f;
    private const float UV_INSET_INVERTED = 1f - UV_INSET;

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

    private static readonly List<Vector3> _globalPositionsBuffer = new List<Vector3>();
    private static readonly List<Vector3> _globalNormalsBuffer = new List<Vector3>();
    private static readonly List<Vector2> _globalUvsBuffer = new List<Vector2>();
    private static readonly List<int> _globalTrianglesBuffer = new List<int>();

    public static void BeginNewMesh()
    {
        _globalPositionsBuffer.Clear();
        _globalNormalsBuffer.Clear();
        _globalUvsBuffer.Clear();
        _globalTrianglesBuffer.Clear();
    }

    public static void AddQuad(int x, int y, int z, Face face, VoxelBlock.Material material)
    {
        int indexStart = _globalPositionsBuffer.Count;

        int faceIndex = (int)face;
        int positionsStart = faceIndex * 4;

        for (int i = 0; i < 4; i++)
        {
            Vector3 offset = QUAD_POSITIONS[positionsStart + i];
            _globalPositionsBuffer.Add(new Vector3(offset.x + x, offset.y + y, offset.z + z));
        }

        Vector3 normal = QUAD_NORMALS[faceIndex];
        _globalNormalsBuffer.Add(normal);
        _globalNormalsBuffer.Add(normal);
        _globalNormalsBuffer.Add(normal);
        _globalNormalsBuffer.Add(normal);

        int materialIndex = (int)material - 1;
        float uvOffsX = materialIndex;
        float uvOffsY = materialIndex / TEXTURE_ATLAS_SIZE;
        _globalUvsBuffer.Add(new Vector2(UV_INSET + uvOffsX, UV_INSET_INVERTED + uvOffsY) * UV_SCALE);
        _globalUvsBuffer.Add(new Vector2(UV_INSET_INVERTED + uvOffsX, UV_INSET_INVERTED + uvOffsY) * UV_SCALE);
        _globalUvsBuffer.Add(new Vector2(UV_INSET + uvOffsX, UV_INSET + uvOffsY) * UV_SCALE);
        _globalUvsBuffer.Add(new Vector2(UV_INSET_INVERTED + uvOffsX, UV_INSET + uvOffsY) * UV_SCALE);

        // Top-right triangle
        _globalTrianglesBuffer.Add(indexStart);
        _globalTrianglesBuffer.Add(indexStart + 1);
        _globalTrianglesBuffer.Add(indexStart + 3);
        // Bottom-left triangle
        _globalTrianglesBuffer.Add(indexStart);
        _globalTrianglesBuffer.Add(indexStart + 3);
        _globalTrianglesBuffer.Add(indexStart + 2);
    }

    public static List<Vector3> GetVertexPositions()
    {
        return _globalPositionsBuffer;
    }

    public static List<Vector3> GetVertexNormals()
    {
        return _globalNormalsBuffer;
    }

    public static List<Vector2> GetVertexUVs()
    {
        return _globalUvsBuffer;
    }

    public static List<int> GetTriangles()
    {
        return _globalTrianglesBuffer;
    }
}
