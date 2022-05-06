using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustardBlock : MonoBehaviour
{

    void Start()
    {
        gameObject.SetActive(false);
    }
    
    void Update()
    {
        
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Debug.Log("#custard "+transform.position);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
