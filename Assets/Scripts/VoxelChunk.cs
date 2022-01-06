using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class VoxelChunk : MonoBehaviour
{
    // Number of blocks in a single dimension of a chunk
    public const int CHUNK_SIZE = 24;
    public const float CHUNK_WORLD_SIZE = CHUNK_SIZE * VoxelBlock.WORLD_SIZE;

    private const int MAX_BLOCK_COUNT = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;

    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshCollider _meshCollider;

    private Mesh _mesh;
    private bool _meshNeedsRebuilding;

    // Raw block data used to build the meshes.
    private VoxelBlock[] _blocks;
    // Share lists for performance and mesh buffers
    private List<Vector3> _positionsBuffer;
    private List<Vector3> _normalsBuffer;
    private List<Vector2> _uvsBuffer;
    private List<int> _trianglesBuffer;

    private void Awake()
    {
        _blocks = new VoxelBlock[MAX_BLOCK_COUNT];

        _mesh = new Mesh();
        _meshFilter.sharedMesh = _mesh;
        _meshNeedsRebuilding = false;

        _positionsBuffer = new List<Vector3>();
        _normalsBuffer = new List<Vector3>();
        _uvsBuffer = new List<Vector2>();
        _trianglesBuffer = new List<int>();
    }

    private void OnDestroy()
    {
        Destroy(_mesh);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VoxelBlock GetBlock(Vector3Int position)
    {
        return _blocks[GetBlockIndex(position.x, position.y, position.z)];
    }

    public void AddBlock(Vector3Int position, VoxelBlock.Material material)
    {
        int index = GetBlockIndex(position.x, position.y, position.z);

        if (_blocks[index].material == VoxelBlock.Material.Empty)
        {
            _blocks[index].material = material;
            _meshNeedsRebuilding = true;
        }
    }

    public void SetBlock(Vector3Int position, VoxelBlock.Material material)
    {
        int blockIndex = GetBlockIndex(position.x, position.y, position.z);

        if (_blocks[blockIndex].material != material)
        {
            _blocks[blockIndex].material = material;
            _meshNeedsRebuilding = true;
        }
    }

    public void PaintBlock(Vector3Int position, VoxelBlock.Material material)
    {
        int blockIndex = GetBlockIndex(position.x, position.y, position.z);

        if (_blocks[blockIndex].material != VoxelBlock.Material.Empty && _blocks[blockIndex].material != material)
        {
            _blocks[blockIndex].material = material;
            _meshNeedsRebuilding = true;
        }
    }

    public void ClearBlock(Vector3Int position)
    {
        int index = GetBlockIndex(position.x, position.y, position.z);

        if (_blocks[index].material != VoxelBlock.Material.Empty)
        {
            _blocks[index].material = VoxelBlock.Material.Empty;
            _meshNeedsRebuilding = true;
        }
    }

    public void UpdateMesh()
    {
        if (_meshNeedsRebuilding)
        {
            _meshNeedsRebuilding = false;

            _positionsBuffer.Clear();
            _normalsBuffer.Clear();
            _uvsBuffer.Clear();
            _trianglesBuffer.Clear();

            for (int z = 0; z < CHUNK_SIZE; z++)
            {
                int zIndexOffs = z * CHUNK_SIZE * CHUNK_SIZE;

                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    int yIndexOffs = y * CHUNK_SIZE;

                    for (int x = 0; x < CHUNK_SIZE; x++)
                    {
                        int mainBlockIndex = zIndexOffs + yIndexOffs + x;
                        VoxelBlock targetBlock = _blocks[mainBlockIndex];

                        if (targetBlock.material != VoxelBlock.Material.Empty)
                        {
                            if (x <= 0 || _blocks[GetBlockIndex(x - 1, y, z)].material == VoxelBlock.Material.Empty)
                            {
                                VoxelMeshUtility.AddQuad(
                                    _positionsBuffer, _normalsBuffer, _uvsBuffer, _trianglesBuffer,
                                    x, y, z, VoxelMeshUtility.Face.Left, targetBlock.material
                                );
                            }
                            if (x >= CHUNK_SIZE - 1 || _blocks[GetBlockIndex(x + 1, y, z)].material == VoxelBlock.Material.Empty)
                            {
                                VoxelMeshUtility.AddQuad(
                                    _positionsBuffer, _normalsBuffer, _uvsBuffer, _trianglesBuffer,
                                    x, y, z, VoxelMeshUtility.Face.Right, targetBlock.material
                                );
                            }
                            if (y >= CHUNK_SIZE - 1 || _blocks[GetBlockIndex(x, y + 1, z)].material == VoxelBlock.Material.Empty)
                            {
                                VoxelMeshUtility.AddQuad(
                                    _positionsBuffer, _normalsBuffer, _uvsBuffer, _trianglesBuffer,
                                    x, y, z, VoxelMeshUtility.Face.Up, targetBlock.material
                                );
                            }
                            if (y <= 0 || _blocks[GetBlockIndex(x, y - 1, z)].material == VoxelBlock.Material.Empty)
                            {
                                VoxelMeshUtility.AddQuad(
                                    _positionsBuffer, _normalsBuffer, _uvsBuffer, _trianglesBuffer,
                                    x, y, z, VoxelMeshUtility.Face.Down, targetBlock.material
                                );
                            }
                            if (z >= CHUNK_SIZE - 1 || _blocks[GetBlockIndex(x, y, z + 1)].material == VoxelBlock.Material.Empty)
                            {
                                VoxelMeshUtility.AddQuad(
                                    _positionsBuffer, _normalsBuffer, _uvsBuffer, _trianglesBuffer,
                                    x, y, z, VoxelMeshUtility.Face.Forward, targetBlock.material
                                );
                            }
                            if (z <= 0 || _blocks[GetBlockIndex(x, y, z - 1)].material == VoxelBlock.Material.Empty)
                            {
                                VoxelMeshUtility.AddQuad(
                                    _positionsBuffer, _normalsBuffer, _uvsBuffer, _trianglesBuffer,
                                    x, y, z, VoxelMeshUtility.Face.Back, targetBlock.material
                                );
                            }
                        }
                    }
                }
            }

            _mesh.Clear();
            _mesh.MarkDynamic();
            _mesh.vertices = _positionsBuffer.ToArray();
            _mesh.normals = _normalsBuffer.ToArray();
            _mesh.uv = _uvsBuffer.ToArray();
            _mesh.triangles = _trianglesBuffer.ToArray();

            // Will need to clear and re-assign the mesh to properly refresh the collider.
            _meshCollider.sharedMesh = null;
            _meshCollider.sharedMesh = _mesh;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlockIndex(int x, int y, int z)
    {
        return (z * CHUNK_SIZE * CHUNK_SIZE) + (y * CHUNK_SIZE) + x;
    }
}
