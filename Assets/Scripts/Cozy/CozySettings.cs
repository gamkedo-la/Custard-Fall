using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "CustardFall/CozySettings")]
public class CozySettings : ScriptableObject
{
    [SerializeField] private AnimationCurve easingFunction;
    [SerializeField] private List<CozyLevel> levels;

    public AnimationCurve EasingFunction => easingFunction;
    public List<CozyLevel> Levels => levels;

}