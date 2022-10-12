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

            var buttonGlowScriptComponent = pauseCanvas.GetComponentInChildren<ButtonGlowScriptComponent>();
            Debug.Log(buttonGlowScriptComponent);

            if (buttonGlowScriptComponent != null)
            {
                buttonGlowScriptComponent.ResetAllTriggers();
            }
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
