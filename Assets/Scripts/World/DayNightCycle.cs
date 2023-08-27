using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DayNightCycle : MonoBehaviour
{
    /* [Range(0.0f, 1.0f)]
    public float time;
    public float fullDayLength;
    public float startTime = 0.4f;
    private float timeRate; */
    public Vector3 noon;
    public float offsetDay = .09f;
    public float offsetNight = -.12f;

    public Vector3 dawnRotation = new Vector3(3, 0, 0);
    public Vector3 duskRotation = new Vector3(177, 0, 0);

    [NonSerialized]
    float time;
    public AnimationCurve timeProgressionModifier;

    [Header("Sun")]
    public Light sun;
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    [Header("Moon")]
    public Light moon;
    public Gradient moonColor;
    public AnimationCurve moonIntensity;

    [Header("Other Lighting")]
    public AnimationCurve lightingIntensityMultiplier;
    public AnimationCurve reflectionsIntensityMultiplier;

    void Update ()
    {
        //increment time
        time = TimeManager.Instance.time;
        time = timeProgressionModifier.Evaluate(time);

        if(time >= 1.0f)
            time = 0.0f;
        
        var offsetDayTime = time + offsetDay;
        if(offsetDayTime >= 1.0f)
            offsetDayTime = 0;
        
        var offsetNightTime = time + offsetNight;
        if(offsetNightTime >= 1.0f)
            offsetNightTime = 0;

        // light rotation
        sun.transform.eulerAngles = noon * ((time - TimeManager.Instance.dayStart + offsetDay) * 4.0f);
        moon.transform.eulerAngles = noon * ((time - TimeManager.Instance.nightStart + offsetNightTime) * 4.0f);
        
        // original calculation
        // sun.transform.eulerAngles = noon * ((time - TimeManager.Instance.dayStart) * 4.0f);
        // moon.transform.eulerAngles = noon * ((time - TimeManager.Instance.nightStart) * 4.0f);
        
        // sun.transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(duskRotation, Vector3.up), Quaternion.LookRotation(dawnRotation, Vector3.up), time);
        // moon.transform.rotation = Quaternion.Slerp(Quaternion.LookRotation(-duskRotation, Vector3.up), Quaternion.LookRotation(-dawnRotation, Vector3.up), time);

        // light intensity
        
        sun.intensity = sunIntensity.Evaluate(offsetDayTime);
        moon.intensity = moonIntensity.Evaluate(offsetNightTime);

        // change colors
        sun.color = sunColor.Evaluate(time);
        moon.color = moonColor.Evaluate(offsetNightTime);

        // enable / disable the sun
        bool showSun = sun.intensity > 0;
        if(sun.gameObject.activeInHierarchy != showSun)
        {
            sun.gameObject.SetActive(showSun);
        }

        // enable / disable the moon
        if (moon.intensity == 0 && moon.gameObject.activeInHierarchy)
            moon.gameObject.SetActive(false);
        else if(moon.intensity > 0 && !moon.gameObject.activeInHierarchy)
            moon.gameObject.SetActive(true);

        // lighting and reflections intensity
        RenderSettings.ambientIntensity = lightingIntensityMultiplier.Evaluate(offsetDayTime);
        RenderSettings.reflectionIntensity = reflectionsIntensityMultiplier.Evaluate(offsetDayTime);
    }
}
