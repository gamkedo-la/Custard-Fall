using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleImageWindowsScript : MonoBehaviour
{
    [SerializeField] Image toggledOffImageWindow;
    [SerializeField] Image toggledOnImageWindow;

    public void HandleButtonClick()
    {
        toggledOnImageWindow.gameObject.SetActive(true);
        toggledOffImageWindow.gameObject.SetActive(false);
    }
}
