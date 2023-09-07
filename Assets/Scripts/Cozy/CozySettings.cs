using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "CustardFall/CozySettings")]
public class CozySettings : ScriptableObject
{
    [SerializeField] private AnimationCurve easingFunction;
    [SerializeField] private List<CozyLevel> levels;
    [SerializeField] private int lvl1Offset = 2;

    public AnimationCurve EasingFunction => easingFunction;
    public List<CozyLevel> Levels => levels;

    public Color GetColorForEffectiveLevel(int level)
    {
        var effectiveLevelIdx = level - 1 + lvl1Offset;
        var clampedIdx = Mathf.Min(effectiveLevelIdx,  levels.Count - 1);
        return levels[clampedIdx].Color;
    }

}