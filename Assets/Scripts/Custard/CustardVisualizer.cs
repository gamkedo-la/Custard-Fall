using System.Collections.Generic;
using UnityEngine;

namespace Custard
{
    public class CustardVisualizer : MonoBehaviour
    {
        public WorldCells worldCells;
        public CustardState custardState;

        public GameObject custardBlockPrefab;

        private GameObject _custardBlocksParent;
        public CustardBlock[,] CustardRenderBlocks;

        private void Start()
        {
            _custardBlocksParent = gameObject;
            CustardRenderBlocks = new CustardBlock[WorldCells.BlocksWidth, WorldCells.BlocksHeight];

            InitCustardBlocks();
        }
    
        private void Update()
        {
        }

        public void RenderChangedCustard()
        {
            var cellsToUpdate = custardState.Buffer;
            foreach (var cellUpdate in cellsToUpdate)
            {
                CustardBlock custardRenderBlock = FindRenderBlock(cellUpdate.Coords);
                var newCustardLevel = cellUpdate.Value;
                if (newCustardLevel > 0)
                {
                    var blockGameObject = custardRenderBlock.gameObject;
                    var scale = blockGameObject.transform.localScale;
                    blockGameObject.transform.localScale = new Vector3(scale.x, newCustardLevel, scale.z);
                    var position = blockGameObject.transform.position;
                    // TODO interpolate change
                    blockGameObject.transform.position = new Vector3(position.x,
                        worldCells.GetHeightAt(cellUpdate.Coords.X, cellUpdate.Coords.Y) + newCustardLevel - .5f, position.z);

                    custardRenderBlock.Show();
                }
                else
                {
                    custardRenderBlock.Hide();
                }
            }
        }

        private CustardBlock FindRenderBlock(Coords coords)
        {
            return CustardRenderBlocks[coords.X, coords.Y];
        }

        private void InitCustardBlocks()
        {
            for (byte x = 0; x < WorldCells.BlocksWidth; x++)
            for (byte y = 0; y < WorldCells.BlocksHeight; y++)
            {
                var custardPosition = WorldCells.GetWorldPosition(x, y);
                var custardCell = Instantiate(custardBlockPrefab,
                    new Vector3(custardPosition.x, 1.5f, custardPosition.y),
                    Quaternion.identity, _custardBlocksParent.transform);
                var custardRenderBlock = custardCell.GetComponent<CustardBlock>();
                CustardRenderBlocks[x, y] = custardRenderBlock;
            }
        }
    }
}