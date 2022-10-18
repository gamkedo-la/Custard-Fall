using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPopup : MonoBehaviour
{

    public GameObject MyPopupCanvas;

    void Start() // start hidden
    {
        if (MyPopupCanvas) MyPopupCanvas.SetActive(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player") return;
        if (MyPopupCanvas) MyPopupCanvas.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player") return;
        if (MyPopupCanvas) MyPopupCanvas.SetActive(false);
    }

}
