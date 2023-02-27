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
    [SerializeField] private float LookInMoveDirectionGraceTime = .35f;
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

    [SerializeField] private GameObject grappleMarker;
    private Vector3 grapplePoint;
    [SerializeField] private LineRenderer grappleLine;


    [SerializeField] public bool ownsGrapplingHook;
    bool grappling;
    private bool grappleIntoTheVoid;
    private bool _isMoveForward;

    private CozinessReceiver cozinessReceiver;


    // yOffset represents local terrain detail the player can stand on, so they are not clipped to round numbers
    private float yOffset = -.05f;
    private Collider _collider;

    public InputControlScheme gameplayScheme;

    public GameObject itemInHand;

    [SerializeField] private PlaceableItem placeModeItemReference;

    private Vector3 targetPoint4PlacingItem;
    private Vector3 _smoothPreviewPosition;
    private Vector3 _velSmoothPreviewPosition = Vector3.zero;


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
        grappleLine.gameObject.SetActive(false);

        currentHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);
        inhaler.owner = gameObject;

        cozinessReceiver = GetComponent<CozinessReceiver>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void FixedUpdate()
    {
        nextGrappleTime += Time.fixedDeltaTime;

        Vector3 playerPosition = transform.position;
        if (!isDashing && !grappling)
        {
            if (placeModeItemReference)
            {
                // player in place mode state
                targetPoint4PlacingItem =
                    FindNearestPlaceModeItemPosition(playerPosition, playerDirectional.transform.forward);
                _smoothPreviewPosition = Vector3.SmoothDamp(_smoothPreviewPosition, targetPoint4PlacingItem,
                    ref _velSmoothPreviewPosition, 0.042f);
                itemInHand.transform.position = _smoothPreviewPosition;
                UpdatePlaceableItemState(playerPosition);
            }
            else if (itemInHand)
            {
                itemInHand.SetActive(false);
            }
        }

        if (grappling)
        {
            grappleMarker.SetActive(false);
            transform.position =
                Vector3.MoveTowards(playerPosition, grapplePoint, grappleSpeed * Time.fixedDeltaTime);
            grappleLine.SetPosition(0, grappleLine.gameObject.transform.position);
            if (Vector3.Distance(transform.position, grapplePoint) < 0.01f)
            {
                grappling = false;
                grappleLine.gameObject.SetActive(false);
            }

            return;
        }
        else if (ownsGrapplingHook)
        {
            var hit = UpdateGrapplePoint();
            if (hit)
            {
                grappleMarker.transform.position = grapplePoint - transform.forward + Vector3.up * .5f;
                grappleMarker.SetActive(true);
            }
            else if (grappleIntoTheVoid)
            {
                grappleLine.gameObject.SetActive(true);
                grappleIntoTheVoid = false;
            }
            else
            {
                grappleLine.gameObject.SetActive(false);
                grappleMarker.SetActive(false);
            }
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
        if (cozinessReceiver.PersonalCozyLevel == 0 && cozinessReceiver.CozinessTillNextLevel <= 0.01f)
        {
            currentHealth -= damage;
            healthbar.SetHealth(currentHealth);

            if (currentHealth <= 0)
            {
                Respawn();
            }
        }
        else
        {
            cozinessReceiver.TakeDamage(damage / (float)maxHealth);
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
            worldCells.GetWorldItemHeightAt(coords) <= 1)
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
        _nextLookInMoveDirectionTime = Time.time + LookInMoveDirectionGraceTime * 3 + .5f;
    }

    public void OnInhale(InputValue context)
    {
        if (placeModeItemReference)
        {
            PlaceItemInHand();
        }

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
        if (!isDashing && Time.time > nextRunningTime && !placeModeItemReference)
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

    public static EventHandler<EventArgs> grapplePressed;
    public static EventHandler<EventArgs> grappleReleased;

    public void OnGrapple(InputValue context) // InputAction.CallbackContext context
    {
        if (placeModeItemReference)
        {
            // cancel place mode
            ExitPlaceMode();
        }

        if (!ownsGrapplingHook) return; // maybe play null sound effect
        if (grappling || nextGrappleTime < grappleCooldownTime)
        {
            return;
        }

        if (context.isPressed)
        {
            Debug.Log("grapple pressed");
            grapplePressed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // this can only run when the OnGrapple is called and the button isn't pressed => release state
            Debug.Log("grapple released");
            grappleReleased?.Invoke(this, EventArgs.Empty);
        }
        // when released the grapple is thrown towards grapplePoint
        // grapplehook hits terrain it can stick to?
        // Yes it stops, player moves towards hook
        // No: it comes back to player

        //On Right Click Raycast from mouse to find collider

        var hitSuccess = UpdateGrapplePoint();

        grappleIntoTheVoid = false;
        grappleLine.SetPosition(0, grappleLine.gameObject.transform.position);
        grappleLine.gameObject.SetActive(true);

        if (hitSuccess)
        {
            grappling = true;
            nextGrappleTime = 0;
            grappleLine.SetPosition(1, grapplePoint);
        }
        else
        {
            grappleIntoTheVoid = true;
            grappleLine.SetPosition(1, transform.position + playerDirectional.transform.forward * grappleDistance);
        }
    }

    private bool UpdateGrapplePoint()
    {
        var playerPosition = playerDirectional.transform.position;
        RaycastHit[] hits = Physics.RaycastAll(playerPosition,
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

                hitSuccess = true;
                grapplePoint = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                break;
            }
        }

        return hitSuccess;
    }

    private void PlaceItemInHand()
    {
        var playerPosition = playerDirectional.transform.position;
        // the player cannot scale high ground
        if (UpdatePlaceableItemState(playerPosition))
        {
            Instantiate(placeModeItemReference.Prototype, targetPoint4PlacingItem, Quaternion.identity);
            ExitPlaceMode();
        }
    }

    public void ExitPlaceMode()
    {
        Destroy(itemInHand);
        itemInHand = null;
        placeModeItemReference = null;
    }

    public void EnterPlaceMode(PlaceableItem item)
    {
        placeModeItemReference = item;
        var playerDirectionalTransform = playerDirectional.transform;
        var playerPosition = playerDirectionalTransform.position;
        targetPoint4PlacingItem =
            FindNearestPlaceModeItemPosition(playerPosition, playerDirectionalTransform.forward);

        itemInHand = Instantiate(item.PlaceablePreview, targetPoint4PlacingItem, Quaternion.identity);
        _smoothPreviewPosition = targetPoint4PlacingItem;
        UpdatePlaceableItemState(playerPosition);
    }

    private bool UpdatePlaceableItemState(Vector3 playerPosition)
    {
        var possible = Vector2.Distance(new Vector2(playerPosition.x, playerPosition.z),
            new Vector2(targetPoint4PlacingItem.x, targetPoint4PlacingItem.z)) >= 1f;
        itemInHand.SetActive(possible);
        return possible;
    }

    private Vector3 FindNearestPlaceModeItemPosition(Vector3 position, Vector3 direction)
    {
        Vector3 tmpTargetPoint4PlacingItem;

        var maxDistance = 3f;
        if (Physics.Raycast(position, direction, out var hitResult, maxDistance,
                LayerMask.GetMask("Terrain", "Obstacles")))
        {
            tmpTargetPoint4PlacingItem = hitResult.point - direction * .9f;
        }
        else
        {
            tmpTargetPoint4PlacingItem = position + direction * maxDistance;
        }

        Coords coords = worldCells.GetCellPosition(tmpTargetPoint4PlacingItem.x, tmpTargetPoint4PlacingItem.z);
        var heightAtTarget = worldCells.GetHeightAt(coords);
        Coords coordsReference = worldCells.GetCellPosition(position.x, position.z);
        var heightAtReference = worldCells.GetHeightAt(coordsReference);
        if (heightAtTarget == heightAtReference)
        {
            var cellBasedPosition = worldCells.GetWorldPosition(coords);
            return new Vector3(cellBasedPosition.x, tmpTargetPoint4PlacingItem.y - .55f, cellBasedPosition.y);
        }
        else
        {
            return position;
        }
    }
}