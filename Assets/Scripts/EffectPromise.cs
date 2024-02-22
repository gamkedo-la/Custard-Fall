using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

public class EffectPromise : MonoBehaviour
{
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private float duration = 1;

    public void PlayEffect(Action onFinish)
    {
        vfx.gameObject.SetActive(true);
        vfx.Play();
        StartCoroutine(WaitTillEffectEnd());
        onFinish?.Invoke();
    }

    private IEnumerator WaitTillEffectEnd()
    {
        yield return new WaitForSeconds(duration);
        vfx.gameObject.SetActive(false);
    }
}
