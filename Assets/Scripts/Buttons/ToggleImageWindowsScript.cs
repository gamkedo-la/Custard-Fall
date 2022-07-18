using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleImageWindowsScript : MonoBehaviour
{
    [SerializeField] Image toggledOffImageWindow;
    [SerializeField] Image toggledOnImageWindow;

    [SerializeField] Image custardTransitionImage;
    private Animator custardTransitionAnimatorComponent;
    private CustardTransitionToggleWindowScript custardTransitionToggleWindowScript;

    private void Awake()
    {
        custardTransitionAnimatorComponent = custardTransitionImage.GetComponent<Animator>();
        custardTransitionToggleWindowScript = custardTransitionImage.GetComponent<CustardTransitionToggleWindowScript>();
    }
    public void ToggleImageWindows()
    {
        toggledOnImageWindow.gameObject.SetActive(true);
        toggledOffImageWindow.gameObject.SetActive(false);
    }

    public void HandleButtonClick()
    {
        TurnOnCustardTransitionImageAndAnimation();
    }

    public void TurnOnCustardTransitionImageAndAnimation()
    {
        custardTransitionToggleWindowScript.toggledOffImageWindow = toggledOffImageWindow;
        custardTransitionToggleWindowScript.toggledOnImageWindow = toggledOnImageWindow;
        custardTransitionToggleWindowScript.currentButtonName = gameObject.name;
        custardTransitionImage.gameObject.SetActive(true);
        custardTransitionAnimatorComponent.SetTrigger("CustardTransitionTrigger");
    }
}
