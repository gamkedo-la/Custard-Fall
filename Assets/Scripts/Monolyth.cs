using System.Collections;
using System.Collections.Generic;
using Custard;
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
        if (e.maxedOut)
        {
            _tidesmanager.OverrideTideStep(new Tidesmanager.TideStep(1),_tidesmanager.indexOfCurrentDayTimeTideLevel - 1);
        }
        else
        {
            _tidesmanager.OverrideTideStep(new Tidesmanager.TideStep(3),_tidesmanager.indexOfCurrentDayTimeTideLevel - 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
