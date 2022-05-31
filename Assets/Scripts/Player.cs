using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public WorldCells worldCells;

    private bool _moveForwards = false;

    public float movementSpeed = 4;

    // yOffset represents local terrain detail the player can stand on, so they are not clipped to round numbers
    private float yOffset = -.05f;
    private Collider _collider;

    // Start is called before the first frame update
    private void Start()
    {
        _collider = GetComponent<Collider>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            MovePlayerForward();
        }
    }

    private void MovePlayerForward()
    {
        var currentTransform = transform;
        var result = currentTransform.position;
        var colliderBounds = _collider.bounds;

        var tracePoint = result + currentTransform.forward * colliderBounds.extents.x / 2;

        Coords coords = worldCells.GetCellPosition(tracePoint.x, tracePoint.z);
        // terrainHeight: currently out of bounds of terrain height check is coded as 255 value (byte max)
        var terrainHeight = worldCells.GetHeightAt(coords);
        var heightDifference = terrainHeight - colliderBounds.min.y;
        // the player cannot scale high ground
        if (terrainHeight != 255 && heightDifference < 2.75f)
        {
            result += Time.deltaTime * movementSpeed * currentTransform.forward;
            if (Math.Abs(heightDifference) > .0001f)
            {
                result += (heightDifference + colliderBounds.extents.y / 2 + yOffset) * Time.deltaTime * 18 *
                          Vector3.up;
            }
        }

        currentTransform.position = result;
    }

    public void OnMoveForward(InputValue input)
    {
        _moveForwards = input.isPressed;
    }

    public void OnMove(InputValue context)
    {
        var val = context.Get<Vector2>();
        var position = transform.position;
        Vector3 playerPos = Camera.main.WorldToScreenPoint(position);
        var lookAtPoint = Camera.main.ScreenToWorldPoint(new Vector3(val.x, val.y, playerPos.z));
        lookAtPoint.y = position.y;
        transform.LookAt(lookAtPoint);
    }
}