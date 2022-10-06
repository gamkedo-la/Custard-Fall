using UnityEngine;

public class EyeballMesh : MonoBehaviour
{
  [SerializeField] private float blinkTimeLeft;
  [SerializeField] private float blinkTime = 0f;
  public float minimumBlinkTime = 3f;
  public float maximumBlinkTime = 9f;
  public float chanceToDoubleBlink = 0.33f;
  private bool isBlinkLayerActive = false;
  private Animator animator;

  void Awake()
  {
    animator = GetComponent<Animator>();
  }

  void Update()
  {
    if (isBlinkLayerActive)
    {
      blinkTimeLeft -= Time.deltaTime;
      if (blinkTimeLeft <= 0)
      {
        animator.SetTrigger("Blink");
        ResetBlinkTimeLeft();
      }
    }
  }

  private void ResetBlinkTimeLeft()
  {
    SetRandomBlinkTime();
    blinkTimeLeft = blinkTime;
  }

  private void SetRandomBlinkTime()
  {
    if (blinkTime == 0f)
    {
      blinkTime = Random.Range(minimumBlinkTime, maximumBlinkTime);
    }
    else if (Random.value <= chanceToDoubleBlink)
    {
      blinkTime = 0f;
    }
    else
    {
      blinkTime = Random.Range(minimumBlinkTime, maximumBlinkTime);
    }
  }

  // BlinkLayer is a layer on the animator that should only be active when the eyeball is playing the idle animation.
  public void ActivateBlinkLayer()
  {
    animator.SetLayerWeight(1, 1);
    isBlinkLayerActive = true;
    ResetBlinkTimeLeft();
  }

  public void DeactivateBlinkLayer()
  {
    animator.SetLayerWeight(1, 0);
    isBlinkLayerActive = false;
  }
}
