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
        }
        
        CoveredByCustard = coveredByCustard;
        
        
    }

    private void OnCoveredbyCustard(bool coveredByCustard)
    {
        MusicManager.Instance.SetUnder(coveredByCustard);
    }
    
}