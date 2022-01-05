using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class VoxelChunk : MonoBehaviour
{
    public class BlockGroup
    {
        public GameObject gameObject;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public Mesh mesh;

        public void Dispose()
        {
            Destroy(gameObject);
            Destroy(mesh);
        }
    }

    // Number of blocks in a single dimension of a chunk
    public const int CHUNK_SIZE = 16;
    public const float CHUNK_WORLD_SIZE = CHUNK_SIZE * VoxelBlock.WORLD_SIZE;

    private const int MAX_BLOCK_COUNT = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;

    // Should be the same order as the VoxelBlock.Material enum
    [SerializeField] private Material[] _blockMaterials;

    // Raw block data used to build the meshes.
    private VoxelBlock[] _blocks;
    // Meshes are grouped per block material.
    private BlockGroup[] _groups;
    // Share lists for performance and mesh buffers
    private List<Vector3> _positionsBuffer;
    private List<Vector3> _normalsBuffer;
    private List<Vector2> _uvsBuffer;
    private List<int> _trianglesBuffer;

    private void Awake()
    {
        _blocks = new VoxelBlock[MAX_BLOCK_COUNT];
        _groups = new BlockGroup[(int)VoxelBlock.Material.LENGTH - 1];

        _positionsBuffer = new List<Vector3>();
        _normalsBuffer = new List<Vector3>();
        _uvsBuffer = new List<Vector2>();
        _trianglesBuffer = new List<int>();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _groups.Length; i++)
        {
            if (_groups[i] != null)
                _groups[i].Dispose();
        }
    }

    public void AddBlock(Vector3Int position, VoxelBlock.Material material)
    {
        int index = GetBlockIndex(position.x, position.y, position.z);

        if (_blocks[index].material == VoxelBlock.Material.Empty)
            _blocks[index].material = material;
    }

    public void SetBlock(Vector3Int position, VoxelBlock.Material material)
    {
        int index = GetBlockIndex(position.x, position.y, position.z);
        _blocks[index].material = material;
        GetOrCreateGroupForMaterial(material);
        UpdateMeshForMaterial(material);
    }

    public void ClearBlock(Vector3Int position)
    {
        int index = GetBlockIndex(position.x, position.y, position.z);
        VoxelBlock.Material prevMat = _blocks[index].material;

        if (prevMat != VoxelBlock.Material.Empty)
        {
            _blocks[index].material = VoxelBlock.Material.Empty;
            UpdateMeshForMaterial(prevMat);
        }
    }

    private BlockGroup GetOrCreateGroupForMaterial(VoxelBlock.Material material)
    {
        int groupIndex = (int)material - 1;
        BlockGroup group = _groups[groupIndex];

        if (group != null)
            return group;

        group = new BlockGroup();

        group.gameObject = new GameObject("Group");
        Transform groupTrans = group.gameObject.transform;
        groupTrans.SetParent(transform, worldPositionStays: false);
        groupTrans.localScale = Vector3.one * VoxelBlock.WORLD_SIZE;
        group.meshFilter = group.gameObject.AddComponent<MeshFilter>();
        group.meshRenderer = group.gameObject.AddComponent<MeshRenderer>();

        group.mesh = new Mesh();
        group.meshFilter.sharedMesh = group.mesh;
        group.meshRenderer.sharedMaterial = _blockMaterials[groupIndex];

        _groups[groupIndex] = group;
        return group;
    }

    public void ForceUpdateMesh()
    {
        for (int i = 0; i < _groups.Length; i++)
        {
            UpdateMeshForMaterial((VoxelBlock.Material)(i + 1));
        }
    }

    public void UpdateMeshForMaterial(VoxelBlock.Material material)
    {
        GetOrCreateGroupForMaterial(material);
        int groupIndex = (int)material - 1;
        BlockGroup group = _groups[groupIndex];

        if (group == null)
            return;

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

                    if (_blocks[mainBlockIndex].material != VoxelBlock.Material.Empty)
                    {
                        if (x <= 0 || _blocks[GetBlockIndex(x - 1, y, z)].material == VoxelBlock.Material.Empty)
                        {
                            VoxelMeshUtility.AddQuad(
                                _positionsBuffer, _normalsBuffer, _uvsBuffer, _trianglesBuffer,
                                x, y, z, VoxelMeshUtility.Face.Left, _blocks[mainBlockIndex].material
                            );
                        }
                        if (x >= CHUNK_SIZE - 1 || _blocks[GetBlockIndex(x + 1, y, z)].material == VoxelBlock.Material.Empty)
                        {
                            VoxelMeshUtility.AddQuad(
                                _positionsBuffer, _normalsBuffer, _uvsBuffer, _trianglesBuffer,
                                x, y, z, VoxelMeshUtility.Face.Right, _blocks[mainBlockIndex].material
                            );
                        }
                        if (y >= CHUNK_SIZE - 1 || _blocks[GetBlockIndex(x, y + 1, z)].material == VoxelBlock.Material.Empty)
                        {
                            VoxelMeshUtility.AddQuad(
                                _positionsBuffer, _normalsBuffer, _uvsBuffer, _trianglesBuffer,
                                x, y, z, VoxelMeshUtility.Face.Up, _blocks[mainBlockIndex].material
                            );
                        }
                        if (y <= 0 || _blocks[GetBlockIndex(x, y - 1, z)].material == VoxelBlock.Material.Empty)
                        {
                            VoxelMeshUtility.AddQuad(
                                _positionsBuffer, _normalsBuffer, _uvsBuffer, _trianglesBuffer,
                                x, y, z, VoxelMeshUtility.Face.Down, _blocks[mainBlockIndex].material
                            );
                        }
                        if (z >= CHUNK_SIZE - 1 || _blocks[GetBlockIndex(x, y, z + 1)].material == VoxelBlock.Material.Empty)
                        {
                            VoxelMeshUtility.AddQuad(
                                _positionsBuffer, _normalsBuffer, _uvsBuffer, _trianglesBuffer,
                                x, y, z, VoxelMeshUtility.Face.Forward, _blocks[mainBlockIndex].material
                            );
                        }
                        if (z <= 0 || _blocks[GetBlockIndex(x, y, z - 1)].material == VoxelBlock.Material.Empty)
                        {
                            VoxelMeshUtility.AddQuad(
                                _positionsBuffer, _normalsBuffer, _uvsBuffer, _trianglesBuffer,
                                x, y, z, VoxelMeshUtility.Face.Back, _blocks[mainBlockIndex].material
                            );
                        }
                    }
                }
            }
        }

        Mesh mesh = _groups[groupIndex].mesh;
        mesh.Clear();
        mesh.MarkDynamic();
        mesh.vertices = _positionsBuffer.ToArray();
        mesh.normals = _normalsBuffer.ToArray();
        mesh.uv = _uvsBuffer.ToArray();
        mesh.triangles = _trianglesBuffer.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBlockIndex(int x, int y, int z)
    {
        return (z * CHUNK_SIZE * CHUNK_SIZE) + (y * CHUNK_SIZE) + x;
    }
}
