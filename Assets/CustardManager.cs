using System;
using System.Collections.Generic;
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

        InitCustardBlocks();
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
                _cellsThatMightCauseChange.Add(new Coords(i, j));
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
        return x == 0 || y == 0 || x == BLOCKS_WIDTH - 1 || y == BLOCKS_HEIGHT - 1;
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
                // TODO copy only cells that change... do we actually need a custard buffer in that sense?
                CopyFromInto(_custardAreaBuffer, _custardArea);
                _cellsThatMightCauseChange.AddRange(_cellsThatMightCauseChangeNextIteration);

                // reset the countdown
                _custardUpdateCountdown = custardCrawlDuration;

                RenderChangedCustard();
                
                // cleanup
                _cellsThatMightCauseChangeNextIteration.Clear();
                _cellsThatChange.Clear();
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
            if (_custardArea[cell.X, cell.Y] > 0)
            {
                _custardBlocks[cell.X, cell.Y].Show();
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
        Queue<Coords> tmpCellsQueue = new Queue<Coords>(cellsOfInterest);
        while (tmpCellsQueue.Count != 0)
        {
            var coords = tmpCellsQueue.Dequeue();

            byte[,] custardAreaOfEffect = GetLocalNeighborhood(coords, custardArea);
            byte[,] heightAreaOfEffect = GetLocalNeighborhood(coords, heightMap);

            ChangeCellState(coords, custardAreaOfEffect, heightAreaOfEffect, custardAreaBuffer,
                cellsThatMightCauseChangeNextIteration, cellsThatChange);
        }

        cellsOfInterest.Clear();
    }

    private void ChangeCellState(Coords coords, byte[,] custardAreaOfEffect, byte[,] heightAreaOfEffect,
        byte[,] custardAreaBuffer,
        HashSet<Coords> cellsThatMightCauseChangeNextIteration, HashSet<Coords> cellsThatChange)
    {
        byte currentHeight = custardAreaOfEffect[1, 1];
        byte nextHigherNeighbor = 0;
        byte nextLowerNeighbor = 0;

        for (byte x = 0; x < 3; x++)
        {
            for (byte y = 0; y < 3; y++)
            {
                int neighborHeight = custardAreaOfEffect[x, y] - heightAreaOfEffect[x, y];

                if (neighborHeight > currentHeight)
                {
                    if (coords.X > 0 && coords.X < BLOCKS_WIDTH - 1 && coords.Y > 0 && coords.Y < BLOCKS_HEIGHT - 1)
                    {
                        cellsThatMightCauseChangeNextIteration.Add(coords.Add(x - 1, y - 1));
                        if ((nextHigherNeighbor == 0 || nextHigherNeighbor > neighborHeight))
                        {
                            nextHigherNeighbor = (byte) neighborHeight;
                            cellsThatMightCauseChangeNextIteration.Add(coords);
                        }
                    }
                }

                if (neighborHeight < currentHeight)
                {
                    if (coords.X > 0 && coords.X < BLOCKS_WIDTH - 1 && coords.Y > 0 && coords.Y < BLOCKS_HEIGHT - 1)
                    {
                        cellsThatMightCauseChangeNextIteration.Add(coords.Add(x - 1, y - 1));
                        if ((nextLowerNeighbor == 0 || nextLowerNeighbor < neighborHeight))
                        {
                            nextLowerNeighbor = (byte) neighborHeight;
                        }
                    }
                }
            }
        }

        var update = nextHigherNeighbor == 0 ? currentHeight : nextHigherNeighbor;
        custardAreaBuffer[coords.X, coords.Y] = update;
        if (nextHigherNeighbor > currentHeight)
        {
            cellsThatMightCauseChangeNextIteration.Add(coords);
            cellsThatChange.Add(coords);
        }
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