using UnityEngine;
using UnityEngine.InputSystem;

public class PauseActivator : MonoBehaviour
{
    [SerializeField] GameObject pauseCanvas = null;
    [SerializeField] GameObject controlCanvas = null;

    private bool isGamePaused = false;

    // Start is called before the first frame update
    void Start()
    {
        pauseCanvas.SetActive(false);
    }

    public void OnPause(InputValue context)
    {
        if (!IsControlMenuActive()) isGamePaused = !isGamePaused;

        pauseCanvas.SetActive(isGamePaused);

        if (isGamePaused)
        {
            Time.timeScale = 0;
            ResetGlowingTriggers();
        }
        else
        {
            Time.timeScale = 1;
        }

        if (IsControlMenuActive()) SetControlMenuActiveOrNot(false);
    }

    private void ResetGlowingTriggers()
    {
        var buttonGlowScriptComponent = pauseCanvas.GetComponentInChildren<ButtonGlowScriptComponent>();
        if (buttonGlowScriptComponent != null)
        {
            buttonGlowScriptComponent.ResetAllTriggers();
        }
    }

    public void UnPauseGameByButton()
    {
        isGamePaused = false;
        pauseCanvas.SetActive(isGamePaused);
        Time.timeScale = 1;
    }
    
    public void PauseGameSilently()
    {
        Time.timeScale = 0;
        isGamePaused = true;
    }

    public void ReturnToPauseMenu()
    {
        pauseCanvas.SetActive(true);
        SetControlMenuActiveOrNot(false);
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    public void ShowControlMenu()
    {
        pauseCanvas.SetActive(false);
        controlCanvas.SetActive(true);
    }

    public void CloseControlMenu()
    {
        pauseCanvas.SetActive(true);
        controlCanvas.SetActive(false);
    }


    public void SetControlMenuActiveOrNot(bool value)
    {
        controlCanvas.SetActive(value);
    }

    private bool IsControlMenuActive()
    {
        return controlCanvas.activeSelf;
    }
}
