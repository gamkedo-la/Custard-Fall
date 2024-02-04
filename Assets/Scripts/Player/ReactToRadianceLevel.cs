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
    private float baseSwimSpeed;

    private void Awake()
    {
        _radianceReceiver = GetComponent<RadianceReceiver>();
        _player = GetComponent<Player>();
        _radianceReceiver.onLevelChange += ReactToChangeInRadiance;
    }

    private void Start()
    {
        baseSpeed = _player.movementSpeed;
        baseSwimSpeed = _player.swimSpeed;
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
            _player.swimSpeed = baseSwimSpeed * modification.SpeedMultiplier;
            Debug.Log(newlevel + " lv: speed mod " + _player.movementSpeed + " swim " + _player.swimSpeed);

            if (modification.InhaleReach == 0)
            {
                _player.inhaler.Size = Inhaler.ConeSize.SMALL;
            }
            else
            {
                _player.inhaler.Size = Inhaler.ConeSize.NORMAL;
            }
            Debug.Log(" inhale cone size:  " + _player.inhaler.Size);
        }
    }

    [Serializable]
    public class LevelModification
    {
        [SerializeField] private float speedMultiplier = 1.2f;
        [SerializeField] private int inhaleReach = 1;
        public float SpeedMultiplier => speedMultiplier;
        public int InhaleReach => inhaleReach;
    }
}