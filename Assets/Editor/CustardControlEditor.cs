using System;
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

            if (GUILayout.Button("apply tide level"))
            {
                manager.custardState.GlobalTideLevel = manager.globalCustardLevel;
            }
            
            if (GUILayout.Button("apply with noise"))
            {
                manager.custardState.GlobalTideLevel = manager.globalCustardLevel;
                manager.SeedCustardUpdate((int)Math.Floor(Time.time * 1000));
                manager.SeedCustardUpdate(((int)Math.Floor(Time.time * 1000))/2);
                manager.SeedCustardUpdate(((int)Math.Floor(Time.time * 1000))/4);
            }            
            
            
            var togglePlayPause = new GUIContent(manager.pauseIterationCountDown ? "Play" : "Pause");
            if (GUILayout.Button(togglePlayPause))
            {
                manager.TogglePause();
            }
            
            if (manager.pauseIterationCountDown && GUILayout.Button("Next half step"))
            {
                manager.ForceNextIterationHalfStep();
            }    
        }
    }
}