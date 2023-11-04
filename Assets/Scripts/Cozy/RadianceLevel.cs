using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(menuName = "CustardFall/RadianceLevel")]
    public class RadianceLevel : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private Color color;
        [SerializeField] private Color progressColor;
        [SerializeField] private Color declineColor;

        public string DisplayName => displayName;
        public Color Color => color;
        public Color ProgressColor => progressColor;
        public Color DeclineColor => declineColor;
        
    }
