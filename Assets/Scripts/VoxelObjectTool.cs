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
    private const int MAX_BRUSH_REGION_SIZE = 48;

    private static readonly int _Color = Shader.PropertyToID("_Color");

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
        GUILayout.Box("1. Place\n2. Delete\n3. Paint\nScroll: Adjust place tool distance\nLeft/Right Arrow: Cycle block material");
        GUILayout.Box("Selected material: " + _selectedMat);
    }

    private void LateUpdate()
    {
        // Switch tools
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

        // Switch material.
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if ((int)_selectedMat <= 1)
            {
                _selectedMat = (VoxelBlock.Material)((int)VoxelBlock.Material.LENGTH - 1);
            }
            else
            {
                _selectedMat = (VoxelBlock.Material)((int)_selectedMat - 1);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if ((int)_selectedMat >= (int)VoxelBlock.Material.LENGTH - 1)
            {
                _selectedMat = (VoxelBlock.Material)1;
            }
            else
            {
                _selectedMat = (VoxelBlock.Material)((int)_selectedMat + 1);
            }
        }

        // Manipulate blocks
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
                    _voxelObj.PaintBlocks(startPlaceLoc, endPlaceLoc, _selectedMat);
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

        if (Physics.Raycast(_transform.position, _transform.forward, out RaycastHit hit, _rayDistance))
        {
            float normalsOffset;

            switch (_currToolType)
            {
                case ToolType.Place:
                    normalsOffset = VoxelBlock.WORLD_SIZE * 0.5f;   // Neighbor
                    break;
                default:
                    normalsOffset = -VoxelBlock.WORLD_SIZE * 0.5f;  // Target
                    break;
            }

            targetPlacePos = hit.point + (hit.normal * normalsOffset);
        }

        if (_placingBlocks)
        {
            _endPlacePos = GetCursorPosition(targetPlacePos);
        }
        else
        {
            _startPlacePos = GetCursorPosition(targetPlacePos);
            _endPlacePos = _startPlacePos;
        }

        // Constrain endPlacePos so that the size doesn't exceed the maximum.
        _endPlacePos = ConstrainTargetToMaxSize(_endPlacePos, anchor: _startPlacePos, Vector3Int.one * MAX_BRUSH_REGION_SIZE);

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
                _visualizerRenderer.material.SetColor(_Color, _placeToolColor);
                break;
            case ToolType.Delete:
                _visualizerRenderer.material.SetColor(_Color, _deleteToolColor);
                break;
            case ToolType.Paint:
                _visualizerRenderer.material.SetColor(_Color, _paintToolColor);
                break;
        }
    }

    private static Vector3 GetCursorPosition(Vector3 worldPos)
    {
        Vector3Int loc = VoxelObject.GetBlockLocation(worldPos);
        return new Vector3(loc.x, loc.y, loc.z) * VoxelBlock.WORLD_SIZE;
    }

    private static Vector3 ConstrainTargetToMaxSize(Vector3 target, Vector3 anchor, Vector3Int maxBlockSize)
    {
        Vector3 maxSize = ((Vector3)maxBlockSize - Vector3.one) * VoxelBlock.WORLD_SIZE;
        Vector3 diff = target - anchor;
        Vector3 result = target;

        if (Mathf.Abs(diff.x) > maxSize.x)
            result.x = (diff.x > 0f) ? anchor.x + maxSize.x : anchor.x - maxSize.x;
        if (Mathf.Abs(diff.y) > maxSize.y)
            result.y = (diff.y > 0f) ? anchor.y + maxSize.y : anchor.y - maxSize.y;
        if (Mathf.Abs(diff.z) > maxSize.z)
            result.z = (diff.z > 0f) ? anchor.z + maxSize.z : anchor.z - maxSize.z;

        return result;
    }
}
