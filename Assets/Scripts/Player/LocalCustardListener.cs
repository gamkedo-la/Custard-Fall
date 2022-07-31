using System;
using Custard;
using UnityEngine;

public class LocalCustardListener: MonoBehaviour
{
    public WorldCells worldCells;
    public CustardState custardState;
    public int HeightTillCovered;

    public bool InsideCustard;
    public bool CoveredByCustard;

    public float timeBeforePlayerTakesDamageFromDrawning;
    public float timeBetweenDamageTicks;
    public int drowningDamage;
    private float timePassedSinceCoveredByCustrad = 0.0f;
    private float timePassedSinceLastDamage = 0.0f;

    private void Update()
    {
        var transformPosition = transform.position;
        Coords cellPosition = worldCells.GetCellPosition(transformPosition.x, transformPosition.z);
        var custardAmount = custardState.GetCurrentCustardLevelAt(cellPosition);

        InsideCustard = custardAmount > 0;
        var coveredByCustard = custardAmount >= HeightTillCovered;

        if (CoveredByCustard != coveredByCustard)
        {
            OnCoveredbyCustard(coveredByCustard);
            timePassedSinceCoveredByCustrad = 0.0f;
            timePassedSinceLastDamage = 0.0f;
        }
        
        CoveredByCustard = coveredByCustard;

        CheckForDrownDamage();
    }

    private void OnCoveredbyCustard(bool coveredByCustard)
    {
        MusicManager.Instance.SetUnder(coveredByCustard);
    }

    private void CheckForDrownDamage()
    {
        if (CoveredByCustard)
        {
            if (timePassedSinceCoveredByCustrad < timeBeforePlayerTakesDamageFromDrawning)
            {
                timePassedSinceCoveredByCustrad += Time.deltaTime;
            }
            else
            {
                if (timePassedSinceLastDamage < timeBetweenDamageTicks)
                {
                    timePassedSinceLastDamage += Time.deltaTime;
                }
                else
                {
                    gameObject.GetComponent<Player>().TakeDamage(drowningDamage);
                    timePassedSinceLastDamage = 0.0f;
                }
            }
        }
    }

}