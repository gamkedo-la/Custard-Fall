using System;
using Custard;
using UnityEngine;

namespace World
{
    public class TerrainLoader : MonoBehaviour
    {
        public WorldCells worldCells;
        [SerializeField]
        LayerMask layerMask;
        
        public void LoadWorld()
        {
            worldCells.Init();
            ScanTerrain();
        }

        private void ScanTerrain()
        {
            RaycastHit hit;
            for (byte x = 0; x < WorldCells.BlocksWidth; x++)
            {
                for (byte y = 0; y < WorldCells.BlocksHeight; y++)
                {
                    var cellMid = worldCells.GetWorldPosition(Coords.Of(x, y));
                    var from = new Vector3(cellMid.x,20,cellMid.y);
                    if(Physics.Raycast(from, Vector3.down, out hit, 30, layerMask))
                    {
                        var terrainHeight = Math.Round(hit.point.y);
                        worldCells.WriteHeightAt(Coords.Of(x,y), (byte)terrainHeight);
                    }
                }
            }
        }
    }
}