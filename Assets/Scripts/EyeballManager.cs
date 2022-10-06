using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeballManager : MonoBehaviour
{
  [SerializeField] private Transform player;
  [SerializeField] private Custard.CustardManager custardManager;
  [SerializeField] private TimeManager timeManager;
  [SerializeField] private List<Eyeball> eyeballs;
  [SerializeField] private GameObject EyeballPrefab;


  [Tooltip("Minimum distance from player that eyeballs will surface. They will dive back down if the player gets too close.")]
  [SerializeField] private float minDistanceToPlayer = 2.5f;

  [Tooltip("Maximum distance from player that eyeballs will surface. They will dive back down if the player gets too far.")]
  [SerializeField] private float maxDistanceToPlayer = 10f;

  [Tooltip("Minimum depth of custard that eyeballs will surface.")]
  [SerializeField] private int minCustardDepth = 2;

  [Tooltip("Maximum number of eyeballs that will gaze up at you.")]
  [SerializeField] private int numberOfEyeballs = 3;

  [Tooltip("When an eyeball dives, it will stay under the custard for at least this amount of time before surfacing again.")]
  [SerializeField] private float timeBeforeResurface = 2f;

  [Tooltip("Minimum amount of time between any eyeball surfacing.")]
  [SerializeField] private float timeBetweenSurfaces = 3f;

  [Tooltip("Check this to use the custom \"awake\" times below for eyeballs. Otherwise, they will only surface at night based on the TimeManager.")]
  [SerializeField] private bool useCustomAwakeTimes = false;

  [Tooltip("The time of day when eyeballs \"wake up\" and start surfacing. (\"Use Custom Awake Times\" must be checked)")]
  [SerializeField] private float wakeupTime = .65f;

  [Tooltip("The time of day when eyeballs \"go to sleep\" and stop surfacing. (\"Use Custom Awake Times\" must be checked)")]
  [SerializeField] private float fallAsleepTime = .35f;

  private float timeUntilNextSurface = 0f;


  void Start()
  {
    CreateEyeballs();
  }

  void Update()
  {
    // Eyeballs with surface if it's the right time of day and they are far enough from the player.
    SurfaceEyeballsIfPossible();
    // Eyeballs will dive if they are too close to the player or if the custard level changes beneath them or if it's "bed" time.
    DiveEyeballsIfNeccessary();
  }

  private void CreateEyeballs()
  {
    for (int i = 0; i < numberOfEyeballs; i++)
    {
      Eyeball eyeball = Instantiate(EyeballPrefab, transform).GetComponent<Eyeball>();
      eyeball.target = player;
      eyeballs.Add(eyeball);
    }
  }

  private bool IsNight()
  {
    return !timeManager.IsDayTime;
  }

  private void SurfaceEyeballsIfPossible()
  {
    if (EyeballsAreAwake())
    {
      timeUntilNextSurface -= Time.deltaTime;

      if (timeUntilNextSurface <= 0f)
      {
        for (int i = 0; i < eyeballs.Count; i++)
        {
          Eyeball eyeball = eyeballs[i];

          if (!eyeball.IsSurfaced())
          {
            Vector3 possibleEyeballPosition = FindRandomPositionInRangeOfPlayer();
            Coords possibleCoords = custardManager.worldCells.GetCellPosition(possibleEyeballPosition.x, possibleEyeballPosition.z);
            int custardAmount = custardManager.custardState.GetCurrentCustardLevelAt(possibleCoords);

            bool custardIsDeepEnough = custardAmount >= minCustardDepth;
            bool noOtherEyeballsAreThere = NoOtherEyeballAtCoords(possibleCoords);
            bool doveLongEnoughAgo = DoveLongEnoughAgo(eyeball);

            if (custardIsDeepEnough && noOtherEyeballsAreThere && doveLongEnoughAgo)
            {
              float custardHeight = custardManager.worldCells.GetTerrainHeightAt(possibleCoords) + custardAmount;
              Vector2 position = custardManager.worldCells.GetWorldPosition(possibleCoords);

              eyeball.transform.position = new Vector3(position.x, custardHeight, position.y);
              eyeball.coords = possibleCoords;
              eyeball.custardAmount = custardAmount;
              eyeball.targetHeight = custardHeight;
              eyeball.Surface();

              timeUntilNextSurface = timeBetweenSurfaces;
              break;
            }
          }
        }
      }
    }
  }

  private void DiveEyeballsIfNeccessary()
  {
    for (int i = 0; i < eyeballs.Count; i++)
    {
      Eyeball eyeball = eyeballs[i];

      if (eyeball.IsSurfaced())
      {
        float distanceToPlayer = Vector3.Distance(player.position, eyeball.transform.position);
        bool playerIsTooCloseOrTooFar = !IsPlayerInRange(distanceToPlayer);

        int custardAmount = custardManager.custardState.GetCurrentCustardLevelAt(eyeball.coords);
        bool custardAmountHasChanged = custardAmount != eyeball.custardAmount;

        if (playerIsTooCloseOrTooFar || !EyeballsAreAwake())
        {
          eyeball.Dive();
        }
        else if (custardAmountHasChanged)
        {
          eyeball.targetHeight = custardAmount;
          eyeball.Dive();
        }
      }
    }
  }

  private Vector3 FindRandomPositionInRangeOfPlayer()
  {
    float randomDistance = Random.Range(minDistanceToPlayer, maxDistanceToPlayer);
    float randomAngle = Random.Range(0, Mathf.PI * 2);
    Vector3 randomVector = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle)) * randomDistance;
    return player.position + randomVector;
  }

  private bool EyeballsAreAwake()
  {
    if (useCustomAwakeTimes)
    {
      if (wakeupTime < fallAsleepTime)
      {
        return timeManager.time >= wakeupTime && timeManager.time <= fallAsleepTime;
      }
      else
      {
        return timeManager.time >= wakeupTime || timeManager.time <= fallAsleepTime;
      }
    }

    return IsNight();
  }

  private bool DoveLongEnoughAgo(Eyeball eyeball)
  {
    return Time.time - eyeball.timeDove > timeBeforeResurface;
  }

  private bool NoOtherEyeballAtCoords(Coords coords)
  {
    for (int i = 0; i < eyeballs.Count; i++)
    {
      Eyeball eyeball = eyeballs[i];
      if (eyeball.IsSurfaced() && eyeball.coords.Equals(coords))
      {
        return false;
      }
    }

    return true;
  }

  private bool IsPlayerInRange(float distanceToPlayer)
  {
    return distanceToPlayer >= minDistanceToPlayer && distanceToPlayer <= maxDistanceToPlayer;
  }
}
