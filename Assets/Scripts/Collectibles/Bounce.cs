using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour
{

    [SerializeField] float amplitude = 0.5f;
    [SerializeField] float frequency = 1f;

    Vector3 startPos = new Vector3();
    Vector3 tempPos = new Vector3();
    float timeOffset;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.localPosition;
        timeOffset = UnityEngine.Random.Range(-1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
        tempPos = startPos;
        tempPos.y += Mathf.Sin ((Time.fixedTime + timeOffset) * Mathf.PI * frequency) * amplitude;
 
        transform.localPosition = tempPos;
    }
}
