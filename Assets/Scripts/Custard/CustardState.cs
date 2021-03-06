using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using UnityEngine;

namespace Custard
{
    [CreateAssetMenu(fileName = "CustardState", menuName = "CustardFall/CustardState", order = 0)]
    public class CustardState : ScriptableObject
    {
        public readonly HashSet<Coords> CellsToProcessInCurrentIteration = new();
        public readonly HashSet<Coords> CellsThatMightCauseChangeNextIteration = new();
        public readonly HashSet<CellValue> Buffer = new();
        // just in case we need them
        public readonly HashSet<CellValue> BufferFromPreviousIteration = new();

        public int[,] CustardArea;
        public int GlobalTideLevel = 1;
        // if there is no custard on a cell and it has not custard neighbors, add custard if scheduled for update
        public bool CreationalMode = false;

        public void Init()
        {
            Debug.Log("initialize custard state model");
            CustardArea = new int[WorldCells.BlocksWidth, WorldCells.BlocksHeight];
        }
        

        public void RegisterUpdate(int x, int y, int newCustardLevel)
        {
            Buffer.Add(new CellValue(x, y, newCustardLevel));
        }
        
        public void RegisterUpdate(Coords coords, int newCustardLevel)
        {
            Buffer.Add(new CellValue(coords, newCustardLevel));
        }

        public void QueueCellForCurrentIteration(int x, int y)
        {
            QueueCellForCurrentIteration(Coords.Of(x, y));
        }

        public void QueueCellsForCurrentIteration(IEnumerable<Coords> coords)
        {
            CellsToProcessInCurrentIteration.AddRange(coords);
        }
        
        public void QueueCellForCurrentIteration(Coords coords)
        {
            CellsToProcessInCurrentIteration.Add(coords);
        }

        public void QueueCellForNextIteration(int x, int y)
        {
            QueueForNextIteration(Coords.Of(x, y));
        }

        public void QueueCellsForNextIteration(IEnumerable<Coords> coords)
        {
            CellsThatMightCauseChangeNextIteration.AddRange(coords);
        }
        
        public void QueueForNextIteration(Coords coords)
        {
            CellsThatMightCauseChangeNextIteration.Add(coords);
        }

        public void ApplyAllUpdatesAndStartNewIteration()
        {
            foreach (var cellUpdate in Buffer)
            {
                CustardArea[cellUpdate.Coords.X, cellUpdate.Coords.Y] = cellUpdate.Value;
            }
            BufferFromPreviousIteration.Clear();
            BufferFromPreviousIteration.AddRange(Buffer);
            Buffer.Clear();
            
            CellsToProcessInCurrentIteration.AddRange(CellsThatMightCauseChangeNextIteration);
            CellsThatMightCauseChangeNextIteration.Clear();
        }

        public int GetCurrentCustardLevelAt(Coords coords)
        {
            return CustardArea[coords.X, coords.Y];
        }
        
        public int GetCurrentCustardLevelAt(int x, int y)
        {
            return CustardArea[x, y];
        }

        public void MarkAsProcessed(Coords coords)
        {
            CellsToProcessInCurrentIteration.Remove(coords);
        }
    }
}