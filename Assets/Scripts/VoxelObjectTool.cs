using UnityEngine;

public class VoxelObjectTool : MonoBehaviour
{
    public enum ToolType
    {
        Place,
        Delete,
        Paint
    }

    private const float VISUALIZER_SIZE_MULTIPLIER = 1.05f;

    [SerializeField] private VoxelObject _voxelObjectPrefab;
    [SerializeField] private GameObject _visualizer;
    [SerializeField] private Color _placeToolColor = Color.blue;
    [SerializeField] private Color _deleteToolColor = Color.red;
    [SerializeField] private Color _paintToolColor = Color.green;

    private Transform _transform;
    private Transform _visualizerTransform;
    private MeshRenderer _visualizerRenderer;
    private VoxelObject _voxelObj;
    private ToolType _currToolType;
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
        _visualizerRenderer = _visualizer.GetComponent<MeshRenderer>();
        _visualizerRenderer.material.SetFloat("_GridScale", VoxelBlock.WORLD_SIZE);

        if (_voxelObj == null)
            _voxelObj = Instantiate(_voxelObjectPrefab);

        SetToolType(ToolType.Place);
        _selectedMat = VoxelBlock.Material.Clean;
    }

    private void OnGUI()
    {
        GUILayout.Box("1. Place\n2. Delete\n3. Paint");
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetToolType(ToolType.Place);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetToolType(ToolType.Delete);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetToolType(ToolType.Paint);
        }

        if (Input.GetMouseButtonDown(0))
        {
            _placingBlocks = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _placingBlocks = false;
            Vector3Int startPlaceLoc = VoxelObject.GetBlockLocation(_startPlacePos);
            Vector3Int endPlaceLoc = VoxelObject.GetBlockLocation(_endPlacePos);

            switch (_currToolType)
            {
                case ToolType.Place:
                    _voxelObj.AddBlocks(startPlaceLoc, endPlaceLoc, _selectedMat);
                    break;
                case ToolType.Delete:
                    _voxelObj.RemoveBlocks(startPlaceLoc, endPlaceLoc);
                    break;
                case ToolType.Paint:
                    // _voxelObj.PaintBlocks(startPlaceLoc, endPlaceLoc, _selectedMat);
                    break;
            }
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
        scale.x = Mathf.Abs(scale.x) + (VoxelBlock.WORLD_SIZE * VISUALIZER_SIZE_MULTIPLIER);
        scale.y = Mathf.Abs(scale.y) + (VoxelBlock.WORLD_SIZE * VISUALIZER_SIZE_MULTIPLIER);
        scale.z = Mathf.Abs(scale.z) + (VoxelBlock.WORLD_SIZE * VISUALIZER_SIZE_MULTIPLIER);
        _visualizerTransform.localScale = scale;
    }

    private void SetToolType(ToolType type)
    {
        _currToolType = type;

        switch (_currToolType)
        {
            case ToolType.Place:
                _visualizerRenderer.material.SetColor("_Color", _placeToolColor);
                break;
            case ToolType.Delete:
                _visualizerRenderer.material.SetColor("_Color", _deleteToolColor);
                break;
            case ToolType.Paint:
                _visualizerRenderer.material.SetColor("_Color", _paintToolColor);
                break;
        }
    }

    private static Vector3 GetCursorPosition(Vector3 worldPos)
    {
        Vector3Int loc = VoxelObject.GetBlockLocation(worldPos);
        return new Vector3(loc.x, loc.y, loc.z) * VoxelBlock.WORLD_SIZE;
    }
}
