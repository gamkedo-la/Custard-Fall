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

    private readonly List<CellValue> _terrainList = new List<CellValue>(BlocksWidth * BlocksHeight);
    private readonly Dictionary<Coords, int> _worldItems = new Dictionary<Coords, int>();
    private bool _updateDebugVisualization = false;
    public bool isDebugMode = false;
    
    public event Action<Coords> onItemHeightChanged;

    public void Init()
    {
        Debug.Log("Initiating world cells");
        _heightMap ??= new int[BlocksWidth, BlocksHeight];
    }

    public int GetHeightAt(int x, int y)
    {
        return GetHeightAt(Coords.Of(x, y));
    }
    
    public int GetHeightAt(Coords coords)
    {
        if (coords.X is < 0 or >= BlocksWidth || coords.Y is < 0 or >= BlocksHeight)
            return 255;
        return _heightMap[coords.X, coords.Y] +  _worldItems.GetValueOrDefault(coords,0);
    }
    
    public int GetTerrainHeightAt(int x, int y)
    {
        return GetTerrainHeightAt(Coords.Of(x, y));
    }
    
    public int GetTerrainHeightAt(Coords coords)
    {
        if (coords.X is < 0 or >= BlocksWidth || coords.Y is < 0 or >= BlocksHeight)
            return 255;
        return _heightMap[coords.X, coords.Y];
    }
    
    public int GetWorldItemHeightAt(int x, int y)
    {
        return GetWorldItemHeightAt(Coords.Of(x, y));
    }
    
    public int GetWorldItemHeightAt(Coords coords)
    {
        if (coords.X is < 0 or >= BlocksWidth || coords.Y is < 0 or >= BlocksHeight)
            return 255;
        return _worldItems.GetValueOrDefault(coords, 0);
    }

    public void WriteHeightAt(Coords coords, int value)
    {
        _heightMap[coords.X, coords.Y] = value;
        _updateDebugVisualization = isDebugMode;
    }

    public void WriteWorldItemHeight(Coords coords, int value)
    {
        if (value > 0)
        {
            _worldItems.TryAdd(coords, value);
            _worldItems[coords] = value;
        }
        else
        {
            _worldItems.Remove(coords);
        }
        onItemHeightChanged?.Invoke(coords);
        _updateDebugVisualization = isDebugMode;
    }

    public List<CellValue> GetTerrainList()
    {
        if (_updateDebugVisualization)
        {
            _terrainList.Clear();

            for (int x = 0; x < BlocksWidth; x++)
            for (int y = 0; y < BlocksHeight; y++)
                _terrainList.Add(CellValue.Of(x, y, GetHeightAt(x,y)));
            _updateDebugVisualization = false;
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

    public void ResetChanges()
    {
        _worldItems.Clear();
    }
}