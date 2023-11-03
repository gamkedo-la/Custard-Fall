using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RadianceManager : MonoBehaviour
{
    [SerializeField] private List<RadianceReceiver> receivers;

    [SerializeField] private float searchRadius = 10f;

    private readonly Dictionary<RadianceReceiver, HashSet<RadianceDispenser>> receivers2Dispensers = new();
    private readonly List<RadianceDispenser> registeredDispensers = new();

    #region Singleton

    public static RadianceManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("More than one instance of RadianceManager found!");
            return;
        }

        Instance = this;
    }

    #endregion

    private void Start()
    {
        foreach (var receiver in receivers)
        {
            receivers2Dispensers.Add(receiver, new());
        }
    }

    public void RegisterDispenser(RadianceDispenser radianceDispenser)
    {
        registeredDispensers.Add(radianceDispenser);
    }

    public void UnregisterDispenser(RadianceDispenser radianceDispenser)
    {
        registeredDispensers.Remove(radianceDispenser);
        foreach (var pair in receivers2Dispensers)
        {
            pair.Value.Remove(radianceDispenser);
        }
    }

    private void FixedUpdate()
    {
        // clean all receivers not any longer near the receiver
        foreach (var receiver2Dispensers in receivers2Dispensers)
        {
            var receiver = receiver2Dispensers.Key;
            var receiverPosition = receiver.transform.position;
            var nearDispensers = receiver2Dispensers.Value;
            var leavingDispensers = new HashSet<RadianceDispenser>();
            foreach (var dispenser in nearDispensers)
            {
                if (Vector3.Distance(receiverPosition, dispenser.transform.position) > searchRadius)
                {
                    leavingDispensers.Add(dispenser);
                }
            }

            foreach (var dispenser in leavingDispensers)
            {
                nearDispensers.Remove(dispenser);
            }
        }

        foreach (var receiver in receivers)
        {
            float maxOfRadiance = 0;
            var transformPosition = receiver.transform.position;
            foreach (var dispenser in registeredDispensers)
            {
                if (!(Vector3.Distance(transformPosition, dispenser.transform.position) <= searchRadius))
                    continue;

                var nearDispensers = receivers2Dispensers.GetValueOrDefault(receiver);
                if (!nearDispensers.Contains(dispenser))
                {
                    nearDispensers.Add(dispenser);
                }

                maxOfRadiance = Mathf.Max(maxOfRadiance, dispenser.Radiance);
            }

            receiver.UpdateRadianceZoneLevel((int) maxOfRadiance);
        }
    }

    public float GetTotalRadiance(RadianceReceiver receiver)
    {
        if (!receivers2Dispensers.TryGetValue(receiver, out HashSet<RadianceDispenser> dispensers))
        {
            return 0;
        }
        else
        {
            return dispensers.Sum(e => e.Radiance);
        }
    }
}