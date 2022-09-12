using UnityEngine;

public class Eyeball : MonoBehaviour
{
  public Transform target;
  private Animator meshAnimator;

  // Public variables are used by EyeballManager to decide when to dive and when to surface.

  // Which cell the eyeball is currently in.
  public Coords coords;

  // Eyeballs will only surface after some time has passed since they dove.
  public float timeDove = 0f;

  // The height of the custard under an eyeball.
  public int custardAmount;

  // The y position to Lerp to.
  public float targetHeight;


  void Awake()
  {
    meshAnimator = GetComponentInChildren<Animator>();
  }

  void Update()
  {
    Vector3 targetPosition = target.position;
    targetPosition.y = targetPosition.y + 1.5f;
    Vector3 direction = targetPosition - transform.position;
    Quaternion rotation = Quaternion.LookRotation(direction);
    transform.rotation = rotation;

    // This is a hack to make the eyeball move to a new custard height if the custard level changes beneath it.
    // When the EyeballManager sets this transform's position, it also sets the targetHeight to be equal to the y coordinate.
    // If the custard level changes, the EyeballManager will set a new targetHeight and call Dive() so that the dive animation looks correct.
    if (targetHeight != transform.position.y)
    {
      float newHeight = Mathf.Lerp(transform.position.y, targetHeight, 5f * Time.deltaTime);
      transform.position = new Vector3(transform.position.x, newHeight, transform.position.z);
    }
  }

  public void Surface()
  {
    meshAnimator.SetBool("IsOnSurface", true);
  }

  public void Dive()
  {
    meshAnimator.SetBool("IsOnSurface", false);
    timeDove = Time.time;
  }

  public bool IsSurfaced()
  {
    return meshAnimator.GetBool("IsOnSurface");
  }
}
