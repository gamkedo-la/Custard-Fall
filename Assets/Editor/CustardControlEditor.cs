using Custard;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CustardManager))]
    public class CustardControlEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CustardManager manager = (CustardManager) target;

            var togglePlayPause = new GUIContent("Pause");
            if (GUILayout.Button(togglePlayPause))
            {
                var isPaused = manager.TogglePause();
                togglePlayPause.text = isPaused ? "Play" : "Pause";
            }
            
            if (GUILayout.Button("Next half step"))
            {
                manager.ForceNextIterationHalfStep();
            }
        }
    }
}