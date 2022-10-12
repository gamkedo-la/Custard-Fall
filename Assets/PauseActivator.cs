using UnityEngine;
using UnityEngine.InputSystem;

public class PauseActivator : MonoBehaviour
{
    [SerializeField] GameObject pauseCanvas = null;

    // Start is called before the first frame update
    void Start()
    {
        pauseCanvas.SetActive(false);
    }

    public void OnPause(InputValue context)
    {
        pauseCanvas.SetActive(!IsGamePaused());
        
        if (IsGamePaused())
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public bool IsGamePaused()
    {
        return pauseCanvas.activeSelf;
    }
}
