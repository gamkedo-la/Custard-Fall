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
    public Inhaler inhaler;

    private bool _moveForwards = false;

    public float movementSpeed = 4;

    public bool isRunning = false;
    public float runningMultiplier;

    public float cooldownTime = 2; 
    private float nextRunningTime = 0;

    // yOffset represents local terrain detail the player can stand on, so they are not clipped to round numbers
    private float yOffset = -.05f;
    private Collider _collider;

    public InputControlScheme gameplayScheme;

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
        if (_moveForwards)
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
        // terrainHeight: currently out of bounds of terrain height check is coded as 255 value (int max)
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

    public void OnMoveForwardGamepad(InputValue input)
    {
        Vector2 vector = input.Get<Vector2>();
        if (vector != new Vector2(0, 0))
        {
            _moveForwards = true;
        }
        else
        {
            _moveForwards = false;
        }
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

    public void OnLookAroundGamepad(InputValue context)
    {
        var val = context.Get<Vector2>();
        float angle = Mathf.Atan2(val.x, val.y) * Mathf.Rad2Deg;
        if (angle != 0)
        {
            transform.localRotation = Quaternion.Euler(new Vector3(0, angle - 90, 0));
        }
        
    }
    public void OnInhale(InputValue context)
    {
        if (context.isPressed)
        {
            inhaler.BeginInhaleInTransformDirection(4f);
        }
        else
        {
            inhaler.StopInhale();
        }
    }
   
public void OnRun(InputValue context)
    {
        if (Time.time > nextRunningTime)
        {
            if (context.isPressed)
            {
                isRunning = true;
                movementSpeed *= runningMultiplier;
                print("ability used, cooldownstarted");
                nextRunningTime = Time.time + cooldownTime; //running cooldown
            }
        }
        else
        {
            isRunning = false;
            movementSpeed = 6;
        }
    }
}