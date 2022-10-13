using System;
using System.Collections.Generic;
using Custard;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class Inhaler : MonoBehaviour
{
    public WorldCells worldCells;
    public CustardManager custardManager;
    public Inventory inventory;
    public ParticleSystem inhalingParticleSystem;
    public ParticleSystem inhalingConeParticleSystem;
    public GameObject plusOnePrefab;
    public readonly Dictionary<Vector3, InhaleCell> affectedCells = new Dictionary<Vector3, InhaleCell>();
    private float _distance;
    public bool isInhale;

    private Quaternion _previousRotation;

    private void FixedUpdate()
    {
        if (isInhale)
        {
            UpdateInhaleConeCells();
            InhaleCustard();
            InhaleCone();
            InhaleResources();
        }
    }

    private void InhaleCustard()
    {
        bool isCustardInCone = false;
        foreach (var keyValuePair in affectedCells)
        {
            var inhaleCell = keyValuePair.Value;
            custardManager.ImpedeCustardCell(inhaleCell.GetCoords(), inhaleCell.GetWorldY(), inhaleCell.GetStrength());
            if (!isCustardInCone)
            {
                var coords = keyValuePair.Value.GetCoords();
                var worldHeight = custardManager.worldCells.GetHeightAt(coords);
                // out of bounds
                if (worldHeight == WorldCells.OutOufBoundsHeight)
                    continue;
                var custardLevelAt = custardManager.custardState.GetCurrentCustardLevelAt(coords);
                isCustardInCone |= worldHeight < inhaleCell.GetWorldY() &&
                                   worldHeight + custardLevelAt >= inhaleCell.GetWorldY();
            }
        }

        if (isCustardInCone)
        {
            OnCustardInhale();
        }
        else
        {
            OnCustardInhaleStopped();
        }
    }

    private void InhaleCone()
    {
        foreach (var keyValuePair in affectedCells)
        {
            var inhaleCell = keyValuePair.Value;
            custardManager.ImpedeCustardCell(inhaleCell.GetCoords(), inhaleCell.GetWorldY(), inhaleCell.GetStrength());
        }
    }

    private void InhaleResources()
    {
        foreach (var keyValuePair in affectedCells)
        {
            var inhaleCell = keyValuePair.Value;

            List<WorldItem> worldCellItems;
            if (WorldItems.itemsInCell.TryGetValue(inhaleCell.GetCoords(), out worldCellItems))
            {
                for (int i = 0; i < worldCellItems.Count; i++)
                {
                    var item = worldCellItems[i];
                    if (item is InhaleListener listener)
                    {
                        var worldHeight = worldCells.GetTerrainHeightAt(inhaleCell.GetCoords());
                        if (inhaleCell.GetWorldY() == worldHeight + 1)
                            listener.Inhale(this, inhaleCell.GetStrength());
                    }
                }
            }
        }
    }

    private void UpdateInhaleConeCells()
    {
        var gameObjectTransform = gameObject.transform;
        var rotation = gameObjectTransform.rotation;
        var coneOrigin = gameObjectTransform.position;
        var coneOriginCoords = worldCells.GetCellPosition(coneOrigin);

        if (rotation.Equals(_previousRotation))
        {
            return;
        }

        _previousRotation = rotation;


        affectedCells.Clear();
        List<Vector3> localConePoints = new List<Vector3>();

        // construct a cone
        // central pane
        addConeRow(localConePoints, 3, 0, 0);
        addConeRow(localConePoints, 5, 1, 0);
        addConeRow(localConePoints, 5, 2, 0);
        addConeRow(localConePoints, 5, 3, 0);
        addConeRow(localConePoints, 5, 4, 0);
        addConeRow(localConePoints, 5, 5, 0);
        addConeRow(localConePoints, 5, 6, 0);
        addConeRow(localConePoints, 3, 7, 0);
        addConeRow(localConePoints, 3, 8, 0);
        // lower pane
        addConeRow(localConePoints, 3, 1, -1);
        addConeRow(localConePoints, 5, 2, -1);
        addConeRow(localConePoints, 3, 3, -1);
        addConeRow(localConePoints, 1, 4, -1);
        // higher pane
        addConeRow(localConePoints, 3, 1, 1);
        addConeRow(localConePoints, 5, 2, 1);
        addConeRow(localConePoints, 3, 3, 1);
        addConeRow(localConePoints, 1, 4, 1);


        foreach (var localPosition in localConePoints)
        {
            var worldPoint = gameObjectTransform.TransformPoint(localPosition);
            var coords = worldCells.GetCellPosition(worldPoint);

            var worldPointY = worldCells.GetHeightAt(coneOriginCoords) + 1 + (int) localPosition.y;
            var strength = 1; //(int)localPosition.y <0?1f - (worldPoint - coneOrigin).magnitude / _distance: 1;
            var key = new Vector3(coords.X, worldPointY, coords.Y);
            if (affectedCells.TryGetValue(key, out var cell))
            {
                if (cell.GetStrength() < strength)
                    cell.SetStrength(strength);
            }
            else
            {
                cell = new InhaleCell(coords, worldPointY, strength);
                affectedCells.Add(key, cell);
            }
        }
    }


    private void addConeRow(List<Vector3> localConePoints, float width, int row, int relativeY)
    {
        const float stepSize = 0.25f;
        for (float relativeZ = row - stepSize; relativeZ <= row + 1; relativeZ += stepSize)
        for (float relativeX = -width / 2 - .44f; relativeX <= width / 2 + .44f; relativeX += stepSize)
            localConePoints.Add(new Vector3(relativeX, relativeY, relativeZ));
    }

    public void BeginInhaleInTransformDirection(float coneLength)
    {
        _distance = coneLength;
        if (!isInhale)
            inhalingConeParticleSystem.gameObject.SetActive(true);
        isInhale = true;
    }

    public void StopInhale()
    {
        isInhale = false;
        OnCustardInhaleStopped();
        inhalingConeParticleSystem.gameObject.SetActive(false);
        affectedCells.Clear();
    }


    public void OnResourceInhaled(Resource resource, int amount)
    {
        try
        {
            // a +1 style feedback number
            if (plusOnePrefab)
            {
                for (int i = 0; i < amount; i++)
                    Instantiate(plusOnePrefab,
                        new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z),
                        Quaternion.Euler(-90f, 0f, 0f));
            }

            var newAmount = inventory.AddOrSubResourceAmount(resource, amount);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void OnCustardInhale()
    {
        inhalingParticleSystem.gameObject.SetActive(true);
    }

    public void OnCustardInhaleStopped()
    {
        inhalingParticleSystem.gameObject.SetActive(false);
    }

    public class InhaleCell
    {
        private readonly Coords _coords;
        private readonly int _worldY;
        private float _strength;

        public InhaleCell(Coords coords, int worldY, float strength)
        {
            this._coords = coords;
            this._worldY = worldY;
            this._strength = strength;
        }

        public Coords GetCoords()
        {
            return _coords;
        }

        public int GetWorldY()
        {
            return _worldY;
        }

        public float GetStrength()
        {
            return _strength;
        }

        public void SetStrength(float strength)
        {
            this._strength = strength;
        }
    }
}