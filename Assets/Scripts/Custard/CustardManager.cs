using System;
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
            foreach (var coords in processing)
            {
                var custardAreaOfEffect =
                    GetLocalNeighborhood(coords, (i, j) => custardState.GetCurrentCustardLevelAt(i, j));
                var heightAreaOfEffect = GetLocalNeighborhood(coords, (i, j) => worldCells.GetHeightAt(i, j));

                ChangeCellState(coords, custardAreaOfEffect, heightAreaOfEffect);
                custardState.MarkAsProcessed(coords);
            }
        }


        private void ChangeCellState(Coords coords, byte[,] custardAreaOfEffect, byte[,] heightAreaOfEffect)
        {
            if (coords.X is < 0 or > WorldCells.BlocksWidth - 1 && coords.Y is < 0 or > WorldCells.BlocksHeight - 1)
                // out of bounds of world area
                return;

            // note: for now let us ignore the case of a corner-on-corner block setup, where the neighboring custard is only adjacent through the corner edge
            int pivotCustardHeight = custardAreaOfEffect[1, 1];
            var pivotHeight = pivotCustardHeight + heightAreaOfEffect[1, 1];
            var biggestCustardStackAbove = 0;
            var stackRequiredToStayAtLevel = 0;

            for (byte x = 0; x < 3; x++)
            for (byte y = 0; y < 3; y++)
            {
                if (coords.X == 0 && x == 0)
                    continue;
                else if (coords.Y == 0 && y == 0)
                    continue;
                else if (coords.X == WorldCells.BlocksWidth - 1 && x == 2)
                    continue;
                else if (coords.Y == WorldCells.BlocksHeight - 1 && y == 2) continue;

                // the custard flows downwards, so we find out how much custard may be coming down
                int neighborCustardHeight = custardAreaOfEffect[x, y];
                var neighborHeight = neighborCustardHeight + heightAreaOfEffect[x, y];

                if (neighborHeight > pivotHeight)
                {
                    var flowyCustardStack = Math.Min(neighborCustardHeight, neighborHeight - pivotHeight);
                    if (flowyCustardStack > 0)
                        if (biggestCustardStackAbove < flowyCustardStack)
                        {
                            biggestCustardStackAbove = flowyCustardStack;
                            MarkCellMightChangeNextIteration(coords, 0, 0);
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
                        {
                            stackRequiredToStayAtLevel = Math.Max(1, pivotCustardHeight);
                            MarkCellMightChangeNextIteration(coords, 0, 0);
                        }
                    }
                    else
                    {
                        var tmpHeight = pivotCustardHeight > heightDifference ? heightDifference : pivotCustardHeight;
                        if (stackRequiredToStayAtLevel < tmpHeight) stackRequiredToStayAtLevel = tmpHeight;
                    }

                    if (heightDifference != 0)
                        MarkCellMightChangeNextIteration(coords, x - 1, y - 1);
                }
            }

            var update = biggestCustardStackAbove >= stackRequiredToStayAtLevel
                ? pivotCustardHeight + 1
                : stackRequiredToStayAtLevel;
            if (update is < 0 or > 255)
                // clamp to safest bet
                update = update < 0 ? 0 : pivotCustardHeight;

            // custardState.RegisterUpdate(coords,(byte) update);
            if (update != pivotCustardHeight)
            {
                custardState.RegisterUpdate(coords, (byte) update);
                // always consider recently changed cells in next iteration
                custardState.QueueCellForNextIteration(coords);
            }
        }

        // TODO simplify
        private void MarkCellMightChangeNextIteration(Coords pivotCoords, int x, int y)
        {
            if ((pivotCoords.X == 0 && x == -1) || (pivotCoords.X == WorldCells.BlocksWidth - 1 && x == 1) ||
                (pivotCoords.Y == 0 && y == -1) ||
                (pivotCoords.Y == WorldCells.BlocksHeight - 1 && y == 1))
                return;
            custardState.QueueCellForNextIteration(pivotCoords.Add(x, y));
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
    }
}