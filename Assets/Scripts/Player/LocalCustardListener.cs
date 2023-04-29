using System;
using Custard;
using UnityEngine;

public class LocalCustardListener : MonoBehaviour
{
    public WorldCells worldCells;
    public CustardState custardState;
    public int HeightTillCovered;
    private Player player;

    public bool InsideCustard;
    public bool CoveredByCustard;

    public float graceTimeForBeingInsideOrOutsideCustard = .6f;

    public float timeBeforePlayerTakesDamageFromDrawning;
    public float timeBetweenDamageTicks;
    public int drowningDamage;
    private float timePassedSinceCoveredByCustard = 0.0f;
    private float timePassedSinceInsideCustardChange = 0.0f;
    private float timePassedSinceLastDamage = 0.0f;

    private void Awake()
    {
        player = gameObject.GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        var transformPosition = transform.position;
        Coords cellPosition = worldCells.GetCellPosition(transformPosition.x, transformPosition.z);
        var custardAmount = custardState.GetCurrentCustardLevelAt(cellPosition);

        var insideCustard = custardAmount > 0;
        var coveredByCustard = custardAmount >= HeightTillCovered;

        if (CoveredByCustard != coveredByCustard)
        {
            OnCoveredByCustard(coveredByCustard);
        }

        if (InsideCustard != insideCustard)
        {
            timePassedSinceInsideCustardChange = 0f;
        }

        if (CoveredByCustard || timePassedSinceInsideCustardChange >= graceTimeForBeingInsideOrOutsideCustard)
        {
            OnInsideCustard(insideCustard);
        }

        InsideCustard = insideCustard;
        CoveredByCustard = coveredByCustard;

        CheckTimeSpentInOrOutsideCustard();
        CheckForDrownDamage();
    }

    private void CheckTimeSpentInOrOutsideCustard()
    {
        // we are only interested in calculating the grace time period
        if (timePassedSinceInsideCustardChange <= 2f)
        {
            timePassedSinceInsideCustardChange += Time.deltaTime;
        }
    }

    private void OnCoveredByCustard(bool coveredByCustard)
    {
        timePassedSinceCoveredByCustard = 0.0f;
        timePassedSinceLastDamage = 0.0f;
        player.EnterSwimMode(coveredByCustard);
    }

    private void OnInsideCustard(bool insideCustard)
    {
        MusicManager.Instance.SetUnder(insideCustard);
    }

    private void CheckForDrownDamage()
    {
        if (CoveredByCustard)
        {
            if (timePassedSinceCoveredByCustard < timeBeforePlayerTakesDamageFromDrawning)
            {
                timePassedSinceCoveredByCustard += Time.deltaTime;
            }
            else
            {
                if (timePassedSinceLastDamage < timeBetweenDamageTicks)
                {
                    timePassedSinceLastDamage += Time.deltaTime;
                }
                else if(player)
                {
                    player.TakeDamage(drowningDamage);
                    timePassedSinceLastDamage = 0.0f;
                }
            }
        }
    }
}