using UnityEngine;

public class VoxelObjectTool : MonoBehaviour
{
    // Hack to prevent Z-fighting
    private const float VISUALIZER_SIZE_MULTIPLIER = 0.99f;

    [SerializeField] private VoxelObject _voxelObjectPrefab;
    [SerializeField] private GameObject _visualizer;

    private Transform _transform;
    private Transform _visualizerTransform;
    private VoxelObject _voxelObj;
    private VoxelBlock.Material _selectedMat;
    private bool _placingBlocks;
    private Vector3 _startPlacePos;
    private Vector3 _endPlacePos;
    private float _rayDistance;

    private void Awake()
    {
        _transform = transform;
        _visualizerTransform = _visualizer.transform;
        _rayDistance = 2f;

        _visualizerTransform.localScale = Vector3.one * VoxelBlock.WORLD_SIZE * VISUALIZER_SIZE_MULTIPLIER;

        if (_voxelObj == null)
            _voxelObj = Instantiate(_voxelObjectPrefab);

        _selectedMat = VoxelBlock.Material.Clean;
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _placingBlocks = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _placingBlocks = false;
            Vector3Int startPlaceLoc = VoxelObject.GetBlockLocation(_startPlacePos);
            Vector3Int endPlaceLoc = VoxelObject.GetBlockLocation(_endPlacePos);
            _voxelObj.AddBlocks(startPlaceLoc, endPlaceLoc, _selectedMat);
        }

        HandlePlacementVisualizer();
    }

    private void HandlePlacementVisualizer()
    {
        _rayDistance += Input.GetAxis("Mouse ScrollWheel");
        _rayDistance = Mathf.Clamp(_rayDistance, 0.5f, 10f);

        Vector3 targetPlacePos = _transform.position + (_transform.forward * _rayDistance);

        if (_placingBlocks)
        {
            _endPlacePos = GetCursorPosition(targetPlacePos);
        }
        else
        {
            _startPlacePos = GetCursorPosition(targetPlacePos);
            _endPlacePos = _startPlacePos;
        }

        _visualizerTransform.position = (_startPlacePos + _endPlacePos) * 0.5f;
        Vector3 scale = _endPlacePos - _startPlacePos;
        scale.x = Mathf.Abs(scale.x) + VoxelBlock.WORLD_SIZE;
        scale.y = Mathf.Abs(scale.y) + VoxelBlock.WORLD_SIZE;
        scale.z = Mathf.Abs(scale.z) + VoxelBlock.WORLD_SIZE;
        _visualizerTransform.localScale = scale;
    }

    private static Vector3 GetCursorPosition(Vector3 worldPos)
    {
        Vector3Int loc = VoxelObject.GetBlockLocation(worldPos);
        return new Vector3(loc.x, loc.y, loc.z) * VoxelBlock.WORLD_SIZE;
    }
}
