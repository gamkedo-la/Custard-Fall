using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CozyBar : MonoBehaviour
{

    [SerializeField]
    private CozinessReceiver cozinessReceiver;
    
    [SerializeField]
    private Slider slider;

    private void Update()
    {
        slider.maxValue = cozinessReceiver.CozyLevel;
        slider.value = Mathf.Lerp(slider.value,cozinessReceiver.FeelingCozy, Time.deltaTime);
    }
}
