using System;
using System.Collections;
using Custard;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public WorldCells worldCells;
    public Inhaler inhaler;

    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform activePoint;

    private AudioSource inhaleSFX;

    [SerializeField] private CustardState custardState;

    [SerializeField] private GameObject playerDirectional;
    public float movementSpeed = 4;
    [SerializeField] private float swimSpeed = 3.4f;
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

    [SerializeField] private GameObject buttonPrompt;

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

    private RadianceReceiver _radianceReceiver;
    [SerializeField] private CombinatorProfileSO combinatorProfile;

    [SerializeField] private bool isSwimming;

    // yOffset represents local terrain detail the player can stand on, so they are not clipped to round numbers
    [SerializeField] private float yOffset = -.6f;
    [SerializeField] private float placementYOffset = .6f;
    private Collider _collider;

    public InputControlScheme gameplayScheme;

    public GameObject itemInHand;

    [SerializeField] private PlaceableItem placeModeItemReference;

    private Vector3 targetPoint4PlacingItem;
    private Vector3 _smoothPreviewPosition;
    private Vector3 _velSmoothPreviewPosition = Vector3.zero;

    private bool _isAtEdge;

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

        _radianceReceiver = GetComponent<RadianceReceiver>();
    }

    private void FixedUpdate()
    {
        nextGrappleTime += Time.fixedDeltaTime;

        Vector3 playerPosition = activePoint.position;
        if (!isDashing && !grappling && !isSwimming)
        {
            playerAnimator.SetBool(Grappling, false);
            if (placeModeItemReference)
            {
                // player in place mode state
                targetPoint4PlacingItem =
                    FindNearestPlaceModeItemPosition(playerPosition, playerDirectional.transform.forward,
                        out ItemReceiver itemReceiver);
                _smoothPreviewPosition = Vector3.SmoothDamp(_smoothPreviewPosition, targetPoint4PlacingItem,
                    ref _velSmoothPreviewPosition, 0.042f);
                itemInHand.transform.position = _smoothPreviewPosition;
                UpdatePlaceableItemState(playerPosition, itemReceiver);
            }
            else if (itemInHand)
            {
                itemInHand.SetActive(false);
            }
        }

        if (grappling)
        {
            grappleMarker.SetActive(false);
            var positionBefore = transform.position;
            var realGrapplePoint = new Vector3(grapplePoint.x,positionBefore.y, grapplePoint.z);
            transform.position =
                Vector3.MoveTowards(positionBefore, realGrapplePoint, grappleSpeed * Time.fixedDeltaTime);
            grappleLine.SetPosition(0, grappleLine.gameObject.transform.position);
            if (Vector3.Distance(transform.position, realGrapplePoint) < 0.01f)
            {
                grappling = false;
                playerAnimator.SetBool(Grappling, false);
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
        else
        {
            MovePlayer(Vector3.zero);
            playerAnimator.SetBool(Walking, false);
        }
    }

    public void TakeDamage(int damage, DamageImplication implication)
    {
        if (implication == DamageImplication.Health ||
            _radianceReceiver.PersonalRadianceLevel == 0 &&
            _radianceReceiver.RadianceTillNextLevel <= 0.01f)
        {
            currentHealth -= damage;
            healthbar.SetHealth(currentHealth);

            if (currentHealth <= 0)
            {
                DieAndRespawn();
            }
        }
        else
        {
            _radianceReceiver.TakeDamage(damage / (float) maxHealth);
        }
    }

    [ContextMenu("DieAndRespawn")]
    void DieAndRespawn()
    {
        playerAnimator.SetBool(Dead, true);
        _pauseActivator.PauseGameSilently();

        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSecondsRealtime(2);
        MusicManager.Instance.SetUnder(false);
        var respawnPointPosition = respawnPoint.position;
        var cellPosition = worldCells.GetCellPosition(respawnPointPosition);
        float yOnSurface = worldCells.GetHeightAt(cellPosition);
        yOnSurface += _collider.bounds.extents.y * 1.5f + yOffset;
        transform.position = new Vector3(respawnPointPosition.x, yOnSurface, respawnPointPosition.z);
        ResetPlayerState();

        _pauseActivator.UnPauseGame();
        playerAnimator.SetBool(Dead, false);
    }


    private void ResetPlayerState()
    {
        currentHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);
        isDashing = false;
        grappling = false;
        _currenRunningMultiplier = 1f;
        _velDash = 0;
        _velLook = Vector3.zero;
        _velMove = Vector3.zero;
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

        if (isSwimming)
        {
            // terrainHeight: currently out of bounds of terrain height check is coded as 255 value (int max)
            var terrainHeight = worldCells.GetHeightAt(coords);
            // player cannot scale high ground
            // player can only leap from a high point if running
            if (terrainHeight != 255)
            {
                var custardLevel = custardState.GetCurrentCustardLevelAt(coords);
                var heightDifference = terrainHeight + custardLevel - colliderBounds.min.y;
                var thresholdHeight = 1.5f;
                if (heightDifference < thresholdHeight || custardLevel > 0)
                {
                    currentPosition += targetDirection * (Time.deltaTime * swimSpeed * _currenRunningMultiplier);
                    if (Math.Abs(heightDifference) > .0001f)
                    {
                        currentPosition += (heightDifference + colliderBounds.extents.y / 2 + yOffset) *
                                           Time.deltaTime * 18 *
                                           Vector3.up;
                        if (custardLevel <= 1)
                        {
                            EnterSwimMode(false);
                        }
                    }

                    currentTransform.position = currentPosition;
                }
            }
        }
        else
        {
            // terrainHeight: currently out of bounds of terrain height check is coded as 255 value (int max)
            var terrainHeight = worldCells.GetHeightAt(coords);
            var custardLevel = custardState.GetCurrentCustardLevelAt(coords);
            var heightDifference = terrainHeight - colliderBounds.min.y;

            if (terrainHeight != 255 && (isDashing ? heightDifference : Math.Abs(heightDifference)) < 2.5f ||
                custardLevel > 0 && Math.Abs(heightDifference + custardLevel) < 1.5)
            {
                currentPosition += targetDirection * (Time.deltaTime * movementSpeed * _currenRunningMultiplier);
                if (Math.Abs(heightDifference) > .0001f)
                {
                    currentPosition +=
                        (heightDifference + (custardLevel > 1 ? custardLevel : 0) + colliderBounds.extents.y / 2 +
                         yOffset) * Time.deltaTime *
                        18 *
                        Vector3.up;
                    if (custardLevel > 1)
                    {
                        EnterSwimMode(true);
                    }
                    else
                    {
                        playerAnimator.SetBool(Walking, true);
                    }
                }
            }
            else
            {
                playerAnimator.SetBool(Walking, true);
                // display leap of faith
                switch (_isAtEdge)
                {
                    case false when heightDifference < -2.5f || heightDifference + custardLevel < -1.25f:
                    {
                        Debug.Log($"height difference: {heightDifference}");
                        _isAtEdge = true;
                        buttonPrompt.SetActive(true);
                        var fadeInThenOutCanvasGroup = buttonPrompt.GetComponent<FadeInThenOutCanvasGroup>();
                        if (!fadeInThenOutCanvasGroup)
                        {
                            buttonPrompt.AddComponent<FadeInThenOutCanvasGroup>();
                        }
                        else
                        {
                            fadeInThenOutCanvasGroup.KeepItUp();
                        }

                        break;
                    }
                    case true:
                        _isAtEdge = false;
                        break;
                }
            }

            currentTransform.position = currentPosition;
        }
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
        if (context.isPressed)
        {
            inhaler.BeginInhaleInTransformDirection(4f);
            inhaleSFX.Play();

            if (itemInHand != null)
            {
                PlaceItemInHand();
            }
        }
        else
        {
            inhaler.StopInhale();
            inhaleSFX.Stop();
            requireUseButtonRelease = false;
        }
    }

    public void OnDash(InputValue context)
    {
        if (!isDashing && !isSwimming && Time.time > nextRunningTime && !placeModeItemReference)
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
        if (currentHealth <= 3)
        {
            GainHealth(1);
        }
    }

    public void OnDebugHealthDown(InputValue context)
    {
        if (currentHealth >= 0)
        {
            TakeDamage(1, DamageImplication.RadianceThenHealth);
        }
    }

    public void OnUseItem(InputValue context)
    {
        if (context.isPressed)
        {
            requireUseButtonRelease = false;
        }
        else
        {
            // on release
            if (itemInHand != null)
                PlaceItemInHand();
        }
    }

    public static EventHandler<EventArgs> grapplePressed;
    public static EventHandler<EventArgs> grappleReleased;
    [SerializeField] private float maxPlaceDistance = 3f;
    [SerializeField] private float placeAtHigherLevelThreshold = .75f;
    [SerializeField] private float placeAtHigherLevelDistanceModifier = 1f;
    private ItemReceiver focusedItemReceiver;
    private Func<bool> canPlaceMoreCheckFunc;
    private Func<bool> onItemPlaced;
    private bool requireUseButtonRelease;

    private static readonly int Dead = Animator.StringToHash("dead");
    private static readonly int Swimming = Animator.StringToHash("swimming");
    private static readonly int Walking = Animator.StringToHash("walking");
    private static readonly int Grappling = Animator.StringToHash("grappling");

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
            grapplePressed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // this can only run when the OnGrapple is called and the button isn't pressed => release state
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
        playerAnimator.SetBool(Grappling, true);
    }

    private bool UpdateGrapplePoint()
    {
        var playerPosition = activePoint.position;
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
                grapplePoint = new Vector3(hit.point.x, playerPosition.y, hit.point.z);
                break;
            }
        }

        return hitSuccess;
    }

    private void PlaceItemInHand()
    {
        if (requireUseButtonRelease)
        {
            return;
        }

        var playerPosition = playerDirectional.transform.position;
        // the player cannot scale high ground
        if (UpdatePlaceableItemState(playerPosition, focusedItemReceiver))
        {
            onItemPlaced?.Invoke();

            if (focusedItemReceiver == null)
            {
                Instantiate(placeModeItemReference.Prototype, targetPoint4PlacingItem, Quaternion.identity);
            }
            else
            {
                focusedItemReceiver.ReceiveItem(placeModeItemReference);
            }

            if (canPlaceMoreCheckFunc())
            {
                requireUseButtonRelease = true;
            }
            else
            {
                ExitPlaceMode();
            }
        }
    }

    private void ExitPlaceMode()
    {
        Destroy(itemInHand);
        itemInHand = null;
        canPlaceMoreCheckFunc = null;
        placeModeItemReference = null;
        focusedItemReceiver?.LeavePreview();
        focusedItemReceiver = null;
        requireUseButtonRelease = false;
    }

    public void EnterPlaceMode(PlaceableItem item, Func<bool> canPlaceMoreCheck, Func<bool> onItemPlaced)
    {
        Debug.Log("Enter place mode with " + item.ResourceName);
        placeModeItemReference = item;
        var playerDirectionalTransform = playerDirectional.transform;
        var playerPosition = playerDirectionalTransform.position;
        targetPoint4PlacingItem =
            FindNearestPlaceModeItemPosition(playerPosition, playerDirectionalTransform.forward,
                out ItemReceiver itemReceiver);

        itemInHand = Instantiate(item.PlaceablePreview, targetPoint4PlacingItem, Quaternion.identity);
        canPlaceMoreCheckFunc = canPlaceMoreCheck;
        this.onItemPlaced = onItemPlaced;
        _smoothPreviewPosition = targetPoint4PlacingItem;
        UpdatePlaceableItemState(playerPosition, itemReceiver);
    }

    private bool UpdatePlaceableItemState(Vector3 playerPosition, ItemReceiver itemReceiver)
    {
        if (focusedItemReceiver != null && itemReceiver != focusedItemReceiver)
        {
            focusedItemReceiver.LeavePreview();
            focusedItemReceiver = null;
        }

        var possible = Vector2.Distance(new Vector2(playerPosition.x, playerPosition.z),
            new Vector2(targetPoint4PlacingItem.x, targetPoint4PlacingItem.z)) >= .3f;
        if (possible)
        {
            if (itemReceiver == null)
            {
                itemInHand.SetActive(true);
            }
            else
            {
                itemInHand.SetActive(false);
                itemReceiver.PreviewReceiveItem(placeModeItemReference);
                focusedItemReceiver = itemReceiver;
            }
        }
        else
        {
            itemInHand.SetActive(false);
        }

        return possible;
    }

    private Vector3 FindNearestPlaceModeItemPosition(Vector3 position, Vector3 direction, out ItemReceiver itemReceiver)
    {
        Vector3 tmpTargetPoint4PlacingItem;
        itemReceiver = null;

        var debugVectorVisual = DebugUtils.ProvideTransform("player transform");
        debugVectorVisual.position = position;
        debugVectorVisual.forward = direction.normalized;

        if (Physics.Raycast(position, direction, out var hitResult, maxPlaceDistance,
                LayerMask.GetMask("Terrain", "Obstacles", "Interactable")))
        {
            bool blockedByOtherItem = false;
            var layerName = LayerMask.LayerToName(hitResult.transform.gameObject.layer);
            if (layerName == "Interactable")
            {
                blockedByOtherItem = true;
                // set output
                itemReceiver =
                    combinatorProfile.CanCombine(placeModeItemReference, hitResult.transform.position + Vector3.up);
            }

            var obstacleDistance = Vector2.Distance(new Vector2(position.x, position.z),
                new Vector2(hitResult.point.x, hitResult.point.z));
            if (blockedByOtherItem || obstacleDistance >= maxPlaceDistance - placeAtHigherLevelThreshold)
            {
                tmpTargetPoint4PlacingItem = hitResult.point - direction * .9f;
            }
            else
            {
                tmpTargetPoint4PlacingItem =
                    position + direction * (maxPlaceDistance - placeAtHigherLevelDistanceModifier);
            }
        }
        else
        {
            tmpTargetPoint4PlacingItem = position + direction * maxPlaceDistance;
        }

        Coords coords = worldCells.GetCellPosition(tmpTargetPoint4PlacingItem.x, tmpTargetPoint4PlacingItem.z);
        var heightAtTarget = worldCells.GetHeightAt(coords);
        Coords coordsReference = worldCells.GetCellPosition(position.x, position.z);
        var heightAtReference = worldCells.GetHeightAt(coordsReference);
        if (heightAtTarget >= heightAtReference - 1 && heightAtTarget <= heightAtReference + 2)
        {
            var cellBasedPosition = worldCells.GetWorldPosition(coords);
            return new Vector3(cellBasedPosition.x,
                heightAtTarget + placementYOffset, cellBasedPosition.y);
        }
        else
        {
            return new Vector3(position.x, heightAtTarget + placementYOffset, position.z);
        }
    }

    public void EnterSwimMode(bool doSwim)
    {
        if (grappling)
            return;

        isSwimming = doSwim;
        if (isSwimming)
        {
            ExitPlaceMode();
        }

        playerAnimator.SetBool(Swimming, doSwim);
        playerAnimator.SetBool(Walking, false);
    }
}

public enum DamageImplication
{
    Health,
    Radiance,
    RadianceThenHealth
}