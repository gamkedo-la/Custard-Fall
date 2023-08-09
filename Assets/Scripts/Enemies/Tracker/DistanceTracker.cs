using System.Collections;
using UnityEngine;


public class DistanceTracker : MonoBehaviour
{
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private float maxDistanceExit = 8f;
    [SerializeField] private float offset = 2f;
    [SerializeField] private float offsetExit = 0f;
    [SerializeField] private float checkIntervalEnter = .4f;
    [SerializeField] private float checkIntervalExit = .3f;

    [SerializeField] private int maxHeightAbove = 0;
    [SerializeField] private int maxHeightBelow = 1;

    public WorldCells worldCells;

    public delegate void OnTargetEnter();

    public OnTargetEnter onTargetEnter;

    public delegate void OnTargetExit();

    public OnTargetExit onTargetExit;


    private GameObject _target;
    private bool _inRange = false;

    private void Awake()
    {
        _target = FindObjectOfType<Player>().gameObject;
    }

    private void Start()
    {
        StartCoroutine(MaybeTriggerDistanceEvent());
    }

    private IEnumerator MaybeTriggerDistanceEvent()
    {
        while (true)
        {
            yield return new WaitForSeconds(_inRange ? checkIntervalExit : checkIntervalEnter);
            var targetPosition = _target.transform.position;
            var position = transform.TransformPoint(Vector3.forward * (_inRange ? offsetExit : offset));
            var inRange = Vector3.Distance(position, targetPosition) < GetMaxDistance();

            if (inRange)
            {
                var difference = GetHeightDifference(position, targetPosition);
                if (difference > maxHeightAbove || difference < -maxHeightBelow)
                {
                    inRange = false;
                }
            }

            if (!_inRange && inRange)
            {
                var difference = GetHeightDifference(position, targetPosition);
                if (difference <= maxHeightAbove && difference >= -maxHeightBelow)
                {
                    onTargetEnter?.Invoke();
                }
                else
                {
                    continue;
                }
            }
            else if (_inRange && !inRange)
            {
                onTargetExit?.Invoke();
            }

            _inRange = inRange;
        }
    }

    private float GetMaxDistance()
    {
        return _inRange ? maxDistanceExit : maxDistance;
    }

    private int GetHeightDifference(Vector3 position, Vector3 targetPosition)
    {
        var coords = worldCells.GetCellPosition(position);
        var targetCoords = worldCells.GetCellPosition(targetPosition);
        var difference = worldCells.GetHeightAt(targetCoords) - worldCells.GetHeightAt(coords);
        return difference;
    }
}