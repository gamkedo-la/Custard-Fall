using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{

    private TimeManager _timeManager;
    [SerializeField] private Image dayFill;
    [SerializeField] private Image nightFill;
    [SerializeField] private Image custardRiseFill;
    [SerializeField] private GameObject arrow;
    
    void Start()
    {
        _timeManager = TimeManager.Instance;
        dayFill.fillAmount = _timeManager.nightStart - _timeManager.dayStart;
        nightFill.fillAmount = 1f - _timeManager.nightStart + _timeManager.dayStart;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateClockTime(_timeManager.time);
    }

    private void UpdateClockTime(float time)
    {
        // Rotate the cube by converting the angles into a quaternion.
        Quaternion target = Quaternion.Euler(0, 0, -time  * 360);

        // Dampen towards the target rotation
        var arrowTransform = arrow.transform;
        arrowTransform.rotation = Quaternion.Slerp(arrowTransform.rotation, target,  Time.deltaTime * 5);
    }
}
