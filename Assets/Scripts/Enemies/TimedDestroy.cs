using System.Collections;
using UnityEngine;


public class TimedDestroy : MonoBehaviour
{
    [SerializeField] private float maxLifespan = 5f;

    private void Start()
    {
        StartCoroutine(KillMe());
    }


    private IEnumerator KillMe()
    {
        yield return new WaitForSeconds(maxLifespan);
        Debug.Log("killing old projectile");
        Destroy(gameObject);
    }
}