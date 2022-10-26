using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleSystem : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] public GameObject grappleHook;
    [Header("Positioning")]
    [SerializeField] public Transform storedTransform;
    [SerializeField] public Transform windupTransform;
    private void Awake() {
        StoredState.onStateEnter += PutAwayGrappleHook;
    }
    public void PutAwayGrappleHook(object sender, System.EventArgs e){
        grappleHook.transform.position = storedTransform.position;
        grappleHook.transform.rotation = storedTransform.rotation;
    }
}
