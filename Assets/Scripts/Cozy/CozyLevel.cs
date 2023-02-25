using UnityEngine;


    [CreateAssetMenu(menuName = "CustardFall/CozyLevelOfSurrounding")]
    public class CozyLevel : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private Color color;
        [SerializeField] private Color progressColor;

        public string DisplayName => displayName;
        public Color Color => color;
        public Color ProgressColor => progressColor;
        
    }
