using UnityEngine;

public class VoxelObjectTool : MonoBehaviour
{
    // Hack to prevent Z-fighting
    private const float VISUALIZER_SIZE_MULTIPLIER = 0.99f;

    [SerializeField] private VoxelObject _voxelObjectPrefab;
    [SerializeField] private GameObject _visualizer;

    private Transform _transform;
    private VoxelObject _voxelObj;
    private VoxelBlock.Material _selectedMat;
    private float _rayDistance;

    private void Awake()
    {
        _transform = transform;
        _rayDistance = 2f;

        _visualizer.transform.localScale = Vector3.one * VoxelBlock.WORLD_SIZE * VISUALIZER_SIZE_MULTIPLIER;

        if (_voxelObj == null)
            _voxelObj = Instantiate(_voxelObjectPrefab);

        _selectedMat = VoxelBlock.Material.Clean;
    }

    private void LateUpdate()
    {
        _rayDistance += Input.GetAxis("Mouse ScrollWheel");
        _rayDistance = Mathf.Clamp(_rayDistance, 0.5f, 10f);

        Vector3 targetPlacePos = _transform.position + (_transform.forward * _rayDistance);
        _visualizer.transform.position = GetCursorPosition(targetPlacePos);

        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int placeLoc = VoxelObject.GetBlockLocation(targetPlacePos);
            _voxelObj.AddBlock(placeLoc, _selectedMat);
        }
    }

    private static Vector3 GetCursorPosition(Vector3 worldPos)
    {
        Vector3Int loc = VoxelObject.GetBlockLocation(worldPos);
        return new Vector3(loc.x, loc.y, loc.z) * VoxelBlock.WORLD_SIZE;
    }
}
