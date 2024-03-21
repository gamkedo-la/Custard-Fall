using System;
using UnityEngine;


public class VfxInstance : MonoBehaviour
{
    [SerializeField] private ParticleSystem vfx;
    

    private void Start()
    {
        vfx.gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (vfx.isStopped)
        {
            Destroy(this);
        }
    }

    public static VfxInstance Spawn(VfxInstance prefab, Vector3 position, Quaternion rotation)
    {
        return Instantiate(prefab, position, rotation);
    }
}