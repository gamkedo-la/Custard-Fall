using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public WorldCells worldCells;
    public Inhaler inhaler;

    private AudioSource inhaleSFX;

    private bool _moveForwards = false;

    public float movementSpeed = 4;

    public bool isRunning = false;
    public float runningMultiplier;

    public float cooldownTime = 2.5f;
    public float runningDuration = .7f;
    private float nextRunningTime = 0;

    public float grappleCooldownTime = 2;
    private float nextGrappleTime = 2;
    [SerializeField] private float grappleDistance = 7f;
    [SerializeField] private float grappleSpeed = 8f;
    private Vector3 grapplePoint;

    bool grappling;

    // yOffset represents local terrain detail the player can stand on, so they are not clipped to round numbers
    private float yOffset = -.05f;
    private Collider _collider;

    public InputControlScheme gameplayScheme;

    public GameObject itemInHand;

    //health bar
    public int maxHealth = 90;
    public int currentHealth;
    public Healthbar healthbar;

    // Start is called before the first frame update
    private void Start()
    {
        _collider = GetComponent<Collider>();
        inhaleSFX = GetComponent<AudioSource>();

        currentHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Time.time > nextRunningTime - cooldownTime + runningDuration)
        {
            isRunning = false;
            movementSpeed = 6;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthbar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        worldCells.ResetChanges();
        SceneManager.LoadScene(currentScene.name);
    }

    void GainHealth(int health)
    {
        currentHealth += health;
        healthbar.SetHealth(currentHealth);
    }

    private void FixedUpdate()
    {
        nextGrappleTime += Time.fixedDeltaTime;
        if (grappling)
        {
            transform.position =
                Vector3.MoveTowards(transform.position, grapplePoint, grappleSpeed * Time.fixedDeltaTime);
            if (Vector3.Distance(transform.position, grapplePoint) < 0.01f)
            {
                grappling = false;
            }

            return;
        }

        if (_moveForwards)
        {
            MovePlayerForward();
        }
    }

    private void MovePlayerForward()
    {
        var currentTransform = transform;
        var currentPosition = currentTransform.position;
        var colliderBounds = _collider.bounds;

        var tracePoint = currentPosition + currentTransform.forward * colliderBounds.extents.x / 2;

        Coords coords = worldCells.GetCellPosition(tracePoint);
        // terrainHeight: currently out of bounds of terrain height check is coded as 255 value (int max)
        var terrainHeight = worldCells.GetHeightAt(coords);
        var heightDifference = terrainHeight - colliderBounds.min.y;
        // player cannot scale high ground
        // player can only leap from a high point if running
        if (terrainHeight != 255 && (isRunning ? heightDifference : Math.Abs(heightDifference)) < 2.75f &&
            worldCells.GetWorldItemHeightAt(coords) == 0)
        {
            currentPosition += Time.deltaTime * movementSpeed * currentTransform.forward;
            if (Math.Abs(heightDifference) > .0001f)
            {
                currentPosition += (heightDifference + colliderBounds.extents.y / 2 + yOffset) * Time.deltaTime * 18 *
                                   Vector3.up;
            }
        }

        currentTransform.position = currentPosition;
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
            inhaleSFX.Play();
        }
        else
        {
            inhaler.StopInhale();
            inhaleSFX.Stop();
        }
    }

    public void OnRun(InputValue context)
    {
        if (!isRunning && Time.time > nextRunningTime)
        {
            if (context.isPressed)
            {
                isRunning = true;
                movementSpeed *= runningMultiplier;
                print("ability used, cooldownstarted");
                nextRunningTime = Time.time + cooldownTime; //running cooldown
            }
        }
    }

    public void OnDebugHealthUp(InputValue context)
    {
        //Debug.Log("health up "+context.isPressed);

        if (currentHealth <= 3)
        {
            Debug.Log("health up");
            GainHealth(1);
        }
    }

    public void OnDebugHealthDown(InputValue context)
    {
        //Debug.Log("health up "+context.isPressed);

        if (currentHealth >= 0)
        {
            Debug.Log("health down");
            TakeDamage(1);
        }
    }

    public void OnUseItem(InputValue context)
    {
        if (!context.isPressed && itemInHand != null)
        {
            PlaceItemInHand();
        }
    }

    public void OnGrapple(InputValue context)
    {
        if (grappling)
        {
            return;
        }

        if (nextGrappleTime < grappleCooldownTime)
        {
            return;
        }

        //On Right Click Raycast from mouse to find collider

        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, grappleDistance);
        bool hitSuccess = false;

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                if (hit.transform.gameObject.name == "ItemCollision")
                {
                    continue;
                }

                grapplePoint = hit.point;
                hitSuccess = true;
                grapplePoint = new Vector3(grapplePoint.x, transform.position.y, grapplePoint.z);
                break;
            }
        }

        if (!hitSuccess)
        {
            return;
        }

        grappling = true;
        nextGrappleTime = 0;
    }

    private void PlaceItemInHand()
    {
        var currentTransform = transform;
        var result = currentTransform.position;
        var colliderBounds = _collider.bounds;

        var tracePoint = result + currentTransform.forward * 1.1f;

        Coords coords = worldCells.GetCellPosition(tracePoint.x, tracePoint.z);
        // terrainHeight: currently out of bounds of terrain height check is coded as 255 value (int max)
        var terrainHeight = worldCells.GetHeightAt(coords);
        var heightDifference = terrainHeight - colliderBounds.min.y;
        // the player cannot scale high ground
        if (terrainHeight != 255 && heightDifference < 1.5f && itemInHand != null)
        {
            var cellWorldPosition = worldCells.GetWorldPosition(coords);
            // only if no other item at target position
            if (worldCells.GetWorldItemHeightAt(coords) == 0)
                Instantiate(itemInHand, new Vector3(cellWorldPosition.x, terrainHeight + 0.2f, cellWorldPosition.y),
                    Quaternion.Euler(90f, 0, 90f));
        }
    }
}