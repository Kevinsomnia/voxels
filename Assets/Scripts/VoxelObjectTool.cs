using UnityEngine;

public class VoxelObjectTool : MonoBehaviour
{
    // Hack to prevent Z-fighting
    private const float VISUALIZER_SIZE_MULTIPLIER = 0.99f;

    [SerializeField] private GameObject _visualizer;

    private Transform _transform;
    private float _rayDistance;

    private void Awake()
    {
        _transform = transform;
        _rayDistance = 2f;

        _visualizer.transform.localScale = Vector3.one * VoxelObject.VOXEL_SIZE * VISUALIZER_SIZE_MULTIPLIER;
    }

    private void LateUpdate()
    {
        _rayDistance += Input.GetAxis("Mouse ScrollWheel");
        _rayDistance = Mathf.Clamp(_rayDistance, 0.5f, 10f);

        Vector3 targetPlacePos = _transform.position + (_transform.forward * _rayDistance);
        _visualizer.transform.position = GetCursorPosition(targetPlacePos);
    }

    private static Vector3 GetCursorPosition(Vector3 worldPos)
    {
        worldPos /= VoxelObject.VOXEL_SIZE;

        int roundedX = Mathf.RoundToInt(worldPos.x);
        int roundedY = Mathf.RoundToInt(worldPos.y);
        int roundedZ = Mathf.RoundToInt(worldPos.z);
        return new Vector3(roundedX, roundedY, roundedZ) * VoxelObject.VOXEL_SIZE;
    }
}
