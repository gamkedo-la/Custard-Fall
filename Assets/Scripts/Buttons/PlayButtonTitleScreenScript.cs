using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButtonTitleScreenScript : MonoBehaviour
{
    public void HandleButtonClick()
    {
        // not used at the moment, see CustardTransitionToggleWindowScript.InvokeWindowToggle
        SceneManager.LoadScene("Coast-World");
    }
    
    public void PlayClassic()
    {
        // not used at the moment, see CustardTransitionToggleWindowScript.InvokeWindowToggle
        SceneManager.LoadScene("Tutorial-World");
    }    
}
