using System;
using UnityEngine;


    public class LookAtCamera : MonoBehaviour
    {
        private void Update()
        {
            var transformRotation = this.transform.rotation;
            this.transform.LookAt(Camera.main.transform);
        }
    }
