using System;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace
{
    public class CameraController : MonoBehaviour
    {
        public GameObject followTarget;
        private Vector3 _offset;

        private Vector3 _targetPosition = Vector3.zero;
        private Vector3 _currentPosition = Vector3.zero;
        private Vector3 _currentVelocity = Vector3.zero;
        [SerializeField] private float followSpeed = 0.075f;

        private void Start()
        {
            var originalTransform = transform;
            _offset = originalTransform.position;
            
            transform.position = FindCameraTargetPosition();
        }

        private void FixedUpdate()
        {
            _targetPosition = FindCameraTargetPosition();

            _currentPosition = Vector3.SmoothDamp(_currentPosition, _targetPosition, ref _currentVelocity, followSpeed);
            transform.position = _currentPosition;
        }

        private Vector3 FindCameraTargetPosition()
        {
            return followTarget.transform.position + _offset;
        }
    }
}