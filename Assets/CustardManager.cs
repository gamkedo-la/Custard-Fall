using System;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;


public class CustardManager : MonoBehaviour
{
    // using byte to make sure network transfer is always minimal, for performance reasons this also means max value is 255
    private const byte BLOCKS_WIDTH = 128;
    private const byte BLOCKS_HEIGHT = 128;

    public GameObject custardBlockPrefab;
    private GameObject _custardBlocksParent;

    public float custardCrawlDuration = .5f;
    private float _custardUpdateCountdown;

    private CustardBlock[,] _custardBlocks;
    private byte[,] _custardArea;
    private byte[,] _custardAreaBuffer;
    private byte[,] _heightMap;

    private readonly HashSet<Coords> _cellsThatMightCauseChange = new();
    private readonly HashSet<Coords> _cellsThatMightCauseChangeNextIteration = new();
    private readonly HashSet<Coords> _cellsThatChange = new();


    private void Start()
    {
        _custardBlocksParent = GameObject.Find("Custard");
        _custardArea = new byte[BLOCKS_WIDTH, BLOCKS_HEIGHT];
        _heightMap = new byte[BLOCKS_WIDTH, BLOCKS_HEIGHT];
        _custardAreaBuffer = new byte[BLOCKS_WIDTH, BLOCKS_HEIGHT];
        _custardBlocks = new CustardBlock[BLOCKS_WIDTH, BLOCKS_HEIGHT];
        _custardUpdateCountdown = custardCrawlDuration;

        InitLevel();
        InitCustardBlocks();
    }

    private void InitLevel()
    {
        for (byte i = 0; i < BLOCKS_WIDTH; i++)
        for (byte j = 0; j < BLOCKS_HEIGHT; j++)
        {
            if (i is > 30 and < 40 && j is > 30 and < 40)
                _heightMap[i, j] = 1;
        }

        CopyFromInto(_custardArea, _custardAreaBuffer);
    }
    
    private void InitCustardBlocks()
    {
        for (byte i = 0; i < BLOCKS_WIDTH; i++)
        for (byte j = 0; j < BLOCKS_HEIGHT; j++)
        {
            var custardPosition = GetCustardPosition(i, j);
            var custardCell = Instantiate(custardBlockPrefab,
                new Vector3(custardPosition.x, 1.5f, custardPosition.y),
                Quaternion.identity, _custardBlocksParent.transform);
            _custardBlocks[i, j] = custardCell.GetComponent<CustardBlock>();
            if (IsInitialWorldCustard(i, j))
            {
                custardCell.GetComponent<CustardBlock>().Show();
                _custardArea[i, j] = 1;
                _cellsThatMightCauseChange.Add(Coords.Of(i, j));
            }
        }

        CopyFromInto(_custardArea, _custardAreaBuffer);
    }

    private void CopyFromInto(byte[,] fromArea, byte[,] toArea)
    {
        for (int i = 0; i < BLOCKS_WIDTH; i++)
        {
            for (int j = 0; j < BLOCKS_HEIGHT; j++)
            {
                toArea[i, j] = fromArea[i, j];
            }
        }
    }

    private bool IsInitialWorldCustard(byte x, byte y)
    {
        // all edges
        return x == 0 || y == 0 || x == BLOCKS_WIDTH - 1 || y == BLOCKS_HEIGHT - 1 || x == 55 && y == 55 ||
               x is > 90 and < 93 && y is > 90 and < 93;
    }

    private static Vector2 GetCustardPosition(byte x, byte y)
    {
        return new Vector2(x - BLOCKS_WIDTH / 2, y - BLOCKS_HEIGHT / 2);
    }

    private void FixedUpdate()
    {
        _custardUpdateCountdown -= Time.deltaTime;
        if (_cellsThatMightCauseChange.Count == 0)
        {
            if (_custardUpdateCountdown <= 0.001f)
            {
                // reset the countdown
                _custardUpdateCountdown = custardCrawlDuration;

                // if (_cellsThatChange.Count != 0)
                // {
                // TODO copy only cells that change... do we actually need a custard buffer in that sense?
                CopyFromInto(_custardAreaBuffer, _custardArea);
                _cellsThatMightCauseChange.AddRange(_cellsThatMightCauseChangeNextIteration);

                RenderChangedCustard();

                // cleanup
                _cellsThatMightCauseChangeNextIteration.Clear();
                _cellsThatChange.Clear();
                // }
            }
        }
        else
        {
            SimulateCustard(_cellsThatMightCauseChange, _custardArea, _heightMap, _custardAreaBuffer,
                _cellsThatMightCauseChangeNextIteration, _cellsThatChange);
        }
    }

    private void RenderChangedCustard()
    {
        foreach (var cell in _cellsThatChange)
        {
            var custardAmount = _custardArea[cell.X, cell.Y];
            if (custardAmount > 0)
            {
                CustardBlock custardBlock = _custardBlocks[cell.X, cell.Y];
                var blockGameObject = custardBlock.gameObject;
                var scale = blockGameObject.transform.localScale;
                blockGameObject.transform.localScale = new Vector3(scale.x, custardAmount, scale.z);
                custardBlock.Show();
            }
            else
            {
                _custardBlocks[cell.X, cell.Y].Hide();
            }
        }
    }

    private void SimulateCustard(HashSet<Coords> cellsOfInterest, byte[,] custardArea, byte[,] heightMap,
        byte[,] custardAreaBuffer, HashSet<Coords> cellsThatMightCauseChangeNextIteration,
        HashSet<Coords> cellsThatChange)
    {
        foreach (Coords coords in cellsOfInterest)
        {
            byte[,] custardAreaOfEffect = GetLocalNeighborhood(coords, custardArea);
            byte[,] heightAreaOfEffect = GetLocalNeighborhood(coords, heightMap);

            ChangeCellState(coords, custardAreaOfEffect, heightAreaOfEffect, custardAreaBuffer,
                cellsThatMightCauseChangeNextIteration, cellsThatChange);
        }

        cellsOfInterest.Clear();
    }

    private void ChangeCellState(Coords coords, byte[,] custardHeightMap, byte[,] localHeightMap,
        byte[,] custardAreaBuffer,
        HashSet<Coords> cellsThatMightChangeNextIteration, HashSet<Coords> cellsThatChange)
    {
        if (coords.X is < 0 or > BLOCKS_WIDTH - 1 && coords.Y is < 0 or > BLOCKS_HEIGHT - 1)
        {
            // out of bounds of world area
            return;
        }

        // note: for now let us ignore the case of a corner-on-corner block setup, where the neighboring custard is only adjacent through the corner edge
        int pivotCustardHeight = custardHeightMap[1, 1];
        int pivotHeight = pivotCustardHeight + localHeightMap[1, 1];
        int biggestCustardStackAbove = 0;
        int stackRequiredToStayAtLevel = 0;

        for (byte x = 0; x < 3; x++)
        {
            for (byte y = 0; y < 3; y++)
            {
                if (coords.X == 0 && x == 0)
                {
                    continue;
                }
                else if (coords.Y == 0 && y == 0)
                {
                    continue;
                }
                else if (coords.X == BLOCKS_WIDTH - 1 && x == 2)
                {
                    continue;
                }
                else if (coords.Y == BLOCKS_HEIGHT - 1 && y == 2)
                {
                    continue;
                }

                // the custard flows downwards, so we find out how much custard may be coming down
                int neighborCustardHeight = custardHeightMap[x, y];
                int neighborHeight = neighborCustardHeight + localHeightMap[x, y];

                if (neighborHeight > pivotHeight)
                {
                    int flowyCustardStack = Math.Min(neighborCustardHeight, neighborHeight - pivotHeight);
                    if (flowyCustardStack > 0)
                    {
                        if (biggestCustardStackAbove < flowyCustardStack)
                        {
                            biggestCustardStackAbove = flowyCustardStack;
                            if (biggestCustardStackAbove > pivotCustardHeight)
                                cellsThatMightChangeNextIteration.Add(coords);
                        }
                    }
                }
                else if (neighborHeight == pivotHeight)
                {
                    stackRequiredToStayAtLevel = pivotCustardHeight;
                }
                else
                {
                    // or how much of the current stack needs to disappear to stay at level with neighboring custard
                    // or be removed entirely
                    var heightDifference = pivotHeight - neighborHeight;
                    if (neighborCustardHeight == 0)
                    {
                        if (stackRequiredToStayAtLevel < pivotCustardHeight)
                            stackRequiredToStayAtLevel = Math.Max(1, pivotCustardHeight - 1);
                    }
                    else
                    {
                        var tmpHeight = pivotCustardHeight > heightDifference ? heightDifference : pivotCustardHeight;
                        if (stackRequiredToStayAtLevel < tmpHeight)
                        {
                            stackRequiredToStayAtLevel = tmpHeight;
                        }
                    }

                    if (heightDifference != 0)
                        CellMightChangeNextIteration(coords, cellsThatMightChangeNextIteration, x - 1, y - 1);
                }
            }
        }

        var update = biggestCustardStackAbove > stackRequiredToStayAtLevel
            ? pivotCustardHeight + 1
            : stackRequiredToStayAtLevel;
        if (update is < 0 or > 255)
        {
            // clamp to safest bet
            update = update < 0 ? 0 : pivotCustardHeight;
        }

        custardAreaBuffer[coords.X, coords.Y] = (byte) update;
        if (update != pivotCustardHeight)
        {
            cellsThatChange.Add(coords);
            // always consider recently changed blocks to 
            cellsThatMightChangeNextIteration.Add(coords);
        }
    }

    private static void CellMightChangeNextIteration(Coords coords, HashSet<Coords> cellsThatMightChangeNextIteration,
        int x,
        int y)
    {
        if (coords.X == 0 && x == -1 || coords.X == BLOCKS_WIDTH - 1 && x == 1 || coords.Y == 0 && y == -1 ||
            coords.Y == BLOCKS_HEIGHT - 1 && y == 1)
            return;
        cellsThatMightChangeNextIteration.Add(coords.Add(x, y));
    }

    private static byte[,] GetLocalNeighborhood(Coords coords, byte[,] map)
    {
        byte[,] localNeighborhood = new byte[3, 3];

        bool leftClamp = coords.X - 1 < 0;
        bool topClamp = coords.Y - 1 < 0;
        bool rightClamp = coords.X + 1 >= BLOCKS_WIDTH;
        bool bottomClamp = coords.Y + 1 >= BLOCKS_HEIGHT;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if ((i == 0 && leftClamp) || (i == 2 && rightClamp) || (j == 0 && topClamp) || (j == 2 && bottomClamp))
                    localNeighborhood[i, j] = 0;
                else
                    localNeighborhood[i, j] = map[coords.X + i - 1, coords.Y + j - 1];
            }
        }

        return localNeighborhood;
    }
}