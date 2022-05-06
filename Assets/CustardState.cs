using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class CustardState : MonoBehaviour
{
    private const int BLOCKS_WIDTH = 128;
    private const int BLOCKS_HEIGHT = 128;

    public GameObject CustardBlockPrefab;

    private byte[,] CustardArea;
    private GameObject[,] CustardCells;

    private List<byte[]> CellsThatMightCauseChange = new List<byte[]>();

    void Start()
    {
        CustardArea = new byte[BLOCKS_WIDTH, BLOCKS_HEIGHT];
        CustardCells = new GameObject[BLOCKS_WIDTH, BLOCKS_HEIGHT];

        InitCustardBlocks();
    }

    private void InitCustardBlocks()
    {
        for (int i = 0; i < BLOCKS_WIDTH; i++)
        {
            for (int j = 0; j < BLOCKS_HEIGHT; j++)
            {
                var custardPosition = GetCustardPosition(i, j);
                var custardCell = Instantiate(CustardBlockPrefab,
                    new Vector3(custardPosition.x, 1.5f, custardPosition.y),
                    Quaternion.identity);
                CustardCells[i, j] =
                    custardCell;
                if (IsInitialWorldCustard(i, j))
                {
                    custardCell.GetComponent<CustardBlock>().Show();
                }
            }
        }
    }

    private bool IsInitialWorldCustard(int x, int y)
    {
        // all edges
        return x == 0 || y == 0 || x == BLOCKS_WIDTH - 1 || y == BLOCKS_HEIGHT - 1;
    }

    private Vector2 GetCustardPosition(int x, int y)
    {
        return new Vector2(x - BLOCKS_WIDTH / 2, y - BLOCKS_HEIGHT / 2);
    }

    void Update()
    {
    }
}