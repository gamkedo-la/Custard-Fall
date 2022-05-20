using System;
using System.Collections;
using System.Collections.Generic;
using Custard;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustardDebugger : MonoBehaviour
{
    public CustardState custardState;
    public WorldCells worldCells;

    
    public bool showCustardDebugInfo = false;
    public bool showQueue = false;
    public bool previewScheduledUpdate = false;
    public bool showLastUpdate = false;
    public bool displayOnWorldCells = true;
    
    void FixedUpdate()
    {
        if (showLastUpdate)
        {
            Visualize(custardState.BufferFromPreviousIteration);
        }
        if (previewScheduledUpdate)
        {
            Visualize(custardState.Buffer);
        }
        if (showQueue)
        {
            Visualize(custardState.CellsToProcessInCurrentIteration, Color.cyan);
            Visualize(custardState.CellsThatMightCauseChangeNextIteration, Color.yellow);
        }
        
    }
    
    private void Visualize(HashSet<Coords> coords)
    {
        Visualize(coords, Random.ColorHSV());
    }

    private void Visualize(HashSet<Coords> coords, Color color)
    {
        foreach (var coord in coords)
        {
            Visualize(coord, 1, color);
        }
    }
    
    private void Visualize(HashSet<CellUpdate> cellUpdates)
    {
        foreach (var cellUpdate in cellUpdates)
        {
            Visualize(cellUpdate.Coords, cellUpdate.AbsoluteCustardLevel);
        }
    }

    private void Visualize(Coords coords, byte level)
    {
        Visualize(coords, level, Color.Lerp(Color.blue, Color.red, 1f/(7f - level)));
    }

    private void Visualize(Coords coords, byte level, Color color)
    {
        var custardPosition = CustardManager.GetCustardPosition(coords);
        var from = new Vector3(custardPosition.x, (displayOnWorldCells? worldCells.GetHeightAt(coords):0) + 1f, custardPosition.y);
        var to = new Vector3(custardPosition.x, (displayOnWorldCells? worldCells.GetHeightAt(coords):0) + 4f, custardPosition.y);
        Debug.DrawLine(from , to, color);
    }
}
