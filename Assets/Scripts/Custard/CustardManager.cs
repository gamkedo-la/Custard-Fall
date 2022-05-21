﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Custard;
using Unity.VisualScripting;
using UnityEngine;

namespace Custard
{
    public class CustardManager : MonoBehaviour
    {
        public WorldCells worldCells;
        public CustardState custardState;

        public CustardVisualizer custardVisualizer;
        public GameObject custardBlockPrefab;

        public bool pauseIterationCountDown;


        public float custardCrawlDuration = .4f;
        public byte initialCustardLevel = 2;
        private float _custardUpdateCountdown;


        private void Start()
        {
            _custardUpdateCountdown = 0;
            custardState.Init();
            custardState.GlobalTideLevel = initialCustardLevel;
            worldCells.Init();
            InitCustardState();
        }

        private void InitCustardState()
        {
            // iteration 0 for custardState
            for (byte x = 0; x < WorldCells.BlocksWidth; x++)
            for (byte y = 0; y < WorldCells.BlocksHeight; y++)
            {
                if (IsInitialWorldCustard(x, y))
                {
                    custardState.RegisterUpdate(x, y, initialCustardLevel);
                    custardState.QueueCellForNextIteration(x, y);
                    // apply and proceed to next iteration
                    _custardUpdateCountdown = 0;
                }
            }
        }

        private bool IsInitialWorldCustard(byte x, byte y)
        {
            // TODO replace with algorithm based on levels of the worldCells
            return x == 0 || y == 0 || x == WorldCells.BlocksWidth - 1 || y == WorldCells.BlocksHeight - 1 ||
                   (x == 55 && y == 55) ||
                   (x is > 90 and < 93 && y is > 90 and < 93);
        }

        private void FixedUpdate()
        {
            if (!pauseIterationCountDown)
                _custardUpdateCountdown -= Time.deltaTime;

            if (custardState.CellsToProcessInCurrentIteration.Count == 0)
            {
                if (_custardUpdateCountdown <= 0.001f)
                {
                    // reset the countdown
                    _custardUpdateCountdown = custardCrawlDuration;

                    NextCustardIteration();
                }
            }
            else
            {
                // prepare next custard iteration
                SimulateCustard();
            }
        }

        private void NextCustardIteration()
        {
            // render all changes we got
            custardVisualizer.RenderChangedCustard();
            // proceed to next custard iteration
            custardState.ApplyAllUpdatesAndStartNewIteration();
        }

        public bool TogglePause()
        {
            pauseIterationCountDown = !pauseIterationCountDown;
            return pauseIterationCountDown;
        }

        public void ForceNextIterationHalfStep()
        {
            if (pauseIterationCountDown)
                if (_custardUpdateCountdown == 0)
                {
                    _custardUpdateCountdown = custardCrawlDuration;
                }
                else
                {
                    _custardUpdateCountdown = 0;
                }
            else
                _custardUpdateCountdown = 0;
        }

        private void SimulateCustard()
        {
            // working on a shallow copy as other cells might find their way into current iteration
            var processing = new HashSet<Coords>(custardState.CellsToProcessInCurrentIteration);
            foreach (var pivot in processing)
            {
                var custardAreaAroundPivot =
                    GetLocalNeighborhood(pivot, (i, j) => custardState.GetCurrentCustardLevelAt(i, j));
                var terrainAreaAroundPivot = GetLocalNeighborhood(pivot, (i, j) => worldCells.GetHeightAt(i, j));

                UpdateCustardState(pivot, custardAreaAroundPivot, terrainAreaAroundPivot);
                custardState.MarkAsProcessed(pivot);
            }
        }


        private void UpdateCustardState(Coords pivot, byte[,] custardAreaAroundPivot, byte[,] terrainAreaAroundPivot)
        {
            if (pivot.X is < 0 or > WorldCells.BlocksWidth - 1 && pivot.Y is < 0 or > WorldCells.BlocksHeight - 1)
                // out of bounds of world area
                return;

            // note: for now let us ignore the case of a corner-on-corner block setup, where the neighboring custard is only adjacent through the corner edge
            int pivotCustardAmount = custardAreaAroundPivot[1, 1];
            var pivotTerrainHeight = terrainAreaAroundPivot[1, 1];
            var pivotTotalHeight = pivotCustardAmount + pivotTerrainHeight;

            // will be applied to the cell at the end
            // custard amount of 0 means no custard
            int newPivotCustardAmount = pivotCustardAmount;
            
            if (pivotTotalHeight > custardState.GlobalTideLevel)
            {
                // # custard is above tide level => we need to shrink, flow down or even dissolve if possible
                var info = RetrieveCustardInfo(pivot, custardAreaAroundPivot, terrainAreaAroundPivot);

                if (info.CustardFromAbove.Count != 0)
                {
                    // custard from above flows down, so we stay
                    newPivotCustardAmount = pivotCustardAmount;
                    // very likely level is not going to stay next iteration as we are still above the global tide level
                    custardState.QueueForNextIteration(pivot);
                    // cells above should flow into me
                    custardState.QueueCellsForNextIteration(info.CustardFromAbove);
                    // I stay at the same level but maybe my neighbors need to be checked
                    custardState.QueueCellsForNextIteration(info.CellsAtSameLevel);
                    if (pivotCustardAmount > 0)
                    {
                        // next iteration: check all cells where I might flow down into
                        custardState.QueueCellsForNextIteration(info.CellsBelow);
                    }
                }
                else if (info.CellsBelow.Count != 0)
                {
                    // we flow downwards
                    newPivotCustardAmount = Math.Max(pivotCustardAmount - 1, 0);
                    if (custardState.GlobalTideLevel < newPivotCustardAmount + pivotTerrainHeight)
                        custardState.QueueForNextIteration(pivot);
                    // next iteration: check all cells that might get affected by this change
                    custardState.QueueCellsForNextIteration(info.CellsAtSameLevel);
                    if (pivotCustardAmount > 0)
                    {
                        // next iteration: check all cells where I might flow down into
                        custardState.QueueCellsForNextIteration(info.CellsBelow);
                    }
                } else if(pivotCustardAmount > 1)
                {
                    // custard is trapped,
                    // so we simply shrink up to a single layer
                    newPivotCustardAmount = pivotCustardAmount - 1;
                    if (custardState.GlobalTideLevel < newPivotCustardAmount + pivotTerrainHeight)
                        custardState.QueueForNextIteration(pivot);
                    // next iteration: check all cells that now might flow into me
                    custardState.QueueCellsForNextIteration(info.CellsAtSameLevel);
                } else if (custardState.GlobalTideLevel == 0 && pivotTerrainHeight == 0)
                {
                    // custard is trapped at bottom most level
                    newPivotCustardAmount = pivotCustardAmount - 1;
                    // next iteration: check all cells that should also dissolve/shrink
                    custardState.QueueCellsForNextIteration(info.CellsAtSameLevel);
                } 
            }
            else if (pivotTotalHeight == custardState.GlobalTideLevel)
            {
                // # custard is at tide level => we should stay at this level
                var info = RetrieveCustardInfo(pivot, custardAreaAroundPivot, terrainAreaAroundPivot);
                
                // stay at same level
                newPivotCustardAmount = pivotCustardAmount;
                // cells above should flow into me
                custardState.QueueCellsForNextIteration(info.CustardFromAbove);
                // spread: next iteration: check all cells below me as they should come up
                custardState.QueueCellsForNextIteration(info.CellsBelow);
            }
            else
            {
                // # custard is below tide level => we should grow
                var info = RetrieveCustardInfo(pivot, custardAreaAroundPivot, terrainAreaAroundPivot);
                
                newPivotCustardAmount = pivotCustardAmount + 1;
                if (custardState.GlobalTideLevel > newPivotCustardAmount + pivotTerrainHeight)
                    custardState.QueueForNextIteration(pivot);
                // cells above should flow into me
                custardState.QueueCellsForNextIteration(info.CustardFromAbove);
                // spread: next iteration: check all cells below me or currently at same level should grow next as well
                custardState.QueueCellsForNextIteration(info.CellsAtSameLevel);
                custardState.QueueCellsForNextIteration(info.CellsBelow);
            }

            // prepare value for byte range
            if (newPivotCustardAmount < 0)
                newPivotCustardAmount = 0;
            else if(newPivotCustardAmount > 255)
            {
                // don't change then
                newPivotCustardAmount = pivotCustardAmount;
            }
            
            if (newPivotCustardAmount != pivotCustardAmount)
            {
                custardState.RegisterUpdate(pivot, (byte) newPivotCustardAmount);
            }
        }

        private CustardAreaInfo RetrieveCustardInfo(Coords pivot, byte[,] custardAreaAroundPivot,
            byte[,] terrainAreaAroundPivot)
        {
            List<Coords> custardCellsAbove = new List<Coords>();
            List<Coords> sameLevelCells = new List<Coords>();
            List<Coords> cellsBelow = new List<Coords>();
           

            int pivotCustardAmount = custardAreaAroundPivot[1, 1];
            var pivotTerrainHeight = terrainAreaAroundPivot[1, 1];
            var pivotTotalHeight = pivotCustardAmount + pivotTerrainHeight;
            
            for (byte x = 0; x < 3; x++)
            for (byte y = 0; y < 3; y++)
            {
                if (pivot.X == 0 && x == 0)
                    continue;
                else if (pivot.Y == 0 && y == 0)
                    continue;
                else if (pivot.X == WorldCells.BlocksWidth - 1 && x == 2)
                    continue;
                else if (pivot.Y == WorldCells.BlocksHeight - 1 && y == 2) continue;

                // convert relative offset to coord
                var coord = pivot.Add(x - 1, y - 1);
                int neighborCustardAmount = custardAreaAroundPivot[x, y];
                var neighborTotalHeight = neighborCustardAmount + terrainAreaAroundPivot[x, y];

                if (neighborTotalHeight > pivotTotalHeight)
                {
                    var flowyCustardStack = Math.Min(neighborCustardAmount, neighborTotalHeight - pivotTotalHeight);
                    if (flowyCustardStack > 0)
                        custardCellsAbove.Add(coord);
                }
                else if (neighborTotalHeight == pivotTotalHeight)
                {
                    sameLevelCells.Add(coord);
                }
                else
                {
                    // or how much of the current stack needs to disappear to stay at level with neighboring custard
                    // or be removed entirely
                    var heightDifference = pivotTotalHeight - neighborTotalHeight;
                    if (heightDifference != 0)
                        cellsBelow.Add(coord);
                }
            }

            return new CustardAreaInfo(custardCellsAbove, sameLevelCells, cellsBelow);
        }

        private byte[,] GetLocalNeighborhood(Coords coords, Func<int, int, byte> getValueAt)

        {
            var localNeighborhood = new byte[3, 3];

            var leftClamp = coords.X - 1 < 0;
            var topClamp = coords.Y - 1 < 0;
            var rightClamp = coords.X + 1 >= WorldCells.BlocksWidth;
            var bottomClamp = coords.Y + 1 >= WorldCells.BlocksHeight;
            for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                if ((i == 0 && leftClamp) || (i == 2 && rightClamp) || (j == 0 && topClamp) || (j == 2 && bottomClamp))
                    localNeighborhood[i, j] = 0;
                else
                    localNeighborhood[i, j] = getValueAt.Invoke(coords.X + i - 1, coords.Y + j - 1);

            return localNeighborhood;
        }

        public static Vector2 GetCustardPosition(byte x, byte y)
        {
            return new Vector2(x - WorldCells.BlocksWidth / 2, y - WorldCells.BlocksHeight / 2);
        }

        public static Vector2 GetCustardPosition(Coords coords)
        {
            return GetCustardPosition(coords.X, coords.Y);
        }


        private struct CustardAreaInfo
        {
            public List<Coords> CustardFromAbove;
            public List<Coords> CellsAtSameLevel;
            public List<Coords> CellsBelow;


            public CustardAreaInfo(List<Coords> custardFromAbove, List<Coords> cellsAtSameLevel,
                List<Coords> cellsBelow)
            {
                CustardFromAbove = custardFromAbove;
                CellsBelow = cellsBelow;
                CellsAtSameLevel = cellsAtSameLevel;
            }
        }
    }
}