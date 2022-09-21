using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButtonTitleScreenScript : MonoBehaviour
{
    public void HandleButtonClick()
    {
        SceneManager.LoadScene("Tutorial-World");
    }    
}
