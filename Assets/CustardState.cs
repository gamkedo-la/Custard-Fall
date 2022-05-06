using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class CustardState : MonoBehaviour
{
    // using byte to make sure network transfer is always minimal, for performance reasons this also means max value is 255
    private const byte BLOCKS_WIDTH = 128;
    private const byte BLOCKS_HEIGHT = 128;

    public GameObject custardBlockPrefab;

    private GameObject _custardBlocksParent;
    private byte[,] _custardArea;
    private GameObject[,] _custardCells;

    private List<byte[]> _cellsThatMightCauseChange = new();

    private void Start()
    {
        _custardBlocksParent =  GameObject.Find("Custard");
        _custardArea = new byte[BLOCKS_WIDTH, BLOCKS_HEIGHT];
        _custardCells = new GameObject[BLOCKS_WIDTH, BLOCKS_HEIGHT];

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
            _custardCells[i, j] = custardCell;
            if (IsInitialWorldCustard(i, j))
            {
                custardCell.GetComponent<CustardBlock>().Show();
                _cellsThatMightCauseChange.Add(new byte[]{i,j});
            }
        }
    }

    private bool IsInitialWorldCustard(byte x, byte y)
    {
        // all edges
        return x == 0 || y == 0 || x == BLOCKS_WIDTH - 1 || y == BLOCKS_HEIGHT - 1;
    }

    private Vector2 GetCustardPosition(byte x, byte y)
    {
        return new Vector2(x - BLOCKS_WIDTH / 2, y - BLOCKS_HEIGHT / 2);
    }

    private void FixedUpdate()
    {
        
    }
}