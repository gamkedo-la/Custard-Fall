using FishNet.Managing.Logging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FishNet.Managing.Scened
{
    /// <summary>
    /// Extensions for SceneLookupData.
    /// </summary>
    internal static class SceneLookupDataExtensions
    {
        /// <summary>
        /// Returns Names from SceneLookupData.
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static string[] GetNames(this SceneLookupData[] datas)
        {
            var names = new string[datas.Length];
            for (var i = 0; i < datas.Length; i++)
                names[i] = datas[i].Name;

            return names;
        }
    }

    /// <summary>
    /// Data container for looking up, loading, or unloading a scene.
    /// </summary>
    public class SceneLookupData
    {
        /// <summary>
        /// Handle of the scene. If value is 0, then handle is not used.
        /// </summary>
        public int Handle = 0;

        /// <summary>
        /// Name of the scene.
        /// </summary>
        public string Name = string.Empty;

        #region Const

        /// <summary>
        /// String to display when scene data is invalid.
        /// </summary>
        private const string INVALID_SCENE =
            "One or more scene information entries contain invalid data and have been skipped.";

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public SceneLookupData()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scene">Scene to generate from.</param>
        public SceneLookupData(Scene scene)
        {
            Handle = scene.handle;
            Name = scene.name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Scene name to generate from.</param>
        public SceneLookupData(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle">Scene handle to generate from.</param>
        public SceneLookupData(int handle)
        {
            Handle = handle;
        }

        #region Comparers.

        public static bool operator ==(SceneLookupData sldA, SceneLookupData sldB)
        {
            //One is null while the other is not.
            if (sldA is null != sldB is null)
                return false;

            /*If here both are either null or have value. */
            if (!(sldA is null))
                return sldA.Equals(sldB);
            else if (!(sldB is null))
                return sldB.Equals(sldA);

            //Fall through indicates both are null.
            return true;
        }

        public static bool operator !=(SceneLookupData sldA, SceneLookupData sldB)
        {
            //One is null while the other is not.
            if (sldA is null != sldB is null)
                return true;

            /*If here both are either null or have value. */
            if (!(sldA is null))
                return !sldA.Equals(sldB);
            else if (!(sldB is null))
                return !sldB.Equals(sldA);

            //Fall through indicates both are null.
            return true;
        }

        public bool Equals(SceneLookupData sld)
        {
            //Comparing instanced against null.
            if (sld is null)
                return false;

            //True if both handles are empty.
            var bothHandlesEmpty = Handle == 0 &&
                                   sld.Handle == 0;

            //If both have handles and they match.
            if (!bothHandlesEmpty && sld.Handle == Handle)
                return true;
            //If neither have handles and name matches.
            else if (bothHandlesEmpty && sld.Name == Name)
                return true;

            //Fall through.
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 2053068273;
            hashCode = hashCode * -1521134295 + Handle.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion

        #region CreateData.

        /// <summary>
        /// Returns a new SceneLookupData.
        /// </summary>
        /// <param name="scene">Scene to create from.</param>
        /// <returns></returns>
        public static SceneLookupData CreateData(Scene scene)
        {
            return new(scene);
        }

        /// <summary>
        /// Returns a new SceneLookupData.
        /// </summary>
        /// <param name="scene">Scene name to create from.</param>
        /// <returns></returns>
        public static SceneLookupData CreateData(string name)
        {
            return new(name);
        }

        /// <summary>
        /// Returns a new SceneLookupData.
        /// </summary>
        /// <param name="scene">Scene handle to create from.</param>
        /// <returns></returns>
        public static SceneLookupData CreateData(int handle)
        {
            return new(handle);
        }

        /// <summary>
        /// Returns a SceneLookupData collection.
        /// </summary>
        /// <param name="scenes">Scenes to create from.</param>
        /// <returns></returns>
        public static SceneLookupData[] CreateData(List<Scene> scenes)
        {
            return CreateData(scenes.ToArray());
        }

        /// <summary>
        /// Returns a SceneLookupData collection.
        /// </summary>
        /// <param name="names">Scene names to create from.</param>
        /// <returns></returns>
        public static SceneLookupData[] CreateData(List<string> names)
        {
            return CreateData(names.ToArray());
        }

        /// <summary>
        /// Returns a SceneLookupData collection.
        /// </summary>
        /// <param name="handles">Scene handles to create from.</param>
        /// <returns></returns>
        public static SceneLookupData[] CreateData(List<int> handles)
        {
            return CreateData(handles.ToArray());
        }

        /// <summary>
        /// Returns a SceneLookupData collection.
        /// </summary>
        /// <param name="scenes">Scenes to create from.</param>
        /// <returns></returns>
        public static SceneLookupData[] CreateData(Scene[] scenes)
        {
            var invalidFound = false;
            var result = new List<SceneLookupData>();
            foreach (var item in scenes)
            {
                if (!item.IsValid())
                {
                    invalidFound = true;
                    continue;
                }

                result.Add(CreateData(item));
            }

            if (invalidFound)
                if (NetworkManager.StaticCanLog(LoggingType.Warning))
                    Debug.LogWarning(INVALID_SCENE);
            return result.ToArray();
        }

        /// <summary>
        /// Returns a SceneLookupData collection.
        /// </summary>
        /// <param name="names">Scene names to create from.</param>
        /// <returns></returns>
        public static SceneLookupData[] CreateData(string[] names)
        {
            var invalidFound = false;
            var result = new List<SceneLookupData>();
            foreach (var item in names)
            {
                if (string.IsNullOrEmpty(item))
                {
                    invalidFound = true;
                    continue;
                }

                result.Add(CreateData(item));
            }

            if (invalidFound)
                if (NetworkManager.StaticCanLog(LoggingType.Warning))
                    Debug.LogWarning(INVALID_SCENE);
            return result.ToArray();
        }

        /// <summary>
        /// Returns a SceneLookupData collection.
        /// </summary>
        /// <param name="handles">Scene handles to create from.</param>
        /// <returns></returns>
        public static SceneLookupData[] CreateData(int[] handles)
        {
            var invalidFound = false;
            var result = new List<SceneLookupData>();
            foreach (var item in handles)
            {
                if (item == 0)
                {
                    invalidFound = true;
                    continue;
                }

                result.Add(CreateData(item));
            }

            if (invalidFound)
                if (NetworkManager.StaticCanLog(LoggingType.Warning))
                    Debug.LogWarning(INVALID_SCENE);
            return result.ToArray();
        }

        #endregion

        /// <summary>
        /// Returns the first scene found using Handle or Name, preferring Handle.
        /// </summary>
        /// <returns></returns>
        /// <param name="foundByHandle">True if scene was found by handle. Handle is always checked first.</param>
        public Scene GetScene(out bool foundByHandle)
        {
            foundByHandle = false;

            if (Handle == 0 && string.IsNullOrEmpty(Name))
            {
                if (NetworkManager.StaticCanLog(LoggingType.Warning))
                    Debug.LogWarning("Scene handle and name is unset; scene cannot be returned.");
                return default;
            }

            Scene result = default;

            //Lookup my handle.
            if (Handle != 0)
            {
                result = SceneManager.GetScene(Handle);
                if (result.handle != 0)
                    foundByHandle = true;
            }

            //If couldnt find handle try by string.
            if (!foundByHandle)
                result = SceneManager.GetScene(Name);

            return result;
        }
    }
}