using UnityEngine;

public class FreeLook : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _mouseSensitivity = 2f;

    private Transform _transform;
    private float _rotX;
    private float _rotY;

    private void Awake()
    {
        _transform = transform;
        _rotX = _transform.eulerAngles.y;
        _rotY = -_transform.eulerAngles.x;
    }

    private void Update()
    {
        _rotX += Input.GetAxis("Mouse X") * _mouseSensitivity;
        _rotY -= Input.GetAxis("Mouse Y") * _mouseSensitivity;

        Vector3 pos = _transform.position;
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        pos += _transform.TransformDirection(inputX, 0f, inputZ) * _moveSpeed * Time.deltaTime;
        _transform.SetPositionAndRotation(pos, Quaternion.Euler(_rotY, _rotX, 0f));
    }
}
