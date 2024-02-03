using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactToRadianceLevel : MonoBehaviour
{
    private RadianceReceiver _radianceReceiver;
    private Player _player;

    [SerializeField] private List<LevelModification> modifications;

    private float baseSpeed;

    private void Awake()
    {
        _radianceReceiver = GetComponent<RadianceReceiver>();
        _player = GetComponent<Player>();
        _radianceReceiver.onLevelChange += ReactToChangeInRadiance;
    }

    private void Start()
    {
        baseSpeed = _player.movementSpeed;
    }

    private void ReactToChangeInRadiance(int newlevel, int previouslevel)
    {
        var modification = modifications[newlevel];
        if (modification == null)
        {
            modification = modifications[^1];
        }

        if (modification != null)
        {
            _player.movementSpeed = baseSpeed * modification.SpeedMultiplier;
        }
    }

    [Serializable]
    public class LevelModification
    {
        [SerializeField] private float speedMultiplier = 1.2f;
        [SerializeField] private int inhaleReach = 1;
        public float SpeedMultiplier => speedMultiplier;
    }
}