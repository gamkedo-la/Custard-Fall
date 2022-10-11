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
        pauseCanvas.SetActive(!pauseCanvas.activeSelf);
    }
}
