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

    private bool moveForwards = false;
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
        result += Time.deltaTime * movementSpeed * currentTransform.forward;
        Coords coords = worldCells.GetCellPosition(result.x, result.z);
        var terrainHeight = worldCells.GetHeightAt(coords);

        var colliderBounds = _collider.bounds;
        result += (terrainHeight - colliderBounds.min.y + colliderBounds.extents.y/2 + yOffset) * Time.deltaTime * 18 * Vector3.up;
        currentTransform.position = result;
    }

    public void OnMoveForward(InputValue input)
    {
        moveForwards = input.isPressed;
    }

    public void OnMove(InputValue context)
    {
        var val = context.Get<Vector2>();
        var position = transform.position;
        Vector3 playerPos = Camera.main.WorldToScreenPoint(position);
        var lookAtPoint = Camera.main.ScreenToWorldPoint(new Vector3(val.x,val.y, playerPos.z));
        lookAtPoint.y = position.y;
        transform.LookAt(lookAtPoint);
    }
}