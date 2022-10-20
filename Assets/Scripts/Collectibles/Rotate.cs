using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{

    [SerializeField] float speed = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        transform.Rotate(new Vector3(0, UnityEngine.Random.value * 360, 0) * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.Rotate(new Vector3(0, speed, 0) * Time.deltaTime);
    }
}
