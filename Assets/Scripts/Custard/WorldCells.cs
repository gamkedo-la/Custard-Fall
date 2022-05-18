using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldCells", menuName = "CustardFall/WorldCells", order = 0)]
public class WorldCells : ScriptableObject
{
    // using byte to make sure network transfer is always minimal, for performance reasons this also means max value is 255
    public const byte BlocksWidth = 128;
    public const byte BlocksHeight = 128;

    private byte[,] _heightMap;

    public void Init()
    {
        Debug.Log("Initiating world cells");
        _heightMap = new byte[BlocksWidth, BlocksHeight];
        
        LoadTerrainMap();
    }

    public byte GetHeightAt(int x, int y)
    {
        return _heightMap[x, y];
    }
    
    private void LoadTerrainMap()
    {
        for (byte i = 0; i < BlocksWidth; i++)
        for (byte j = 0; j < BlocksHeight; j++)
            if (i is > 30 and < 40 && j is > 30 and < 40)
                _heightMap[i, j] = 1;
    }
    
    public void CopyFromInto(byte[,] fromArea, byte[,] toArea)
    {
        for (var i = 0; i < BlocksWidth; i++)
        for (var j = 0; j < BlocksHeight; j++)
            toArea[i, j] = fromArea[i, j];
    }
}