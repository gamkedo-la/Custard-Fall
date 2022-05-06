#if UNITY_EDITOR
using FishNet.Utility.Constant;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FishNet.Editing
{
    public static class Finding
    {
        #region Private.

        /// <summary>
        /// Path where the FishNet.Runtime assembly is.
        /// </summary>
        [System.NonSerialized] private static string _fishNetRuntimePath = string.Empty;

        /// <summary>
        /// Path where the FishNet.Generated assembly is.
        /// </summary>
        private static string _fishNetGeneratedPath = string.Empty;

        #endregion

        /// <summary>
        /// Sets FishNet assembly paths.
        /// </summary>
        /// <param name="error"></param>
        private static void SetPaths(bool error)
        {
            if (_fishNetGeneratedPath != string.Empty && _fishNetRuntimePath != string.Empty)
                return;

            var guids = AssetDatabase.FindAssets("t:asmdef", new string[] {"Assets"});
            var objectPaths = new string[guids.Length];
            for (var i = 0; i < guids.Length; i++)
                objectPaths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);

            var runtimeName = (UtilityConstants.RUNTIME_ASSEMBLY_NAME + ".asmdef").ToLower();
            var generatedName = (UtilityConstants.GENERATED_ASSEMBLY_NAME + ".asmdef").ToLower();
            /* Find all network managers which use Single prefab linking
             * as well all network object prefabs. */
            foreach (var item in objectPaths)
            {
                //Found directory to create object in.
                if (item.ToLower().Contains(runtimeName))
                    _fishNetRuntimePath = System.IO.Path.GetDirectoryName(item);
                else if (item.ToLower().Contains(generatedName))
                    _fishNetGeneratedPath = System.IO.Path.GetDirectoryName(item);

                if (_fishNetGeneratedPath != string.Empty && _fishNetRuntimePath != string.Empty)
                    return;
            }
        }

        /// <summary>
        /// Gets path for where the FishNet.Runtime assembly is.
        /// </summary>
        /// <returns></returns>
        public static string GetFishNetRuntimePath(bool error)
        {
            SetPaths(error);
            return _fishNetRuntimePath;
        }

        /// <summary>
        /// Gets path for where the FishNet.Generated assembly is.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string GetFishNetGeneratedPath(bool error)
        {
            SetPaths(error);
            return _fishNetGeneratedPath;
        }

        /// <summary>
        /// Gets all GameObjects in Assets and optionally scenes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<GameObject> GetGameObjects(bool userAssemblies, bool fishNetAssembly, bool includeScenes,
            string[] ignoredPaths = null)
        {
            var results = new List<GameObject>();

            string[] guids;
            string[] objectPaths;

            guids = AssetDatabase.FindAssets("t:GameObject", null);
            objectPaths = new string[guids.Length];
            for (var i = 0; i < guids.Length; i++)
                objectPaths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);

            foreach (var item in objectPaths)
            {
                var inFishNet = item.Contains(_fishNetRuntimePath);
                if (inFishNet && !fishNetAssembly)
                    continue;
                if (!inFishNet && !userAssemblies)
                    continue;
                if (ignoredPaths != null)
                {
                    var ignore = false;
                    foreach (var path in ignoredPaths)
                        if (item.Contains(path))
                        {
                            ignore = true;
                            break;
                        }

                    if (ignore)
                        continue;
                }

                var go = (GameObject) AssetDatabase.LoadAssetAtPath(item, typeof(GameObject));
                results.Add(go);
            }

            if (includeScenes)
                results.AddRange(GetSceneGameObjects());

            return results;
        }

        /// <summary>
        /// Gets all GameObjects in all open scenes.
        /// </summary>
        /// <returns></returns>
        private static List<GameObject> GetSceneGameObjects()
        {
            var results = new List<GameObject>();

            for (var i = 0; i < SceneManager.sceneCount; i++)
                results.AddRange(GetSceneGameObjects(SceneManager.GetSceneAt(i)));

            return results;
        }

        /// <summary>
        /// Gets all GameObjects in a scene.
        /// </summary>
        private static List<GameObject> GetSceneGameObjects(Scene s)
        {
            var results = new List<GameObject>();
            var buffer = new List<Transform>();
            //Iterate all root objects for the scene.
            var gos = s.GetRootGameObjects();
            for (var i = 0; i < gos.Length; i++)
            {
                /* Get GameObjects within children of each
                 * root object then add them to the cache. */
                gos[i].GetComponentsInChildren<Transform>(true, buffer);
                foreach (var t in buffer)
                    results.Add(t.gameObject);
            }

            return results;
        }


        /// <summary>
        /// Gets created ScriptableObjects of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<UnityEngine.Object> GetScriptableObjects<T>(bool fishNetAssembly, bool breakOnFirst = false)
        {
            var tType = typeof(T);
            var results = new List<UnityEngine.Object>();

            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new string[] {"Assets"});
            var objectPaths = new string[guids.Length];
            for (var i = 0; i < guids.Length; i++)
                objectPaths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);


            /* This might be faster than using directory comparers.
             * Don't really care since this occurs only at edit. */
            var fishNetPaths = new List<string>();
            fishNetPaths.Add(_fishNetGeneratedPath.Replace(@"/", @"\"));
            fishNetPaths.Add(_fishNetGeneratedPath.Replace(@"\", @"/"));
            fishNetPaths.Add(_fishNetRuntimePath.Replace(@"/", @"\"));
            fishNetPaths.Add(_fishNetRuntimePath.Replace(@"\", @"/"));
            /* Find all network managers which use Single prefab linking
             * as well all network object prefabs. */
            foreach (var item in objectPaths)
            {
                //This will skip hidden unity types.
                if (!item.EndsWith(".asset"))
                    continue;
                if (fishNetAssembly)
                {
                    var found = false;
                    foreach (var path in fishNetPaths)
                        if (item.Contains(path))
                        {
                            found = true;
                            break;
                        }

                    if (!found)
                        continue;
                }

                var obj = AssetDatabase.LoadAssetAtPath(item, tType);
                if (obj != null && tType != null && obj.GetType() == tType)
                {
                    results.Add(obj);
                    if (breakOnFirst)
                        return results;
                }
            }

            return results;
        }
    }
}
#endif