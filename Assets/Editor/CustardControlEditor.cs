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
            
            if (GUILayout.Button("Press to scatter tide activity"))
            {
                manager.custardState.GlobalTideLevel = manager.targetTideLevel;
                manager.SeedCustardUpdate((int)Math.Floor(Time.time * 1000));
                manager.SeedCustardUpdate(((int)Math.Floor(Time.time * 1000))/2);
                manager.SeedCustardUpdate(((int)Math.Floor(Time.time * 1000))/4);
            }
            
            if (GUILayout.Button(manager.custardState.CreationalMode ? "Creation Mode On" : "Creation Mode Off"))
            {
                manager.custardState.CreationalMode = !manager.custardState.CreationalMode;
            }
            
            if (GUILayout.Button(manager.pauseIterationCountDown ? "Play" : "Pause"))
            {
                manager.TogglePause();
            }
            
            if (manager.pauseIterationCountDown && GUILayout.Button("Next Half Step"))
            {
                manager.ForceNextIterationHalfStep();
            }    
        }
    }
}