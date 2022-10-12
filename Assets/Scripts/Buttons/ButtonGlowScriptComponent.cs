using UnityEngine;
using UnityEngine.UI;

public class ButtonGlowScriptComponent : MonoBehaviour
{
    [SerializeField] Image myGlowImage;
    private Animator myGlowImagesAnimatorComponent;

    private void Awake()
    {
        myGlowImagesAnimatorComponent = myGlowImage.GetComponent<Animator>();
    }
    public void HandlePointerEnter()
    {
        myGlowImagesAnimatorComponent.SetTrigger("MouseEnterGlowTrigger");
    }

    public void HandlePointerExit()
    {
        myGlowImagesAnimatorComponent.SetTrigger("MouseExitDeglowTrigger");
    }

    public void ResetAllTriggers()
    {
        myGlowImagesAnimatorComponent.ResetTrigger("MouseEnterGlowTrigger");
        myGlowImagesAnimatorComponent.ResetTrigger("MouseExitDeglowTrigger");
        
        Debug.Log(myGlowImage.color);
        Color tempColor = myGlowImage.color;
        tempColor.a = 0f;
        myGlowImage.color = tempColor;
        Debug.Log(myGlowImage.color);
    }
}
