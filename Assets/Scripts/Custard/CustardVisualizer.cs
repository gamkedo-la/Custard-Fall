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
        private CustardBlock[,] _custardRenderBlocks;

        private void Start()
        {
            _custardBlocksParent = GameObject.Find("Custard");
            _custardRenderBlocks = new CustardBlock[WorldCells.BlocksWidth, WorldCells.BlocksHeight];

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
                var newCustardLevel = cellUpdate.AbsoluteCustardLevel;
                if (newCustardLevel > 0)
                {
                    var blockGameObject = custardRenderBlock.gameObject;
                    var scale = blockGameObject.transform.localScale;
                    blockGameObject.transform.localScale = new Vector3(scale.x, newCustardLevel, scale.z);
                    var position = blockGameObject.transform.position;
                    // TODO interpolate change
                    blockGameObject.transform.position = new Vector3(position.x,
                        worldCells.GetHeightAt(cellUpdate.Coords.X, cellUpdate.Coords.Y) +newCustardLevel * .5f + 1f, position.z);

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
            return _custardRenderBlocks[coords.X, coords.Y];
        }

        private void InitCustardBlocks()
        {
            for (byte i = 0; i < WorldCells.BlocksWidth; i++)
            for (byte j = 0; j < WorldCells.BlocksHeight; j++)
            {
                var custardPosition = GetCustardPosition(i, j);
                var custardCell = Instantiate(custardBlockPrefab,
                    new Vector3(custardPosition.x, 1.5f, custardPosition.y),
                    Quaternion.identity, _custardBlocksParent.transform);
                var custardRenderBlock = custardCell.GetComponent<CustardBlock>();
                _custardRenderBlocks[i, j] = custardRenderBlock;
            }
        }

        private static Vector2 GetCustardPosition(byte x, byte y)
        {
            return new Vector2(x - WorldCells.BlocksWidth / 2, y - WorldCells.BlocksHeight / 2);
        }

    }
}