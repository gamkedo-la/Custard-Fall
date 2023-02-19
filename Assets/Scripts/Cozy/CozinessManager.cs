using System.Collections.Generic;
using UnityEngine;

public class CozinessManager : MonoBehaviour
{
    [SerializeField] private List<CozinessReceiver> receivers;

    [SerializeField] private float searchRadius = 10f;

    private readonly Dictionary<CozinessReceiver, HashSet<CozyDispenser>> receivers2Dispensers = new();
    private readonly List<CozyDispenser> registeredDispensers = new();

    #region Singleton

    public static CozinessManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("More than one instance of CozinessManager found!");
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

    public void RegisterDispenser(CozyDispenser cozyDispenser)
    {
        registeredDispensers.Add(cozyDispenser);
    }

    public void UnregisterDispenser(CozyDispenser cozyDispenser)
    {
        registeredDispensers.Remove(cozyDispenser);
    }

    private void FixedUpdate()
    {
        // clean all receivers not any longer near the receiver
        foreach (var receiver2Dispensers in receivers2Dispensers)
        {
            var receiver = receiver2Dispensers.Key;
            var receiverPosition = receiver.transform.position;
            var nearDispensers = receiver2Dispensers.Value;
            var leavingDispensers = new HashSet<CozyDispenser>();
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
                receiver.OnCozyLeave(dispenser.Coziness);
            }
        }


        foreach (var receiver in receivers)
        {
            var transformPosition = receiver.transform.position;
            foreach (var dispenser in registeredDispensers)
            {
                if (Vector3.Distance(transformPosition, dispenser.transform.position) <= searchRadius)
                {
                    var nearDispensers = receivers2Dispensers.GetValueOrDefault(receiver);
                    if (!nearDispensers.Contains(dispenser))
                    {
                        nearDispensers.Add(dispenser);
                        receiver.OnCozyReceive(dispenser.Coziness);
                    }
                }
            }
        }
    }
}