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

        private void Start()
        {
            var originalTransform = transform;
            offset = originalTransform.position;
            angle = originalTransform.rotation.eulerAngles.x;
        }

        private void FixedUpdate()
        {
            transform.position = followTarget.transform.position + offset;
        }
    }
}