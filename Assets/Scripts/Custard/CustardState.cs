using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Custard
{
    [CreateAssetMenu(fileName = "CustardState", menuName = "CustardFall/CustardState", order = 0)]
    public class CustardState : ScriptableObject
    {
        public readonly HashSet<Coords> CellsToProcessInCurrentIteration = new();
        public readonly HashSet<Coords> CellsThatMightCauseChangeNextIteration = new();
        public readonly HashSet<CellUpdate> Buffer = new();
        // just in case we need them
        public readonly HashSet<CellUpdate> BufferFromPreviousIteration = new();

        public byte[,] CustardArea;

        public void Init()
        {
            Debug.Log("initialize custard state model");
            CustardArea = new byte[WorldCells.BlocksWidth, WorldCells.BlocksHeight];
        }
        

        public void RegisterUpdate(byte x, byte y, byte newCustardLevel)
        {
            Buffer.Add(new CellUpdate(x, y, newCustardLevel));
        }
        
        public void RegisterUpdate(Coords coords, byte newCustardLevel)
        {
            Buffer.Add(new CellUpdate(coords, newCustardLevel));
        }

        public void QueueCellForCurrentIteration(byte x, byte y)
        {
            QueueCellForCurrentIteration(Coords.Of(x, y));
        }

        public void QueueCellForCurrentIteration(Coords coords)
        {
            CellsToProcessInCurrentIteration.Add(coords);
        }

        public void QueueCellForNextIteration(byte x, byte y)
        {
            QueueCellForNextIteration(Coords.Of(x, y));
        }


        public void QueueCellForNextIteration(Coords coords)
        {
            CellsThatMightCauseChangeNextIteration.Add(coords);
        }

        public void ApplyAllUpdatesAndStartNewIteration()
        {
            foreach (var cellUpdate in Buffer)
            {
                CustardArea[cellUpdate.Coords.X, cellUpdate.Coords.Y] = cellUpdate.AbsoluteCustardLevel;
            }
            BufferFromPreviousIteration.Clear();
            BufferFromPreviousIteration.AddRange(Buffer);
            Buffer.Clear();
            
            CellsToProcessInCurrentIteration.AddRange(CellsThatMightCauseChangeNextIteration);
            CellsThatMightCauseChangeNextIteration.Clear();
        }

        public byte GetCurrentCustardLevelAt(int x, int y)
        {
            return CustardArea[x, y];
        }

        public void MarkAsProcessed(Coords coords)
        {
            CellsToProcessInCurrentIteration.Remove(coords);
        }
    }
}