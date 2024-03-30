using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform _camera;

    private enum Mode
    {
        LookAtCamera,
        LookAtCameraInverted
    }

    [SerializeField] private Mode lookAtMode;

    private void Start()
    {
        _camera = Camera.main!.transform;
    }

    private void LateUpdate()
    {
        switch (lookAtMode)
        {
            case Mode.LookAtCamera:
                transform.forward = _camera.transform.forward;
                break;
            case Mode.LookAtCameraInverted:
                transform.forward = -_camera.transform.forward;
                break;
            default:
                transform.LookAt(_camera.transform);
                break;
        }
    }
}