using System;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace
{
    public class CameraController : MonoBehaviour
    {
        public GameObject followTarget;
        private Vector3 offset;
        private double angle;

        private Vector3 targetPosition = Vector3.zero;
        private Vector3 currentPosition = Vector3.zero;
        private Vector3 currentVelocity = Vector3.zero;
        [SerializeField] private float followSpeed = 0.075f;

        private void Start()
        {
            var originalTransform = transform;
            offset = originalTransform.position;
            angle = originalTransform.rotation.eulerAngles.x;
        }

        private void FixedUpdate()
        {
            targetPosition = followTarget.transform.position + offset;

            currentPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref currentVelocity, followSpeed);
            transform.position = currentPosition;
        }
    }
}