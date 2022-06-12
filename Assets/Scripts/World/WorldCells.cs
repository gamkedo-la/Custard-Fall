using System;
using System.Collections.Generic;
using Custard;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldCells", menuName = "CustardFall/WorldCells", order = 0)]
public class WorldCells : ScriptableObject
{
    // using int to make sure network transfer is always minimal, for performance reasons this also means max value is 255
    public const int BlocksWidth = 128;
    public const int BlocksHeight = 128;

    private int[,] _heightMap;

    private List<CellValue> _terrainList = new List<CellValue>(BlocksWidth * BlocksHeight);
    private bool updateDebugVisualization = false;

    public void Init()
    {
        Debug.Log("Initiating world cells");
        _heightMap ??= new int[BlocksWidth, BlocksHeight];
    }

    public int GetHeightAt(int x, int y)
    {
        return _heightMap[x, y];
    }

    public int GetHeightAt(Coords coords)
    {
        if (coords.X is < 0 or >= BlocksWidth || coords.Y is < 0 or >= BlocksHeight)
            return 255;
        return _heightMap[coords.X, coords.Y];
    }

    public void WriteHeightAt(Coords coords, int value)
    {
        _heightMap[coords.X, coords.Y] = value;
        updateDebugVisualization = true;
    }

    public List<CellValue> GetTerrainList()
    {
        if (updateDebugVisualization)
        {
            _terrainList.Clear();

            for (int x = 0; x < BlocksWidth; x++)
            for (int y = 0; y < BlocksHeight; y++)
                _terrainList.Add(CellValue.Of(x, y, _heightMap[x, y]));
            updateDebugVisualization = false;
        }

        return _terrainList;
    }

    public Coords GetCellPosition(double x, double y)
    {
        var cellX = Math.Floor(x) + WorldCells.BlocksWidth / 2f;
        var cellY = Math.Floor(y) + WorldCells.BlocksHeight / 2f;
        return Coords.Of( (int)cellX,  (int)cellY);
    }
    
    public static Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(x - WorldCells.BlocksWidth / 2f + .5f, y - WorldCells.BlocksHeight / 2f + .5f);
    }

    public Vector2 GetWorldPosition(Coords coords)
    {
        return GetWorldPosition(coords.X, coords.Y);
    }
}