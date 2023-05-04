using System;
using UnityEngine;


    public class LookAtCamera : MonoBehaviour
    {

        private enum Mode
        {
            LookAtCamera,
            LookAtCameraInverted,
        }

        [SerializeField] private Mode lookAtMode;
    
        private void LateUpdate()
        {
            switch (lookAtMode)
            {
                case Mode.LookAtCamera:
                    this.transform.forward = Camera.main.transform.forward;
                    break;
                case Mode.LookAtCameraInverted:
                    this.transform.forward = -Camera.main.transform.forward;
                    break;
                default:
                    this.transform.LookAt(Camera.main.transform);
                    break;
            }
        }
    }
