using System;
using System.Collections;
using System.Collections.Generic;
using Custard;
using UnityEditorInternal;
using UnityEngine;

public class Monolyth : MonoBehaviour
{
    private UpgradeableStructure _upgrader;
    private Tidesmanager _tidesmanager;
    void Awake()
    {
        _upgrader = GetComponent<UpgradeableStructure>();
        _upgrader.OnLevelUp += ShrinkCustard;

        _tidesmanager = FindObjectOfType<Tidesmanager>();
    }

    private void ShrinkCustard(object sender, UpgradeableStructure.UpgradeArgs e)
    {
        var tidesmanagerIndexOfCurrentDayTimeTideLevel = _tidesmanager.indexOfCurrentDayTimeTideLevel - 1;
        if (tidesmanagerIndexOfCurrentDayTimeTideLevel < 0)
        {
            tidesmanagerIndexOfCurrentDayTimeTideLevel = _tidesmanager.CurrentMaxDayTimeIndex();
        }
        if (e.maxedOut)
        {
            _tidesmanager.OverrideTideStep(new Tidesmanager.TideStep(1),tidesmanagerIndexOfCurrentDayTimeTideLevel, Reset);
        }
        else
        {
            _tidesmanager.OverrideTideStep(new Tidesmanager.TideStep(3),tidesmanagerIndexOfCurrentDayTimeTideLevel);
        }
    }

    public void Reset()
    {
        _upgrader.SetCurrentLevelSilently(2);
    }
}
