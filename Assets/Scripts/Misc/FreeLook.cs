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
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        _rotX += Input.GetAxis("Mouse X") * _mouseSensitivity;
        _rotY -= Input.GetAxis("Mouse Y") * _mouseSensitivity;

        Vector3 pos = _transform.position;
        Vector3 dir = _transform.TransformDirection(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        if (Input.GetKey(KeyCode.Space))
            dir.y += 1.0f;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            dir.y -= 1.0f;

        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        _transform.SetPositionAndRotation(pos + (dir * _moveSpeed * Time.deltaTime), Quaternion.Euler(_rotY, _rotX, 0f));
    }
}
