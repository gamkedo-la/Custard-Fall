using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public WorldCells worldCells;
    public Inhaler inhaler;

    private AudioSource inhaleSFX;


    [SerializeField] private GameObject playerDirectional;
    public float movementSpeed = 4;
    private bool _isLookInMoveDirection = true;
    private Vector3 _targetMoveDirection = Vector3.zero;
    private Vector3 _currentMoveDirection = Vector3.zero;
    private Vector3 _targetLookDirection = Vector3.zero;
    private Vector3 _currentLookDirection = Vector3.zero;
    [SerializeField] private float lookTransitionSpeed = .1f;
    [SerializeField] private float lookTransitionSpeedMouse = .02f;
    private Vector3 _velLook = Vector3.zero;
    [SerializeField] private float moveTransitionSpeed = .1f;
    private Vector3 _velMove = Vector3.zero;
    [SerializeField] private float dashTransitionSpeed = .1f;
    private float _velDash = 0f;
    [SerializeField] private float LookInMoveDirectionGraceTime = .3f;
    private float _nextLookInMoveDirectionTime;

    public bool isDashing = false;
    public float runningMultiplier;
    private float _currenRunningMultiplier = 1f;

    public float cooldownTime = 2.5f;
    public float runningDuration = .7f;
    private float nextRunningTime = 0;

    public float grappleCooldownTime = 2;
    private float nextGrappleTime = 2;
    [SerializeField] private float grappleDistance = 7f;
    [SerializeField] private float grappleSpeed = 8f;
    private Vector3 grapplePoint;


    [SerializeField]
    public bool ownsGrapplingHook;
    bool grappling;
    private bool _isMoveForward;



    // yOffset represents local terrain detail the player can stand on, so they are not clipped to round numbers
    private float yOffset = -.05f;
    private Collider _collider;

    public InputControlScheme gameplayScheme;

    public GameObject itemInHand;


    //health bar
    public int maxHealth = 90;
    public int currentHealth;
    public Healthbar healthbar;

    private int _mouseTargetLayerMask;
    private PauseActivator _pauseActivator;

    // Start is called before the first frame update
    private void Start()
    {
        inhaleSFX = GetComponent<AudioSource>();
        _collider = GetComponent<Collider>();
        _pauseActivator = FindObjectOfType<PauseActivator>();

        _mouseTargetLayerMask = LayerMask.GetMask("MousePointerTarget");

        currentHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);
        inhaler.owner = gameObject;
    }

    // Update is called once per frame
    private void Update()
    {
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

        if (Time.time > nextRunningTime - cooldownTime + runningDuration)
        {
            if (isDashing)
            {
                _currentMoveDirection = Vector3.zero;
            }

            _currenRunningMultiplier =
                Mathf.SmoothDamp(_currenRunningMultiplier, 1f, ref _velDash, dashTransitionSpeed * .1f);
            isDashing = false;
        }

        if (Time.time > _nextLookInMoveDirectionTime)
        {
            _isLookInMoveDirection = true;
        }

        if (_isLookInMoveDirection)
        {
            _targetLookDirection = _targetMoveDirection;
        }

        if (isDashing)
        {
            _currentMoveDirection = playerDirectional.transform.forward;
            _currenRunningMultiplier = Mathf.SmoothDamp(_currenRunningMultiplier, runningMultiplier, ref _velDash,
                dashTransitionSpeed);
        }

        bool lookAround = !_isLookInMoveDirection;
        _currentLookDirection =
            Vector3.SmoothDamp(_currentLookDirection, _targetLookDirection, ref _velLook,
                lookAround ? lookTransitionSpeedMouse : lookTransitionSpeed);
        if (!isDashing)
        {
            var targetMoveDirection = _isMoveForward ? playerDirectional.transform.forward : _targetMoveDirection;
            _currentMoveDirection = Vector3.SmoothDamp(_currentMoveDirection, targetMoveDirection, ref _velMove,
                moveTransitionSpeed);
        }

        if (_currentLookDirection.sqrMagnitude != 0f)
        {
            var currentTransform = playerDirectional.transform;
            currentTransform.LookAt(currentTransform.position + _currentLookDirection);
        }

        if (isDashing)
        {
            MovePlayer(playerDirectional.transform.forward);
        }
        else if (_currentMoveDirection.sqrMagnitude != 0f)
        {
            MovePlayer(_currentMoveDirection);
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
        MusicManager.Instance.SetUnder(false);
        SceneManager.LoadScene(currentScene.name);
    }

    void GainHealth(int health)
    {
        currentHealth += health;
        healthbar.SetHealth(currentHealth);
    }

    private void MovePlayerForward()
    {
        MovePlayer(playerDirectional.transform.forward);
    }

    private void MovePlayer(Vector3 targetDirection)
    {
        var currentTransform = transform;
        var currentPosition = currentTransform.position;
        var colliderBounds = _collider.bounds;

        var tracePoint = currentPosition + targetDirection * colliderBounds.extents.x / 2;

        Coords coords = worldCells.GetCellPosition(tracePoint);
        // terrainHeight: currently out of bounds of terrain height check is coded as 255 value (int max)
        var terrainHeight = worldCells.GetHeightAt(coords);
        var heightDifference = terrainHeight - colliderBounds.min.y;
        // player cannot scale high ground
        // player can only leap from a high point if running
        if (terrainHeight != 255 && (isDashing ? heightDifference : Math.Abs(heightDifference)) < 2.75f &&
            worldCells.GetWorldItemHeightAt(coords) == 0)
        {
            currentPosition += targetDirection * (Time.deltaTime * movementSpeed * _currenRunningMultiplier);
            if (Math.Abs(heightDifference) > .0001f)
            {
                currentPosition += (heightDifference + colliderBounds.extents.y / 2 + yOffset) * Time.deltaTime * 18 *
                                   Vector3.up;
            }
        }

        currentTransform.position = currentPosition;
    }

    public void OnMoveForwardButton(InputValue input)
    {
        _isMoveForward = input.Get<float>() > .8f;
    }
    
    public void OnMoveForwardDirectional(InputValue input)
    {
        var directionalInput = input.Get<Vector2>();
        _targetMoveDirection = new Vector3(-directionalInput.y, 0, directionalInput.x);
        if (_isLookInMoveDirection)
        {
            _targetLookDirection = _targetMoveDirection;
        }
    }

    public void OnLookAround(InputValue context)
    {
        var directionalInput = context.Get<Vector2>();
        if (directionalInput.sqrMagnitude == 0f)
        {
            _isLookInMoveDirection = true;
        }
        else
        {
            _isLookInMoveDirection = false;
            _nextLookInMoveDirectionTime = Time.time + LookInMoveDirectionGraceTime;
            _targetLookDirection = new Vector3(-directionalInput.y, 0, directionalInput.x);
        }
    }

    public void OnMouseLookAround(InputValue context)
    {
        if (_pauseActivator.IsGamePaused()) return;

        var mousePosition = context.Get<Vector2>();

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 5000f, _mouseTargetLayerMask))
        {
            var lookAtPoint = hit.point;
            var directionalTransform = playerDirectional.transform;
            var position = directionalTransform.position;
            lookAtPoint.y = position.y;
            _targetLookDirection = (lookAtPoint - position).normalized;
        }

        _isLookInMoveDirection = false;
        _nextLookInMoveDirectionTime = Time.time + LookInMoveDirectionGraceTime * 2;
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

    public void OnDash(InputValue context)
    {
        if (!isDashing && Time.time > nextRunningTime)
        {
            if (context.isPressed)
            {
                isDashing = true;
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
            GainHealth(1);
        }
    }

    public void OnDebugHealthDown(InputValue context)
    {

        if (currentHealth >= 0)
        {
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
        if(!ownsGrapplingHook)
            return;
            
        if (grappling || nextGrappleTime < grappleCooldownTime)
        {
            return;
        }

        if (context.isPressed)
        {
            // when pressed the grapple hook spins, winding up for throw
        }
        // when released the grapple is thrown towards grapplePoint
        // grapplehook hits terrain it can stick to?
        // Yes it stops, player moves towards hook
        // No: it comes back to player

        //On Right Click Raycast from mouse to find collider

        RaycastHit[] hits = Physics.RaycastAll(playerDirectional.transform.position,
            playerDirectional.transform.forward, grappleDistance);
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
        var currentTransform = playerDirectional.transform;
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