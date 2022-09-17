using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Custard;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Custard
{
    public class CustardManager : MonoBehaviour
    {
        public WorldCells worldCells;
        public CustardState custardState;
        private Dictionary<Coords, ImpededCell> _impededCells;

        private CustardSpawn[] _initialSpawns;

        public CustardVisualizer custardVisualizer;
        public GameObject custardBlockPrefab;

        public bool pauseIterationCountDown;


        public float custardCrawlDuration = .4f;
        public int targetTideLevel = 2;
        private float _custardUpdateCountdown;


        private void Start()
        {
            worldCells.onItemHeightChanged += OnHeihgtChanged;
            _custardUpdateCountdown = custardCrawlDuration;
            _impededCells = new Dictionary<Coords, ImpededCell>();

            _initialSpawns = FindObjectsOfType<CustardSpawn>();
        }


        public void InitCustardState()
        {
            custardState.GlobalTideLevel = targetTideLevel;
            custardState.Init();

            if (_initialSpawns != null)
                foreach (var custardSpawn in _initialSpawns)
                {
                    var spawnPosition = worldCells.GetCellPosition(custardSpawn.transform.position);
                    if (custardSpawn.fillUp)
                    {
                        FillUp(spawnPosition);
                    }
                    else
                    {
                        custardState.RegisterUpdate(spawnPosition, 1);
                        custardState.QueueForNextIteration(spawnPosition);
                    }
                }

            // iteration 0 for custardState
            for (int x = 0; x < WorldCells.BlocksWidth; x++)
            for (int y = 0; y < WorldCells.BlocksHeight; y++)
            {
                if (IsInitialWorldCustard(x, y))
                {
                    custardState.RegisterUpdate(x, y, 1);
                    custardState.QueueCellForNextIteration(x, y);
                }
            }

            // apply and proceed to next iteration
            _custardUpdateCountdown = 0;
        }

        private void FillUp(Coords spawnPosition)
        {
            HashSet<Coords> alreadyHandled = new HashSet<Coords>();
            Queue<Coords> fillUpCandidates = new Queue<Coords>();
            fillUpCandidates.Enqueue(spawnPosition);
            var targetHeight = worldCells.GetHeightAt(spawnPosition) + 1;
            while (fillUpCandidates.Count != 0)
            {
                var coords = fillUpCandidates.Dequeue();
                
                if (!alreadyHandled.Contains(coords)&&!IsOutOfBounds(coords))
                {
                    var currentHeight = worldCells.GetHeightAt(coords);
                    if (currentHeight < targetHeight)
                    {
                        custardState.RegisterUpdate(coords,
                            Math.Max(custardState.GetCurrentCustardLevelAt(coords), targetHeight - currentHeight));
                        var info = RetrieveCustardInfo(coords,
                            GetLocalNeighborhood(coords, (x, y) => 0),
                            GetLocalNeighborhood(coords, (x, y) => worldCells.GetHeightAt(x, y)));
                        info.CellsBellow.ForEach(fillUpCandidates.Enqueue);
                        info.CellsAtSameLevel.ForEach(fillUpCandidates.Enqueue);
                    }
                }
                alreadyHandled.Add(coords);
            }
        }

        private bool IsInitialWorldCustard(int x, int y)
        {
            return x == 0 || y == 0 || x == WorldCells.BlocksWidth - 1 || y == WorldCells.BlocksHeight - 1;
        }

        private void Update()
        {
            UpdateWhichCellsAreImpeded();
            List<CellValue> valuesOfImpededCells = new List<CellValue>();
            foreach (var cell in custardState.Buffer)
            {
                if (_impededCells.ContainsKey(cell.Coords))
                {
                    valuesOfImpededCells.Add(cell);
                }
            }

            custardVisualizer.RenderCustard(valuesOfImpededCells);
        }

        private void FixedUpdate()
        {
            custardState.GlobalTideLevel = targetTideLevel;
            if (!pauseIterationCountDown)
                _custardUpdateCountdown -= Time.deltaTime;

            UpdateWhichCellsAreImpeded();

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

        private void UpdateWhichCellsAreImpeded()
        {
            List<Coords> _timedOut = new List<Coords>();

            foreach (var impededCell in _impededCells.Values)
            {
                impededCell.SetDuration(impededCell.GetDuration() - Time.deltaTime);
                if (impededCell.GetDuration() < 0)
                    _timedOut.Add(impededCell.GetCoords());
            }

            foreach (var timedOut in _timedOut)
            {
                _impededCells.Remove(timedOut);
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

        public void SeedCustardUpdate(int seed)
        {
            var random = new System.Random(seed);
            const int numWindows = 8;
            var windowNoX = random.Next(0, numWindows / 2);
            var windowNoY = random.Next(0, numWindows / 2);
            const int windowSize = WorldCells.BlocksWidth / numWindows * 2;
            const float gate = .4f;
            for (int i = 0; i < windowSize; i++)
            for (int j = 0; j < windowSize; j++)
            {
                float pX = (i + windowSize * windowNoX) / (float) WorldCells.BlocksWidth;
                float pY = (j + windowSize * windowNoY) / (float) WorldCells.BlocksWidth;
                if (Mathf.PerlinNoise(pX, pY) > gate)
                    custardState.QueueCellForNextIteration(i + windowSize * windowNoX, j + windowSize * windowNoY);
            }
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
                custardState.MarkAsProcessed(pivot);
                if(IsOutOfBounds(pivot))
                    continue;
                var custardAreaAroundPivot =
                    GetLocalNeighborhood(pivot, (i, j) => custardState.GetCurrentCustardLevelAt(i, j));
                var terrainAreaAroundPivot = GetLocalNeighborhood(pivot, (i, j) => worldCells.GetHeightAt(i, j));

                UpdateCustardState(pivot, custardAreaAroundPivot, terrainAreaAroundPivot);
            }
        }


        private void UpdateCustardState(Coords pivot, int[,] custardAreaAroundPivot, int[,] terrainAreaAroundPivot)
        {
            _impededCells.TryGetValue(pivot, out var impededCell);

            // note: for now let us ignore the case of a corner-on-corner block setup, where the neighboring custard is only adjacent through the corner edge
            int pivotCustardAmount = custardAreaAroundPivot[1, 1];
            var pivotTerrainHeight = terrainAreaAroundPivot[1, 1];
            var pivotTotalHeight = pivotCustardAmount + pivotTerrainHeight;

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
                        custardState.QueueCellsForNextIteration(info.CellsBellow);
                    }
                }
                else if (info.CellsBellow.Count != 0)
                {
                    if (pivotCustardAmount > 0)
                    {
                        // we flow downwards
                        newPivotCustardAmount = pivotCustardAmount - 1;
                        if (custardState.GlobalTideLevel < newPivotCustardAmount + pivotTerrainHeight)
                            custardState.QueueForNextIteration(pivot);
                        // next iteration: check all cells that might get affected by this change
                        custardState.QueueCellsForNextIteration(info.CellsAtSameLevel);
                    }

                    // next iteration: check all cells where I might flow down into
                    custardState.QueueCellsForNextIteration(info.CellsBellow);
                }
                else if (pivotCustardAmount > 1)
                {
                    // custard is trapped,
                    // so we simply shrink up to a single layer
                    newPivotCustardAmount = pivotCustardAmount - 1;
                    if (custardState.GlobalTideLevel < newPivotCustardAmount + pivotTerrainHeight)
                        custardState.QueueForNextIteration(pivot);
                    // next iteration: check all cells that now might flow into me
                    custardState.QueueCellsForNextIteration(info.CellsAtSameLevel);
                }
                else if (pivotCustardAmount == 1 && info.CellsBellow.Count == 0 &&
                         info.CellsAtSameLevel.Count + info.CustardFromAbove.Count == 8)
                {
                    // custard is trapped
                    newPivotCustardAmount = 1;
                }
                else if (custardState.GlobalTideLevel == 0 && pivotTerrainHeight == 0)
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
                custardState.QueueCellsForNextIteration(info.CellsBellow);
            }
            else
            {
                // # custard is below tide level => we should grow
                var info = RetrieveCustardInfo(pivot, custardAreaAroundPivot, terrainAreaAroundPivot);

                bool shouldRise = pivotCustardAmount > 0 || info.CustardFromAbove.Count > 0 ||
                                  info.CustardAtSameLevel.Count > 0 || custardState.CreationalMode;
                if (shouldRise)
                {
                    newPivotCustardAmount = pivotCustardAmount + 1;
                    if (custardState.GlobalTideLevel > newPivotCustardAmount + pivotTerrainHeight)
                        custardState.QueueForNextIteration(pivot);
                    // cells above should flow into me
                    custardState.QueueCellsForNextIteration(info.CustardFromAbove);
                    // spread: next iteration: check all cells below me or currently at same level should grow next as well
                    custardState.QueueCellsForNextIteration(info.CellsAtSameLevel);
                    custardState.QueueCellsForNextIteration(info.CellsBellow);
                }
            }

            if (impededCell != null)
            {
                var strength = impededCell.GetStrength();
                if (newPivotCustardAmount <= 1)
                    newPivotCustardAmount = 0;
                else if (newPivotCustardAmount == 2 && strength > .33f)
                    newPivotCustardAmount = 1;
            }

            // prepare value for int range
            if (newPivotCustardAmount < 0)
                newPivotCustardAmount = 0;
            else if (newPivotCustardAmount > 255)
            {
                // don't change if out of range for some reason
                newPivotCustardAmount = pivotCustardAmount;
            }

            if (newPivotCustardAmount != pivotCustardAmount)
            {
                custardState.RegisterUpdate(pivot, newPivotCustardAmount);
            }
        }

        private static bool IsOutOfBounds(Coords coords)
        {
            return coords.X is < 0 or >= WorldCells.BlocksWidth || coords.Y is < 0 or >= WorldCells.BlocksHeight;
        }

        private CustardAreaInfo RetrieveCustardInfo(Coords pivot, int[,] custardAreaAroundPivot,
            int[,] terrainAreaAroundPivot)
        {
            List<Coords> custardCellsAbove = new List<Coords>();
            List<Coords> custardtAtSameLevel = new List<Coords>();
            List<Coords> sameLevelCells = new List<Coords>();
            List<Coords> cellsBellow = new List<Coords>();


            int pivotCustardAmount = custardAreaAroundPivot[1, 1];
            var pivotTerrainHeight = terrainAreaAroundPivot[1, 1];
            var pivotTotalHeight = pivotCustardAmount + pivotTerrainHeight;

            for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
            {
                var coord = pivot.Add(x - 1, y - 1);
                if (x == 1 && y == 1)
                {
                    continue;
                }
                else if (pivot.X == 0 && x == 0 || pivot.Y == 0 && y == 0 ||
                         pivot.X == WorldCells.BlocksWidth - 1 && x == 2 ||
                         pivot.Y == WorldCells.BlocksHeight - 1 && y == 2)
                {
                    sameLevelCells.Add(coord);
                    continue;
                }

                // convert relative offset to coord
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
                    if (neighborCustardAmount != 0)
                        custardtAtSameLevel.Add(coord);
                }
                else
                {
                    // or how much of the current stack needs to disappear to stay at level with neighboring custard
                    // or be removed entirely
                    var heightDifference = pivotTotalHeight - neighborTotalHeight;
                    if (heightDifference != 0)
                    {
                        cellsBellow.Add(coord);
                    }
                }
            }

            return new CustardAreaInfo(custardCellsAbove, custardtAtSameLevel, sameLevelCells, cellsBellow);
        }

        private int[,] GetLocalNeighborhood(Coords coords, Func<int, int, int> getValueAt)

        {
            var localNeighborhood = new int[3, 3];

            var leftClamp = coords.X - 1 < 0;
            var topClamp = coords.Y - 1 < 0;
            var rightClamp = coords.X + 1 >= WorldCells.BlocksWidth;
            var bottomClamp = coords.Y + 1 >= WorldCells.BlocksHeight;
            for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                if ((i == 0 && leftClamp) || (i == 2 && rightClamp) || (j == 0 && topClamp) || (j == 2 && bottomClamp))
                    localNeighborhood[i, j] = 0;
                else if (getValueAt != null)
                    localNeighborhood[i, j] = getValueAt.Invoke(coords.X + i - 1, coords.Y + j - 1);

            return localNeighborhood;
        }


        private struct CustardAreaInfo
        {
            public readonly List<Coords> CustardFromAbove;
            public readonly List<Coords> CustardAtSameLevel;
            public readonly List<Coords> CellsAtSameLevel;
            public readonly List<Coords> CellsBellow;

            public CustardAreaInfo(List<Coords> custardFromAbove, List<Coords> custardAtSameLevel,
                List<Coords> cellsAtSameLevel,
                List<Coords> cellsBellow)
            {
                CustardFromAbove = custardFromAbove;
                CustardAtSameLevel = custardAtSameLevel;
                CellsBellow = cellsBellow;
                CellsAtSameLevel = cellsAtSameLevel;
            }
        }

        public void ImpedeCustardCell(Coords coords, int originY, float strength)
        {
            if (IsOutOfBounds(coords))
                return;
            // if (!custardState.Buffer.TryGetValue(new CellValue(coords, 0), out var presentCell))
            // {
            //     var cell = custardState.CustardArea[coords.X, coords.Y];
            //     custardState.Buffer.Add(new CellValue(coords, Math.Max(cell - 1,0)));
            // }
            // else
            // {
            //     custardState.Buffer.Add(new CellValue(coords, Math.Max(presentCell.Value - 1,0)));
            // }
            custardState.CellsThatMightCauseChangeNextIteration.Add(coords);

            // check, is actually custard at impeded point?
            if (custardState.CustardArea[coords.X, coords.Y] + worldCells.GetHeightAt(coords) < originY)
                return;

            if (!_impededCells.TryGetValue(coords, out var alreadyImpeded))
            {
                _impededCells.Add(coords, new ImpededCell(coords, strength));
            }
            else if (alreadyImpeded.GetStrength() < strength)
            {
                alreadyImpeded.SetStrength(strength);
                alreadyImpeded.SetDuration(2f);
            }
            else
            {
                alreadyImpeded.SetDuration(2f);
            }
        }

        private void OnHeihgtChanged(Coords coords)
        {
            custardState.QueueCellForCurrentIteration(coords);
        }
    }


    public class ImpededCell
    {
        private readonly Coords _coords;
        private float _strength;
        private float durationLeft = 1f;

        public ImpededCell(Coords coords, float strength)
        {
            this._coords = coords;
            this._strength = strength;
        }

        public Coords GetCoords()
        {
            return _coords;
        }

        public float GetStrength()
        {
            return _strength;
        }

        public float GetDuration()
        {
            return durationLeft;
        }

        public void SetDuration(float duration)
        {
            durationLeft = duration;
        }

        public void SetStrength(float strength)
        {
            this._strength = strength;
        }
    }
}