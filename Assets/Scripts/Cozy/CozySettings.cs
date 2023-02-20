using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "CustardFall/CozySettings")]
public class CozySettings : ScriptableObject
{
    [SerializeField] private AnimationCurve easingFunction;

    public AnimationCurve EasingFunction => easingFunction;
}