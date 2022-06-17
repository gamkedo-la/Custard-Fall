using System;
using System.Collections;
using System.Collections.Generic;
using Custard;
using UnityEngine;
using Random = UnityEngine.Random;

public class VisualDebugger : MonoBehaviour
{
    public CustardState custardState;
    public WorldCells worldCells;
    public Player player;

    private Inhaler _inhaler;
    
    

    public bool showCustardDebugInfo = false;
    public bool showQueue = false;
    public bool showNextQueue = false;
    public bool previewScheduledUpdate = false;
    public bool showLastUpdate = false;
    public bool showTerrainInfo;
    public bool displayOnWorldCells = true;
    public bool showInhaleArea;

    private void Start()
    {
        _inhaler = player.GetComponent<Inhaler>();
    }

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
        }
        if (showNextQueue)
        {
            Visualize(custardState.CellsThatMightCauseChangeNextIteration, Color.black);
        }
        if (showTerrainInfo)
        {
            Visualize(worldCells.GetTerrainList());
        }
        if (showInhaleArea)
        {
            List<Coords> positions = new List<Coords>();
            foreach (var coneCell in _inhaler.affectedCells)
            {
                positions.Add(coneCell.Value.GetCoords());
            }
            Visualize(positions,Color.blue);
        }
        
    }
    
    public void Visualize(IEnumerable<CellValue> cellValues)
    {
        foreach (var coord in cellValues)
        {
            Visualize(coord.Coords, coord.Value, Color.Lerp(Color.black, Color.magenta, coord.Value / 9f));
        }
    }

    public void Visualize(IEnumerable<CellValue> cellValues, Color color)
    {
        foreach (var coord in cellValues)
        {
            Visualize(coord.Coords, coord.Value, color);
        }
    }

    public void Visualize(IEnumerable<Coords> coords)
    {
        Visualize(coords, Random.ColorHSV());
    }

    public void Visualize(IEnumerable<Coords> coords, Color color)
    {
        foreach (var coord in coords)
        {
            Visualize(coord, 1, color);
        }
    }
    
    public void Visualize(HashSet<CellValue> cellUpdates)
    {
        foreach (var cellUpdate in cellUpdates)
        {
            Visualize(cellUpdate.Coords, cellUpdate.Value);
        }
    }

    public void Visualize(Coords coords, int level)
    {
        Visualize(coords, level, Color.Lerp(Color.blue, Color.red, 1f/(7f - level)));
    }

    public void Visualize(Coords coords, int level, Color color)
    {
        var custardPosition = worldCells.GetWorldPosition(coords);
        var from = new Vector3(custardPosition.x, (displayOnWorldCells? worldCells.GetHeightAt(coords):0) + 1f, custardPosition.y);
        var to = new Vector3(custardPosition.x, (displayOnWorldCells? worldCells.GetHeightAt(coords):0) + 4f, custardPosition.y);
        Debug.DrawLine(from , to, color, Time.deltaTime);
    }
}
