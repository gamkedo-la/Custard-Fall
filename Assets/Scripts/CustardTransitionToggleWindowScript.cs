using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CustardTransitionToggleWindowScript : MonoBehaviour
{
    public Image toggledOffImageWindow;
    public Image toggledOnImageWindow;

    public string currentButtonName;

    public void InvokeWindowToggle()
    {
        if (currentButtonName == "PlayButton")
        {
            SceneManager.LoadScene("Coast-World");
        }
        else if (currentButtonName == "PlayButton Original")
        {
            SceneManager.LoadScene("Tutorial-World");
        }

        toggledOffImageWindow.gameObject.SetActive(false);
        toggledOnImageWindow.gameObject.SetActive(true);
    }
}
