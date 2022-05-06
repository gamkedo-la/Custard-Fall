﻿using FishNet.Connection;
using FishNet.Documenting;
using FishNet.Managing.Logging;
using FishNet.Object;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FishNet.Observing
{
    /// <summary>
    /// Controls which clients can see and get messages for an object.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkObserver : NetworkBehaviour
    {
        #region Types.

        /// <summary>
        /// How ObserverManager conditions are used.
        /// </summary>
        public enum ConditionOverrideType
        {
            /// <summary>
            /// Keep current conditions, add new conditions from manager.
            /// </summary>
            AddMissing = 1,

            /// <summary>
            /// Replace current conditions with manager conditions.
            /// </summary>
            UseManager = 2,

            /// <summary>
            /// Keep current conditions, ignore manager conditions.
            /// </summary>
            IgnoreManager = 3
        }

        #endregion

        #region Serialized.

        /// <summary>
        /// 
        /// </summary>
        [Tooltip("How ObserverManager conditions are used.")] [SerializeField]
        private ConditionOverrideType _overrideType = ConditionOverrideType.IgnoreManager;

        /// <summary>
        /// How ObserverManager conditions are used.
        /// </summary>
        public ConditionOverrideType OverrideType => _overrideType;

        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Conditions connections must met to be added as an observer. Multiple conditions may be used.")]
        [SerializeField]
        internal List<ObserverCondition> _observerConditions = new();

        /// <summary>
        /// Conditions connections must met to be added as an observer. Multiple conditions may be used.
        /// </summary>
        public IReadOnlyList<ObserverCondition> ObserverConditions => _observerConditions;

        [APIExclude]
#if MIRROR
        public List<ObserverCondition> ObserverConditionsInternal
#else
        internal List<ObserverCondition> ObserverConditionsInternal
#endif
        {
            get => _observerConditions;
            set => _observerConditions = value;
        }

        #endregion

        #region Private.

        /// <summary>
        /// Conditions under this component which are timed.
        /// </summary>
        private List<ObserverCondition> _timedConditions = new();

        /// <summary>
        /// True if all non-timed conditions passed.
        /// </summary>
        private bool _nonTimedMet;

        /// <summary>
        /// NetworkObject this belongs to.
        /// </summary>
        private NetworkObject _networkObject;

        /// <summary>
        /// True if renderers have been populated.
        /// </summary>
        private bool _renderersPopulated;

        /// <summary>
        /// Becomes true when registered with ServerObjects as Timed observers.
        /// </summary>
        private bool _registeredAsTimed;

        /// <summary>
        /// Found renderers on and beneath this object.
        /// </summary>
        private Renderer[] _renderers;

        #endregion

        private void OnEnable()
        {
            if (_networkObject != null && _networkObject.IsServer)
                RegisterTimedConditions();
        }

        private void OnDisable()
        {
            if (_networkObject != null && _networkObject.Deinitializing)
                UnregisterTimedConditions();
        }

        private void OnDestroy()
        {
            if (_networkObject != null)
                UnregisterTimedConditions();
        }

        /// <summary>
        /// Initializes this script for use.
        /// </summary>
        /// <param name="networkManager"></param>
        internal void PreInitialize(NetworkObject networkObject)
        {
            _networkObject = networkObject;

            var observerFound = false;
            for (var i = 0; i < _observerConditions.Count; i++)
                if (_observerConditions[i] != null)
                {
                    observerFound = true;

                    /* Make an instance of each condition so values are
                     * not overwritten when the condition exist more than
                     * once in the scene. Double edged sword of using scriptable
                     * objects for conditions. */
                    _observerConditions[i] = _observerConditions[i].Clone();
                    var oc = _observerConditions[i];
                    oc.InitializeOnce(_networkObject);
                    //If timed also register as containing timed conditions.
                    if (oc.Timed())
                        _timedConditions.Add(oc);
                }
                else
                {
                    _observerConditions.RemoveAt(i);
                    i--;
                }

            //No observers specified 
            if (!observerFound)
            {
                /* Print warning and remove component if not using
                 * IgnoreManager. This is because other overrides would
                 * suggest conditions should be added in someway, but
                 * none are specified.
                 * 
                 * Where-as no conditions with ignore manager would
                 * make sense if the manager had conditions, but you wanted
                 * this object global visible, thus no conditions. */
                if (OverrideType != ConditionOverrideType.IgnoreManager)
                {
                    if (NetworkManager.CanLog(LoggingType.Warning))
                        Debug.LogWarning(
                            $"NetworkObserver exist on {gameObject.name} but there are no observer conditions. This script has been removed.");
                    Destroy(this);
                }

                return;
            }

            RegisterTimedConditions();
        }

        /// <summary>
        /// Returns a condition if found within Conditions.
        /// </summary>
        /// <returns></returns>
        public ObserverCondition GetObserverCondition<T>()
        {
            /* Do not bother setting local variables,
             * condition collections aren't going to be long
             * enough to make doing so worth while. */

            var conditionType = typeof(T);
            for (var i = 0; i < _observerConditions.Count; i++)
                if (_observerConditions[i].GetType() == conditionType)
                    return _observerConditions[i];

            //Fall through, not found.
            return null;
        }

        /// <summary>
        /// Returns ObserverStateChange by comparing conditions for a connection.
        /// </summary>
        /// <returns>True if added to Observers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ObserverStateChange RebuildObservers(NetworkConnection connection, bool timedOnly)
        {
            var currentlyAdded = _networkObject.Observers.Contains(connection);

            //True if all conditions are met.
            var allConditionsMet = true;
            //True if all non timed conditions are met.
            var nonTimedMet = true;
            /* If cnnection is owner then they can see the object. */
            var notOwner = connection != _networkObject.Owner;
            /* If host and connection is the local client for host
             * then do not update visibility for it. This will ensure
             * objects which the host does not own will not be hidden
             * from the host. */
            var notLocalConnection = !(_networkObject.IsHost && connection == _networkObject.LocalConnection);

            /* Only check conditions if not owner. Owner will always
            * have visibility. */
            if (notOwner)
            {
                /* If a timed update and nonTimed
                 * have not been met then there's
                 * no reason to check timed. */
                if (timedOnly && !_nonTimedMet)
                {
                    nonTimedMet = false;
                    allConditionsMet = false;
                }
                else
                {
                    var collection = timedOnly ? _timedConditions : _observerConditions;
                    for (var i = 0; i < collection.Count; i++)
                    {
                        var condition = collection[i];
                        /* If any observer returns removed then break
                         * from loop and return removed. If one observer has
                         * removed then there's no reason to iterate
                         * the rest. */
                        var conditionMet = condition.ConditionMet(connection, currentlyAdded, out var notProcessed);
                        if (notProcessed)
                            conditionMet = currentlyAdded;

                        //Condition not met.
                        if (!conditionMet)
                        {
                            allConditionsMet = false;
                            if (!condition.Timed())
                                nonTimedMet = false;
                            break;
                        }
                    }
                }
            }

            _nonTimedMet = nonTimedMet;

            //If not for the host-client connection.
            if (notLocalConnection)
            {
                //If all conditions met.
                if (allConditionsMet)
                    return ReturnPassedConditions(currentlyAdded);
                else
                    return ReturnFailedCondition(currentlyAdded);
            }
            //Is host-client.
            else
            {
                SetHostRenderers(allConditionsMet);
                return ReturnPassedConditions(currentlyAdded);
            }
        }

        /// <summary>
        /// Registers timed observer conditions.
        /// </summary>
        private void RegisterTimedConditions()
        {
            if (_timedConditions.Count == 0)
                return;
            //Already registered or no timed conditions.
            if (_registeredAsTimed)
                return;

            _registeredAsTimed = true;
            _networkObject.NetworkManager.ServerManager.Objects.AddTimedNetworkObserver(_networkObject);
        }

        /// <summary>
        /// Unregisters timed conditions.
        /// </summary>
        private void UnregisterTimedConditions()
        {
            if (_timedConditions.Count == 0)
                return;
            if (!_registeredAsTimed)
                return;

            _registeredAsTimed = false;
            _networkObject.NetworkManager.ServerManager.Objects.RemoveTimedNetworkObserver(_networkObject);
        }

        /// <summary>
        /// Returns an ObserverStateChange when a condition fails.
        /// </summary>
        /// <param name="currentlyAdded"></param>
        /// <returns></returns>
        private ObserverStateChange ReturnFailedCondition(bool currentlyAdded)
        {
            if (currentlyAdded)
                return ObserverStateChange.Removed;
            else
                return ObserverStateChange.Unchanged;
        }

        /// <summary>
        /// Returns an ObserverStateChange when all conditions pass.
        /// </summary>
        /// <param name="currentlyAdded"></param>
        /// <returns></returns>
        private ObserverStateChange ReturnPassedConditions(bool currentlyAdded)
        {
            if (currentlyAdded)
                return ObserverStateChange.Unchanged;
            else
                return ObserverStateChange.Added;
        }

        /// <summary>
        /// Sets renderers enabled state.
        /// </summary>
        /// <param name="enable"></param>
        private void SetHostRenderers(bool enable)
        {
            if (!_renderersPopulated)
            {
                _renderersPopulated = true;
                _renderers = GetComponentsInChildren<Renderer>(true);
            }

            var count = _renderers.Length;
            for (var i = 0; i < count; i++)
                _renderers[i].enabled = enable;
        }
    }
}